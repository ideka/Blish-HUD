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

        private HashSet<string> _queuedReqSet = new HashSet<string>();
        private HashSet<string> _reqSet = new HashSet<string>();

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Meta _meta;
        private readonly string _metaPath;
        private readonly string _dbPath;
        private readonly Dictionary<string, ILoadCollection> _collections = new Dictionary<string, ILoadCollection>();

        private readonly object _reqSetLock = new object();
        private readonly object _metaLock = new object();

        internal IDbAccess GetAccess()
            => new DbAccess(new SQLiteContext(_dbPath, true), this);

        internal void QueueReqSet(HashSet<string> reqSet) {
            lock (_reqSetLock) {
                _queuedReqSet = reqSet;
            }
        }

        internal async Task UpdateCollections() {
            var version = new Version() {
                BuildId = GameService.Gw2Mumble.Info.BuildId,
                Locale = GameService.Overlay.UserLocale.Value,
            };

            // Not ready to update
            if (version.BuildId == 0) {
                return;
            }

            await using var db = new SQLiteContext(_dbPath, false);

            while (true) {
                lock (_reqSetLock) {
                    if (_reqSet.SetEquals(_queuedReqSet)) {
                        break;
                    }

                    _reqSet = _queuedReqSet;
                }

                // Make sure all required collections are up-to-date
                await Task.WhenAll(_reqSet.Select(async name => {
                    if (!_collections.TryGetValue(name, out var collection)) {
                        _logger.Warn($"Attempted to load unknown collection {name}");
                        return;
                    }

                    if (version.IsTheSame(collection.CurrentVersion) || collection.Loading != null) {
                        return;
                    }

                    await collection.Load(db, version, _cts.Token);

                    lock (_metaLock) {
                        if (collection.CurrentVersion is Version newVersion) {
                            _meta.Versions[name] = newVersion;
                        } else {
                            _meta.Versions.Remove(name);
                        }

                        File.WriteAllText(_metaPath, JsonConvert.SerializeObject(_meta));
                    }
                }));

                // Delete all unneeded collections
                await Task.WhenAll(_collections.Select(async kv => {
                    if (_reqSet.Contains(kv.Key)) {
                        return;
                    }

                    await kv.Value.Unload(db, _cts.Token);

                    lock (_metaLock) {
                        _meta.Versions.Remove(kv.Key);
                        File.WriteAllText(_metaPath, JsonConvert.SerializeObject(_meta));
                    }
                }));

                // Vacuum database
                await db.Connection.ExecuteScalarAsync<string>("VACUUM");
            }
        }

        private void BuildIdChanged(object sender, ValueEventArgs<int> e) {
            _ = UpdateCollections();
        }

        public void Dispose() {
            GameService.Gw2Mumble.Info.BuildIdChanged -= BuildIdChanged;
            _cts.Cancel();
        }
    }
}
