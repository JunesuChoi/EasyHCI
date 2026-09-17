using System;
using System.Collections.Generic;
using System.IO;

namespace EasyHCI
{
    /// <summary>
    /// Locates the HCI MemTest executable.
    ///
    /// HCI MemTest is proprietary freeware owned by HCI Design, and embedding it
    /// inside another program needs the author's permission. This fork therefore
    /// ships no copy and extracts none: it uses the memtest.exe the operator
    /// already has. The search order matches how the portable pack lays files out,
    /// so the launcher and the downloaded MemTest folder can sit side by side.
    /// </summary>
    internal static class memtestLocator
    {
        private const string FileName = "memtest.exe";

        // A file next to the launcher lets an operator point at a copy anywhere on
        // disk without rebuilding or editing settings inside the app.
        private const string OverrideFileName = "memtest_path.txt";

        internal static string Find(string appPath)
        {
            foreach (string candidate in EnumerateCandidates(appPath))
            {
                if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate))
                    return candidate;
            }
            return null;
        }

        internal static string DescribeSearch(string appPath)
        {
            var lines = new List<string>();
            foreach (string candidate in EnumerateCandidates(appPath))
                lines.Add("  " + candidate);
            return string.Join(Environment.NewLine, lines.ToArray());
        }

        private static IEnumerable<string> EnumerateCandidates(string appPath)
        {
            // 1. An explicit pointer file wins over every guess.
            string pointer = Path.Combine(appPath, "Resources", OverrideFileName);
            if (File.Exists(pointer))
            {
                string declared = null;
                try { declared = File.ReadAllText(pointer).Trim(); }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
                if (!string.IsNullOrEmpty(declared))
                    yield return declared;
            }

            // 2. Beside the launcher, including the folder older releases used.
            yield return Path.Combine(appPath, "Resources", FileName);
            yield return Path.Combine(appPath, FileName);

            // 3. Walk up towards the pack root and look where the pack stores tools.
            DirectoryInfo directory = null;
            try { directory = new DirectoryInfo(appPath); }
            catch (ArgumentException) { }
            for (int depth = 0; depth < 6 && directory != null; depth++)
            {
                yield return Path.Combine(directory.FullName, "hci-memtest", FileName);

                foreach (string found in FindInToolTree(directory.FullName))
                    yield return found;

                directory = directory.Parent;
            }
        }

        private static IEnumerable<string> FindInToolTree(string root)
        {
            var results = new List<string>();
            string tools = Path.Combine(root, "tools");
            if (!Directory.Exists(tools))
                return results;

            // Only the folders that can hold MemTest are scanned, so a large tool
            // tree does not turn one lookup into a full recursive walk.
            var folders = new List<string>();
            try
            {
                foreach (string level1 in Directory.GetDirectories(tools))
                {
                    folders.Add(level1);
                    foreach (string level2 in Directory.GetDirectories(level1))
                        folders.Add(level2);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            foreach (string folder in folders)
            {
                string name = Path.GetFileName(folder);
                if (name.IndexOf("hci", StringComparison.OrdinalIgnoreCase) < 0 &&
                    name.IndexOf("memtest", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;

                string direct = Path.Combine(folder, FileName);
                if (File.Exists(direct))
                    results.Add(direct);

                try
                {
                    string[] nested = Directory.GetFiles(folder, FileName, SearchOption.AllDirectories);
                    Array.Sort(nested, StringComparer.OrdinalIgnoreCase);
                    results.AddRange(nested);
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }

            return results;
        }
    }
}

