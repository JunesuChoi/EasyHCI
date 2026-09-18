using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace EasyHCI
{
    /// <summary>
    /// Drives the MemTest window without depending on its language.
    ///
    /// The original launcher found every window and button by its Korean caption,
    /// which only ever worked against the Koreanised memtest.exe that upstream
    /// bundled. The free build from HCI Design is English, so none of those lookups
    /// matched: the welcome dialog was never dismissed, the controls were never
    /// found, and the run stopped there. The control ids below are the same in both
    /// builds, so they are what this class uses.
    /// </summary>
    internal static class memtestUi
    {
        internal const int RamEditId = 1000;
        internal const int StartButtonId = 1001;
        internal const int StopButtonId = 1002;
        internal const int StatusStaticId = 1999;
        internal const int DefaultButtonId = 2;

        private const int BmClick = 0x00F5;
        private const string DialogClass = "#32770";

        private delegate bool EnumProc(IntPtr handle, IntPtr parameter);

        [DllImport("user32.dll")] private static extern bool EnumWindows(EnumProc callback, IntPtr parameter);
        [DllImport("user32.dll")] private static extern bool EnumChildWindows(IntPtr parent, EnumProc callback, IntPtr parameter);
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr handle, out uint processId);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetWindowTextW(IntPtr handle, StringBuilder text, int count);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] private static extern int GetClassNameW(IntPtr handle, StringBuilder text, int count);
        [DllImport("user32.dll")] private static extern int GetDlgCtrlID(IntPtr handle);
        [DllImport("user32.dll")] private static extern bool PostMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);

        internal static string WindowText(IntPtr handle)
        {
            var text = new StringBuilder(1024);
            GetWindowTextW(handle, text, text.Capacity);
            return text.ToString();
        }

        private static string ClassName(IntPtr handle)
        {
            var text = new StringBuilder(256);
            GetClassNameW(handle, text, text.Capacity);
            return text.ToString();
        }

        internal static List<IntPtr> TopLevelWindows(uint processId)
        {
            var found = new List<IntPtr>();
            EnumWindows(delegate(IntPtr handle, IntPtr parameter)
            {
                uint owner;
                GetWindowThreadProcessId(handle, out owner);
                if (owner == processId) found.Add(handle);
                return true;
            }, IntPtr.Zero);
            return found;
        }

        internal static List<IntPtr> ChildWindows(IntPtr parent)
        {
            var found = new List<IntPtr>();
            EnumChildWindows(parent, delegate(IntPtr handle, IntPtr parameter)
            {
                found.Add(handle);
                return true;
            }, IntPtr.Zero);
            return found;
        }

        internal static IntPtr ChildById(IntPtr parent, int id)
        {
            if (parent == IntPtr.Zero) return IntPtr.Zero;
            foreach (IntPtr child in ChildWindows(parent))
            {
                if (GetDlgCtrlID(child) == id) return child;
            }
            return IntPtr.Zero;
        }

        /// <summary>The MemTest window itself, identified by its Start button.</summary>
        internal static IntPtr FindMainWindow(uint processId)
        {
            foreach (IntPtr handle in TopLevelWindows(processId))
            {
                if (ClassName(handle) != DialogClass) continue;
                if (ChildById(handle, StartButtonId) != IntPtr.Zero) return handle;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// Any modal dialog the process owns, such as the first-run welcome or the
        /// confirmation shown when a test begins. They are told apart from the main
        /// window by the absence of the Start button.
        /// </summary>
        internal static IntPtr FindModalDialog(uint processId)
        {
            foreach (IntPtr handle in TopLevelWindows(processId))
            {
                if (ClassName(handle) != DialogClass) continue;
                if (ChildById(handle, StartButtonId) != IntPtr.Zero) continue;
                if (ChildById(handle, DefaultButtonId) != IntPtr.Zero) return handle;
            }
            return IntPtr.Zero;
        }

        /// <summary>Presses the dialog's default button without blocking on it.</summary>
        internal static bool ClickDefaultButton(IntPtr dialog)
        {
            if (dialog == IntPtr.Zero) return false;

            IntPtr button = ChildById(dialog, DefaultButtonId);
            if (button == IntPtr.Zero)
            {
                foreach (IntPtr child in ChildWindows(dialog))
                {
                    if (ClassName(child).IndexOf("Button", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        button = child;
                        break;
                    }
                }
            }
            if (button == IntPtr.Zero) return false;

            PostMessage(button, BmClick, IntPtr.Zero, IntPtr.Zero);
            return true;
        }

        internal static void Click(IntPtr control)
        {
            if (control != IntPtr.Zero) PostMessage(control, BmClick, IntPtr.Zero, IntPtr.Zero);
        }

        internal static string StatusText(IntPtr mainWindow)
        {
            return WindowText(ChildById(mainWindow, StatusStaticId));
        }

        /// <summary>
        /// The body text of a dialog. Static labels come first; the fallback keeps
        /// this useful if a build lays the dialog out differently.
        /// </summary>
        internal static string FirstLabelText(IntPtr dialog)
        {
            if (dialog == IntPtr.Zero) return string.Empty;

            string fallback = string.Empty;
            foreach (IntPtr child in ChildWindows(dialog))
            {
                string text = WindowText(child);
                if (string.IsNullOrEmpty(text)) continue;

                if (ClassName(child).IndexOf("Static", StringComparison.OrdinalIgnoreCase) >= 0)
                    return text;

                if (fallback.Length == 0) fallback = text;
            }
            return fallback;
        }

        /// <summary>
        /// A running test reports a coverage percentage, which is the one part of
        /// the status line that reads the same in every language.
        /// </summary>
        internal static bool StatusShowsRunning(string status)
        {
            return !string.IsNullOrEmpty(status) && status.IndexOf('%') >= 0;
        }

        /// <summary>
        /// A refused allocation names the size in megabytes and carries no coverage
        /// readout. The idle label and the "preparing" message match neither.
        /// </summary>
        internal static bool StatusShowsAllocationFailure(string status)
        {
            if (string.IsNullOrEmpty(status)) return false;
            if (StatusShowsRunning(status)) return false;
            return status.IndexOf("MB", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
