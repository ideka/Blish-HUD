using System;
using System.IO;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class LockFile : IDisposable {
        private readonly FileStream _stream;

        private LockFile(FileStream stream) {
            _stream = stream;
        }

        public static LockFile? TryAcquire(string path) {
            try {
                return new LockFile(File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None));
            } catch (IOException e) when (e.GetType() == typeof(IOException)) {
                return null;
            }
        }

        public void Dispose() {
            _stream.Dispose();
        }
    }
}
