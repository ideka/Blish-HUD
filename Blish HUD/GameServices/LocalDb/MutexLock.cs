using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class MutexLock : IDisposable {
        private static readonly Logger _logger = Logger.GetLogger<MutexLock>();

        private static string? _appGuid = null;

        public readonly bool HasHandle = false;
        public readonly bool TimedOut = false;
        public readonly bool Abandoned = false;

        private readonly Mutex _mutex;

        public MutexLock(string name, int timeout = -1)
        {
            {
                _appGuid ??= ((GuidAttribute)Assembly.GetExecutingAssembly()
                   .GetCustomAttributes(typeof(GuidAttribute), false)
                   .GetValue(0)).Value;
                _mutex = new Mutex(false, $"Global\\{{{_appGuid}}}\\{name}");

                var allowEveryoneRule = new MutexAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    MutexRights.FullControl,
                    AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                _mutex.SetAccessControl(securitySettings);
            }

            try {
                HasHandle = _mutex.WaitOne(timeout < 0 ? Timeout.Infinite : timeout, false);

                if (!HasHandle) {
                    TimedOut = true;
                }
            } catch (AbandonedMutexException) {
                _logger.Warn($"Abandoned mutex encountered: {name}");
                HasHandle = true;
                Abandoned = true;
            }
        }

        public void Dispose() {
            if (_mutex == null) {
                return;
            }

            if (HasHandle) {
                _mutex.ReleaseMutex();
            }

            _mutex.Close();
        }
    }
}
