using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal partial class DbHandler {
        private interface ILoadCollection {
            Type IdType { get; }
            string TableName { get; }
            Version? CurrentVersion { get; }
            Version? Loading { get; }

            Task Unload(SQLiteContext db, CancellationToken ct);
            Task Load(SQLiteContext db, Version version, CancellationToken ct);
        }

        private partial class Collection<TId, TItem> : ILoadCollection, IMetaCollection
            where TId : notnull
            where TItem : class {
            private static readonly Logger _logger = Logger.GetLogger<Collection<TId, TItem>>();

            public event Action? Loaded;

            public Type IdType { get; } = typeof(TId);
            public string TableName { get; }
            public Version? CurrentVersion => _getVersion();

            public Version? Loading { get; private set; }
            public Exception? Exception { get; private set; }

            public bool IsAvailable => CurrentVersion.HasValue;
            public bool IsFaulted => Exception != null;

            private readonly Func<Version?> _getVersion;
            private readonly Func<CancellationToken, Task<IEnumerable<(TId id, TItem item)>>> _load;

            public Collection(
                string tableName,
                Func<Version?> getVersion,
                Func<CancellationToken, Task<IEnumerable<(TId id, TItem item)>>> load) {

                TableName = tableName;
                _getVersion = getVersion;
                _load = load;
            }

            public Task WaitUntilLoaded() {
                var tcs = new TaskCompletionSource<object>();

                Loaded += loaded;

                void loaded() {
                    Loaded -= loaded;
                    tcs.SetResult(null!);
                }

                return IsAvailable
                    ? Task.CompletedTask
                    : IsFaulted && !Loading.HasValue
                    ? Task.CompletedTask
                    : tcs.Task;
            }

            public IDbCollection<TId, TItem> Access(SQLiteAsyncConnection db)
                => new LocalCollection<TId, TItem>(db, TableName);

            public async Task Unload(SQLiteContext db, CancellationToken _) {
                await db.Connection.ExecuteAsync($"DELETE FROM `{TableName}`");
            }

            public async Task Load(SQLiteContext db, Version version, CancellationToken ct) {
                Loading = version;

                try {
                    var values = await _load(ct);
                    await db.Connection.RunInTransactionAsync(transaction => {
                        transaction.ExecuteScalar<string>("PRAGMA locking_mode = EXCLUSIVE");
                        transaction.ExecuteScalar<string>("PRAGMA journal_mode = MEMORY");

                        // Delete all content from table first
                        transaction.Execute($"DELETE FROM `{TableName}`");

                        // Insert all items into the table
                        foreach (var (id, value) in values.OrderBy(x => x.id)) {
                            transaction.Execute(
                                $"INSERT INTO `{TableName}` ({SQLiteContext.ID_COLUMN}, {SQLiteContext.DATA_COLUMN})\n" +
                                $"VALUES (?, jsonb(?))", id, SQLiteContext.Serialize(value));
                        }
                    });

                    Exception = null;
                } catch (Exception e) {
                    _logger.Warn(e, $"Failed to load cache for {TableName}");
                    Exception = e;
                } finally {
                    Loading = null;
                    Loaded?.Invoke();
                }
            }
        }
    }
}
