﻿using System;
using System.Runtime.InteropServices;

namespace SnaffCore
{
    public class Impersonator
    {
        private static IntPtr _userHandle = IntPtr.Zero;
        private static string _username = string.Empty;

        public static bool Login(string domain, string username, string password)
        {
            if (_userHandle != IntPtr.Zero)
            {
                return true;
            }

            _username = username;
            return LogonUser(username, domain, password, 2, 0, ref _userHandle);
        }

        public static bool StartImpersonating()
        {
            if (_userHandle == IntPtr.Zero)
            {
                return true;
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

        public static string GetUsername()
        {
            return _username;
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

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool ImpersonateLoggedOnUser(IntPtr hToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        private static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool CloseHandle(IntPtr handle);
    }
}
