using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace Blish_HUD.LocalDb {
    public interface IDbCollection {
        Task<int> CountAsync();
        Task<long> LongCountAsync();
    }

    public interface IDbCollection<TId, TItem> : IDbCollection
            where TId : notnull
            where TItem : class {
        Task<TItem?> GetAsync(TId id);
        Task<IEnumerable<TItem>> AllAsync();
        Task<IEnumerable<TItem>> JsonQueryAsync(string field, object value);
    }

    public interface IMetaCollection {
        event Action? Loaded;
        bool IsAvailable { get; }
        Task WaitUntilLoaded();
    }

    internal partial class DbHandler {
        private class LocalCollection<TId, TItem> : IDbCollection<TId, TItem>
            where TId : notnull
            where TItem : class {
            private readonly SQLiteAsyncConnection _db;
            private readonly string _tableName;

            public LocalCollection(SQLiteAsyncConnection db, string tableName) {
                _db = db;
                _tableName = tableName;
            }

            private async Task<IEnumerable<TItem>> SelectWhere(string where, params object[] args) {
                var json = await _db.QueryScalarsAsync<string>(
                    $"SELECT json({SQLiteContext.DATA_COLUMN})\n" +
                    $"FROM `{_tableName}`\n" +
                    $"WHERE {where}", args);
                return json.Select(SQLiteContext.Deserialize<TItem>);
            }

            public async Task<int> CountAsync()
                => await _db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM `{_tableName}`");

            public async Task<long> LongCountAsync()
                => await _db.ExecuteScalarAsync<long>($"SELECT COUNT(*) FROM `{_tableName}`");

            public async Task<TItem?> GetAsync(TId id)
                => (await SelectWhere($"{SQLiteContext.ID_COLUMN} = ?", id)).FirstOrDefault();

            public async Task<IEnumerable<TItem>> AllAsync()
                => await SelectWhere("1 = 1");

            public async Task<IEnumerable<TItem>> JsonQueryAsync(string path, object value)
                => await SelectWhere($"json_extract({SQLiteContext.DATA_COLUMN}, '$.{path}') LIKE ?", value);
        }
    }
}
