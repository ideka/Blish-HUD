using Gw2Sharp.WebApi;
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

        private class Meta {
            public Dictionary<string, Version> Versions { get; set; } = new Dictionary<string, Version>();
        }

        private /*readonly record*/ struct Version {
            public int BuildId { get; set; }
            public Locale Locale { get; set; }

            public readonly bool IsTheSame(Version? other) {
                return BuildId == other?.BuildId && Locale == other?.Locale;
            }
        }

        internal Locale? ForcedLocale { get; set; }

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Meta _meta;
        private readonly string _metaPath;
        private readonly string _dbPath;
        private readonly Dictionary<string, ILoadCollection> _collections = new Dictionary<string, ILoadCollection>();

        private readonly object _lock = new object();

        internal IDbAccess GetAccess()
            => new DbAccess(new SQLiteContext(_dbPath, true), this);

        internal int CountMismatchedLocaleCollections() {
            lock (_lock) {
                return _meta.Versions.Count(x => x.Value.Locale != GameService.Overlay.UserLocale.Value);
            }
        }

        internal async Task UpdateCollections() {
            int buildId = GameService.Gw2Mumble.Info.BuildId;

            // Not ready to update
            if (buildId == 0) {
                return;
            }

            await using var db = new SQLiteContext(_dbPath, false);

            do {
                // Make sure all collections are up-to-date
                await Task.WhenAll(_collections.Select(async kv => {
                    string name = kv.Key;
                    ILoadCollection collection = kv.Value;

                    // Collection is already loading, skip
                    if (collection.Loading != null) {
                        return;
                    }

                    // BuildID already matches and the locale either also matches or isn't forced, skip
                    if (collection.CurrentVersion?.BuildId == buildId &&
                        (collection.CurrentVersion?.Locale == GameService.Overlay.UserLocale.Value ||
                        GameService.Overlay.UserLocale.Value != ForcedLocale)) {
                        return;
                    }

                    await collection.Load(db, new Version() {
                        BuildId = buildId,
                        Locale = GameService.Overlay.UserLocale.Value,
                    }, _cts.Token);

                    lock (_lock) {
                        if (collection.CurrentVersion is Version newVersion) {
                            _meta.Versions[name] = newVersion;
                        } else {
                            _meta.Versions.Remove(name);
                        }

                        File.WriteAllText(_metaPath, JsonConvert.SerializeObject(_meta));
                    }
                }));

                // If the current locale is forced and there's still collections with a different locale, go again.
            } while (GameService.Overlay.UserLocale.Value == ForcedLocale && CountMismatchedLocaleCollections() > 0);

            // Vacuum database
            await db.Connection.ExecuteScalarAsync<string>("VACUUM");
        }

        public void Dispose() {
            _cts.Cancel();
        }
    }
}
