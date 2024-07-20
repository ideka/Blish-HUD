﻿using Blish_HUD.LocalDb;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;
using System.IO;

#nullable enable

namespace Blish_HUD {
    public class LocalDbService : GameService {

        private const string DIRECTORY_NAME = "localdb/";
        private const string META_FILENAME = "meta.json";
        private const string DATABASE_FILENAME = "localdb.sqlite";

        public IDbMeta Meta => _handler;

        private string _basePath = null!;
        private DbHandler _handler = null!;

        public IDbAccess GetAccess() {
            return _handler.GetAccess();
        }

        protected override void Initialize() {
            _basePath = DirectoryUtil.RegisterDirectory(Path.Combine(DirectoryUtil.CachePath, DIRECTORY_NAME));
        }

        protected override void Load() {
            _handler = new DbHandler(
                Path.Combine(_basePath, META_FILENAME),
                Path.Combine(_basePath, DATABASE_FILENAME));

            UpdateCollections();

            Gw2Mumble.Info.BuildIdChanged += BuildIdChanged;
        }

        protected override void Update(GameTime gameTime) { /* NOOP */ }

        protected override void Unload() {
            Gw2Mumble.Info.BuildIdChanged -= BuildIdChanged;

            _handler.Dispose();
        }

        internal void UpdateCollections() {
            _ = _handler.UpdateCollections();
        }

        internal int CountMismatchedLocaleCollections(Locale locale) {
            return _handler.CountMismatchedLocaleCollections(locale);
        }

        internal void ForceLocale(Locale locale) {
            _handler.ForcedLocale = locale;
            UpdateCollections();
        }

        internal bool CollectionExists(string name) {
            return _handler.GetCollection(name) != null;
        }

        private void BuildIdChanged(object sender, ValueEventArgs<int> e) {
            UpdateCollections();
        }
    }
}
