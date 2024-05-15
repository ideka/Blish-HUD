using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal partial class DbHandler {
        private class SQLiteContext : IDisposable, IAsyncDisposable {
            private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings() {
                TypeNameHandling = TypeNameHandling.Auto,
                Converters = new List<JsonConverter>
                {
                    new ApiEnumConverter(),
                    new ApiFlagsConverter(),
                    new Coordinates2Converter(),
                    new Coordinates3Converter(),
                    new NullableRenderUrlConverter(),
                    new RectangleConverter(),
                    new RenderUrlConverter(),
                },
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver() {
                    NamingStrategy = new SnakeCaseNamingStrategy(),
                },
            };

            public SQLiteAsyncConnection Connection { get; }

            public const string ID_COLUMN = "id";
            public const string DATA_COLUMN = "data";

            public SQLiteContext(string dbPath, bool readOnly) {
                this.Connection = new SQLiteAsyncConnection(dbPath,
                    readOnly
                        ? SQLiteOpenFlags.ReadOnly
                        : SQLiteOpenFlags.ReadWrite);
            }

            public static void Create(string dbPath, IEnumerable<ILoadCollection> collections) {
                using var connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

                foreach (var collection in collections) {
                    // TODO: Use BLOB and byte[] for Guid?
                    bool isText = Type.GetTypeCode(collection.IdType) switch {
                        TypeCode.Int32 => false,
                        TypeCode.String => true,
                        _ => throw new Exception($"Unsupported key type: {collection.IdType}"),
                    };

                    // Use INTEGER for int and TEXT for string
                    // Use WITHOUT ROWID for string id tables (see <https://www.sqlite.org/lang_createtable.html#rowid>)
                    connection.Execute(
                        $"CREATE TABLE IF NOT EXISTS `{collection.TableName}`\n (" +
                        $"{ID_COLUMN} {(isText ? "TEXT" : "INTEGER")} PRIMARY KEY NOT NULL,\n" +
                        $"{DATA_COLUMN} BLOB NOT NULL\n" +
                        $") {(isText ? "WITHOUT ROWID" : "")}");
                }
            }

            public static string Serialize<TItem>(TItem item)
                => JsonConvert.SerializeObject(item, typeof(TItem), _jsonSerializerSettings);

            public static T Deserialize<T>(string data)
                => JsonConvert.DeserializeObject<T>(data, _jsonSerializerSettings) ?? throw new Exception();

            public void Dispose() => DisposeAsync().AsTask().Wait();

            public async ValueTask DisposeAsync() => await Connection.CloseAsync();
        }
    }
}
