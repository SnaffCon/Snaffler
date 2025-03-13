using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SnaffCore.Config;

namespace SnaffCore
{
    public class Impersonator
    {
        private static IntPtr _userHandle = IntPtr.Zero;

        public static bool Login(string domain, string username, string password)
        {
            if (_userHandle != IntPtr.Zero)
            {
                return true;
            }

            return LogonUser(username, domain, password, 2, 0, ref _userHandle);
        }

        public static bool StartImpersonating()
        {
            if (_userHandle == IntPtr.Zero)
            {
                return false;
            }

            return ImpersonateLoggedOnUser(_userHandle);
        }

        public static bool StopImpersonating()
        {
            if (_userHandle == IntPtr.Zero)
            {
                return true;
            }

            return RevertToSelf();
        }

        public static bool Free()
        {
            if (_userHandle == IntPtr.Zero)
            {
                return true;
            }

            return CloseHandle(_userHandle);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            ref IntPtr phToken
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
        private static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}
