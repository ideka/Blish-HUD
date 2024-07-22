﻿using Gw2Sharp.WebApi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using File = System.IO.File;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal partial class DbHandler : IDisposable {
        private static readonly Logger _logger = Logger.GetLogger<DbHandler>();

        private const string MUTEX_NAME = "DbHandler.Meta";

        private class Meta {
            public Dictionary<string, Version> Versions { get; set; } = new Dictionary<string, Version>();

            public Version? GetVersion(string name) {
                return Versions.TryGetValue(name, out var version) ? version : (Version?)null;
            }
        }

        private /*readonly record*/ struct Version {
            public int BuildId { get; set; }
            public Locale Locale { get; set; }
            public bool IsFaulted { get; set; }

            [JsonIgnore]
            public readonly bool IsValid => BuildId > 0;
        }

        internal Locale? ForcedLocale { get; set; }
        internal event Action<IMetaCollection>? CollectionLoaded;

        private Meta _meta;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly string _metaPath;
        private readonly string _dbPath;
        private readonly string _lockPath;
        private readonly Dictionary<string, ILoadCollection> _collections = new Dictionary<string, ILoadCollection>();

        private void ReloadMeta(MutexLock @lock) {
            if (!@lock.HasHandle) {
                return;
            }

            if (!File.Exists(_metaPath)) {
                WriteMeta(@lock);
                return;
            }

            var notLoaded = _collections.Values.Where(x => !x.IsLoaded).ToList();

            try {
                var meta = JsonConvert.DeserializeObject<Meta>(File.ReadAllText(_metaPath));
                if (meta == null) {
                    _logger.Warn("Cache meta load resulted empty or null, recreating...");
                    WriteMeta(@lock);
                } else {
                    _meta = meta;
                }
            } catch (Exception e) {
                _logger.Warn(e, "Exception when loading LocalDb cache meta.");
            }

            foreach (var collection in notLoaded) {
                if (collection.IsLoaded) {
                    CollectionLoaded?.Invoke(collection);
                }
            }
        }

        private void WriteMeta(MutexLock @lock) {
            if (!@lock.HasHandle) {
                return;
            }
            File.WriteAllText(_metaPath, JsonConvert.SerializeObject(_meta));
        }

        internal IDbAccess GetAccess()
            => new DbAccess(new SQLiteContext(_dbPath, true), this);

        internal int CountMismatchedLocaleCollections() {
            using (new MutexLock(MUTEX_NAME)) {
                return _meta.Versions.Count(x => x.Value.Locale != GameService.Overlay.UserLocale.Value);
            }
        }

        internal async Task UpdateCollections() {
            int buildId = GameService.Gw2Mumble.Info.BuildId;

            // Not ready to update
            if (buildId == 0) {
                return;
            }

            // Check if someone else (potentially a different BlishHUD process) is already updating the database
            using var lockFile = LockFile.TryAcquire(_lockPath);
            if (lockFile == null) {
                return;
            }

            await using var db = new SQLiteContext(_dbPath, false);

            do {
                using (var @lock = new MutexLock(MUTEX_NAME)) {
                    ReloadMeta(@lock);
                }

                // Make sure all collections are up-to-date
                await Task.WhenAll(_collections.Select(async kv => {
                    string name = kv.Key;
                    ILoadCollection collection = kv.Value;

                    // BuildID already matches and the locale either also matches or isn't forced, skip
                    if (collection.CurrentVersion?.BuildId == buildId &&
                        (collection.CurrentVersion?.Locale == GameService.Overlay.UserLocale.Value ||
                        GameService.Overlay.UserLocale.Value != ForcedLocale)) {
                        return;
                    }

                    bool wasLoaded = collection.IsLoaded;
                    bool success = await collection.Load(db, _cts.Token);

                    using (new MutexLock(MUTEX_NAME)) {
                        if (success) {
                            _meta.Versions[name] = new Version() {
                                BuildId = buildId,
                                Locale = GameService.Overlay.UserLocale.Value,
                                IsFaulted = false,
                            };
                        } else if (!_meta.Versions.ContainsKey(name)) {
                            _meta.Versions[name] = new Version() {
                                BuildId = -1,
                                Locale = GameService.Overlay.UserLocale.Value,
                                IsFaulted = true,
                            };
                        }
                        File.WriteAllText(_metaPath, JsonConvert.SerializeObject(_meta));
                    }

                    if (!wasLoaded && collection.IsLoaded) {
                        CollectionLoaded?.Invoke(collection);
                    }
                }));

                _cts.Token.ThrowIfCancellationRequested();

                // If the current locale is forced and there's still collections with a different locale, go again
            } while (GameService.Overlay.UserLocale.Value == ForcedLocale && CountMismatchedLocaleCollections() > 0);

            // Reset forced locale once everything has been updated
            ForcedLocale = null;

            _cts.Token.ThrowIfCancellationRequested();

            // Vacuum database
            await db.Connection.ExecuteScalarAsync<string>("VACUUM");
        }

        private async Task MetaReloader() {
            const int MIN_WAIT = 3;
            int wait = MIN_WAIT;

            while (true) {
                await Task.Delay(TimeSpan.FromSeconds(wait), _cts.Token);

                using var @lock = new MutexLock(MUTEX_NAME, TimeSpan.Zero);
                if (@lock.TimedOut) {
                    wait *= 2;
                    continue;
                }

                if (_collections.Values.All(x => x.IsLoaded)) {
                    break;
                }

                wait = MIN_WAIT;

                ReloadMeta(@lock);
            }
        }

        public void Dispose() {
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
