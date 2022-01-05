using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    class PathUtils
    {
        private static string ASSETS_FOLDER_NAME => "Assets";
        private static string PACKAGES_FOLDER_NAME => "Packages";

        internal static bool IsInternalPath(string path)
        {
            string[] folders = path.Split(Path.DirectorySeparatorChar);
            return folders.Contains(ASSETS_FOLDER_NAME);
        }

        internal static bool IsPathRelativeToAssets(string path) =>
            GetRootFolder(path).Equals(ASSETS_FOLDER_NAME, StringComparison.Ordinal);

        internal static bool IsPathRelativeToPackages(string path) =>
            GetRootFolder(path).Equals(PACKAGES_FOLDER_NAME, StringComparison.Ordinal);

        internal static bool IsPathRelativeToProject(string path)
        {
            string projectFolderName = Directory.GetParent(Application.dataPath).Name;
            return GetRootFolder(path).Equals(projectFolderName, StringComparison.Ordinal);
        }

        internal static bool IsPathRelativeToSkins(string path)
        {
            string skinFolderName = "Skins";
            return GetRootFolder(path).Equals(skinFolderName, StringComparison.Ordinal);
        }

        internal static string GetRootFolder(string path)
        {
            while (true)
            {
                string temp = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(temp))
                {
                    break;
                }

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

        internal static string GetPathRelativeToRepositoryFolder(string path)
        {
            string fullPath = GetFullPath(path);
            string projectFolderName = Directory.GetParent(GitUtils.RepositoryPath).Name;

            return GetRelativeTo(fullPath, projectFolderName, false);
        }

        private static string GetRelativeTo(string path, string relativeTo, bool includeRelativeToInPath = true)
        {
            List<string> split = path.Split(Path.DirectorySeparatorChar).ToList();

            if (!split.Contains(relativeTo))
            {
                return string.Empty;
            }

            int indexOfRelativeTo = split.IndexOf(relativeTo);
            if (includeRelativeToInPath)
            {
                return string.Join(
                    Path.DirectorySeparatorChar.ToString(),
                    split.GetRange(indexOfRelativeTo, split.Count - indexOfRelativeTo)
                );
            }
            else
            {
                return string.Join(
                    Path.DirectorySeparatorChar.ToString(),
                    split.GetRange(indexOfRelativeTo + 1, split.Count - indexOfRelativeTo - 1)
                );
            }
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

            if (IsPathRelativeToSkins(path))
            {
                return Path.Combine(ShapeShifter.SkinsFolder.Parent.FullName, path);
            }

            return string.Empty;
        }

        internal static int GetAssetCountInFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(GetPathRelativeToAssetsFolder(path)))
            {
                Debug.LogError("Trying to count number of assets inside path that is not a folder.");
                return 0;
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(GetFullPath(path));

            FileInfo[] files = directoryInfo.GetFiles();
            int count = 0;
            for (int index = 0; index < files.Length; index++)
            {
                FileInfo fileInfo = files[index];
                if (fileInfo.Extension == ".meta")
                {
                    continue;
                }

                count++;
            }

            return count;
        }
    }
}