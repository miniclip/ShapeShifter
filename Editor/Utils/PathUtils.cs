using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    class PathUtils
    {
        private static string ASSETS_FOLDER_NAME => "Assets";

        private static bool IsInternalPath(string path)
        {
            string[] folders = path.Split(Path.DirectorySeparatorChar);
            return folders.Contains(ASSETS_FOLDER_NAME);
        }

        private static bool IsPathRelativeToAssets(string path)
        {
            return GetRootFolder(path).Equals(ASSETS_FOLDER_NAME, StringComparison.Ordinal);
        }

        public static bool IsFullPath(string path) => Path.IsPathRooted(path);

        private static string GetRootFolder(string path)
        {
            while (true)
            {
                string temp = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(temp))
                    break;
                path = temp;
            }

            return path;
        }

        internal static string GetPathRelativeToAssetsFolder(string path)
        {
            if (!IsInternalPath(path))
            {
                Debug.LogError($"Path is not from inside {ASSETS_FOLDER_NAME}");
                return string.Empty;
            }

            if (IsPathRelativeToAssets(path))
            {
                return path;
            }

            return GetRelativeTo(path, ASSETS_FOLDER_NAME);
        }
        
        internal static string GetPathRelativeToProjectFolder(string path)
        {
            string fullPath = GetFullPath(path);
            string projectFolderName = Directory.GetParent(Application.dataPath).Name;

            return GetRelativeTo(fullPath, projectFolderName);
        }

        private static string GetRelativeTo(string path, string relativeTo)
        {
            List<string> split = path.Split(Path.DirectorySeparatorChar).ToList();

            if (!split.Contains(relativeTo))
            {
                Debug.LogError($"Path {path} does not contain {relativeTo}");
                return string.Empty;
            }

            int indexOfRelativeTo = split.IndexOf(relativeTo);
            string result = string.Join(
                Path.DirectorySeparatorChar.ToString(),
                split.GetRange(indexOfRelativeTo, split.Count - indexOfRelativeTo)
            );

            return result;
        }

        internal static string GetFullPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                return path;
            }

            if (IsPathRelativeToAssets(path))
            {
                return Path.GetFullPath(path);
            }

            return string.Empty;
        }
    }
}