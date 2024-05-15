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

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly Meta _meta;
        private readonly string _metaPath;
        private readonly string _dbPath;
        private readonly Dictionary<string, ILoadCollection> _collections = new Dictionary<string, ILoadCollection>();

        private readonly object _lock = new object();

        public IDbAccess GetContext()
            => new DbAccess(new SQLiteContext(_dbPath, true), this);

        private async Task UpdateCollections() {
            var version = new Version() {
                BuildId = GameService.Gw2Mumble.Info.BuildId,
                Locale = GameService.Overlay.UserLocale.Value,
            };

            // Not ready to update
            if (version.BuildId == 0) {
                return;
            }

            await using var db = new SQLiteContext(_dbPath, false);

            await Task.WhenAll(_collections.Select(async kv => {
                if (version.IsTheSame(kv.Value.CurrentVersion) || kv.Value.Loading != null) {
                    return;
                }

                await kv.Value.Load(db, version, _cts.Token);

                lock (_lock) {
                    if (kv.Value.CurrentVersion is Version newVersion) {
                        _meta.Versions[kv.Key] = newVersion;
                    } else {
                        _meta.Versions.Remove(kv.Key);
                    }

                    File.WriteAllText(_metaPath, JsonConvert.SerializeObject(_meta));
                }
            }));

            // TODO: Clear unneeded collections
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
