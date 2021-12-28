using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public class PathUtils
    {
        private static string ASSETS_FOLDER_NAME => "Assets";
        private static string PACKAGES_FOLDER_NAME => "Packages";

        private static bool IsInternalPath(string path)
        {
            string[] folders = path.Split(Path.DirectorySeparatorChar);
            return folders.Contains(ASSETS_FOLDER_NAME);
        }

        public static bool IsPathRelativeToAssets(string path)
        {
            return GetRootFolder(path).Equals(ASSETS_FOLDER_NAME, StringComparison.Ordinal);
        }

        public static bool IsPathRelativeToPackages(string path)
        {
            return GetRootFolder(path).Equals(PACKAGES_FOLDER_NAME, StringComparison.Ordinal);
        }
        
        private static bool IsPathRelativeToProject(string path)
        {
            string projectFolderName = Directory.GetParent(Application.dataPath).Name;
            return GetRootFolder(path).Equals(projectFolderName, StringComparison.Ordinal);
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

            if (IsPathRelativeToAssets(path) || IsPathRelativeToPackages(path))
            {
                return Path.GetFullPath(path);
            }

            if (IsPathRelativeToProject(path))
            {
                string projectContainerFolderName = Directory.GetParent(Application.dataPath).Parent.FullName;
                
                return Path.Combine(projectContainerFolderName, path);
            }
            
            return string.Empty;
        }
        
    }
}