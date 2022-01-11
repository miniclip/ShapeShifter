using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Miniclip.ShapeShifter
{
    static class FileSystemWatcherManager
    {
        private static readonly Dictionary<string, FileSystemWatcher> pathToFileSystemWatcherDict =
            new Dictionary<string, FileSystemWatcher>();

        internal static void AddPathToWatchlist(string path, FileSystemEventHandler fileChangedCallback)
        {
            if (pathToFileSystemWatcherDict.ContainsKey(path))
            {
                return;
            }

            FileSystemWatcher fileWatcher = new FileSystemWatcher(path);
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.EnableRaisingEvents = true;
            fileWatcher.Changed += fileChangedCallback;
            pathToFileSystemWatcherDict.Add(path, fileWatcher);
        }

        internal static void RemovePathFromWatchlist(string path)
        {
            if (!pathToFileSystemWatcherDict.ContainsKey(path))
            {
                return;
            }

            FileSystemWatcher watcher = pathToFileSystemWatcherDict[path];
            watcher.EnableRaisingEvents = false;
            pathToFileSystemWatcherDict[path] = null;
            pathToFileSystemWatcherDict.Remove(path);
        }

        internal static void RemoveAllPathsFromWatchlist()
        {
            List<string> keyCollection = pathToFileSystemWatcherDict.Keys.ToList();
            for (int index = 0; index < keyCollection.Count; index++)
            {
                string key = keyCollection[index];
                RemovePathFromWatchlist(key);
            }
        }
    }
}