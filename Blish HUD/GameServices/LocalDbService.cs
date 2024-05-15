using Microsoft.Xna.Framework;
using Blish_HUD.LocalDb;
using System.IO;

#nullable enable

namespace Blish_HUD {
    public class LocalDbService : GameService {

        private const string DIRECTORY_NAME = "localdb";
        private const string META_FILENAME = "meta.json";
        private const string DATABASE_FILENAME = "localdb.sqlite";

        private string _basePath = null!;
        private DbHandler _handler = null!;

        protected override void Initialize() {
            _basePath = Path.Combine(DirectoryUtil.BasePath, DIRECTORY_NAME);
            Directory.CreateDirectory(_basePath);
        }

        protected override void Load() {
            _handler = new DbHandler(
                Path.Combine(_basePath, META_FILENAME),
                Path.Combine(_basePath, DATABASE_FILENAME));
        }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

        protected override void Unload() {
            _handler.Dispose();
        }
    }
}
