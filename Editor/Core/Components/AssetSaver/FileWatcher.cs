using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;

namespace Miniclip.ShapeShifter.Saver
{
    static class FileWatcher
    {
        private static readonly Dictionary<string, FileSystemWatcher> pathToFileSystemWatcherDict =
            new Dictionary<string, FileSystemWatcher>();

        private static void AddPathToWatchlist(string path, FileSystemEventHandler fileChangedCallback)
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

        private static void RemovePathFromWatchlist(string path)
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

        internal static void StartWatchingFolder(string pathToWatch)
        {
            AddPathToWatchlist(pathToWatch, OnFileChanged);
        }

        internal static void StopWatchingFolder(string pathToUnwatch)
        {
            RemovePathFromWatchlist(pathToUnwatch);
        }

        private static void OnFileChanged(object sender, FileSystemEventArgs args)
        {
            DirectoryInfo assetDirectory = new DirectoryInfo(Path.GetDirectoryName(args.FullPath));
            string game = assetDirectory.Parent.Parent.Name;
            string guid = assetDirectory.Name;
            string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);

            ShapeShifter.DirtyAssets.Add(key);
        }
    }
}