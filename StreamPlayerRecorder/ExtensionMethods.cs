using System;
using System.Runtime.InteropServices;

namespace ExtensionMethods
{
    public static class ConsoleEx
    {
        public class Remove
        {
            public static bool CloseButton()
            {
                DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
                return true;
            }

            public static bool MinimizeButton()
            {
                DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
                return true;
            }

            public static bool MaximizeButton()
            {
                DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
                return true;
            }
        }

        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;
        private const int SC_MINIMIZE = 0xF020;
        private const int SC_MAXIMIZE = 0xF030;

        [DllImport("user32.dll")]
        private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();
    }
}
