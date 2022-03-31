using System;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Switcher
{
    public static class AssetSwitcherOperations
    {
        public static void CopyFromOriginToSkinnedExternal(DirectoryInfo directory)
        {
            string relativePath = ExternalAssetSkinner.GenerateRelativePathFromKey(directory.Name);
            string origin = Path.Combine(Application.dataPath, relativePath);
            string target = Path.Combine(directory.FullName, Path.GetFileName(origin));
            FileUtils.SafeCopy(origin, target);
        }

        public static void CopyFromSkinnedExternalToOrigin(DirectoryInfo directory)
        {
            string relativePath = ExternalAssetSkinner.GenerateRelativePathFromKey(directory.Name);
            string target = Path.Combine(Application.dataPath, relativePath);
            string searchPattern = Path.GetFileName(target);
            FileInfo[] fileInfos = directory.GetFiles(searchPattern);

            if (fileInfos.Length <= 0)
            {
                return;
            }

            FileInfo origin = fileInfos[0];
            FileUtils.SafeCopy(origin.FullName, target);
        }

        public static void PerformCopiesWithTracking(GameSkin selected,
            string description,
            Action<DirectoryInfo> internalAssetOperation,
            Action<DirectoryInfo> externalAssetOperation)
        {
            ShapeShifterLogger.Log($"{description}: {selected.Name}");

            string gameFolderPath = selected.MainFolderPath;

            if (Directory.Exists(gameFolderPath))
            {
                int totalDirectories = Directory.EnumerateDirectories(
                        gameFolderPath,
                        "*",
                        SearchOption.AllDirectories
                    )
                    .Count();

                float progress = 0.0f;
                float progressBarStep = 1.0f / totalDirectories;

                PerformOperationOnPath(
                    gameFolderPath,
                    ShapeShifterConstants.INTERNAL_ASSETS_FOLDER,
                    internalAssetOperation,
                    description,
                    progressBarStep,
                    ref progress
                );

                PerformOperationOnPath(
                    gameFolderPath,
                    ShapeShifterConstants.EXTERNAL_ASSETS_FOLDER,
                    externalAssetOperation,
                    description,
                    progressBarStep,
                    ref progress
                );

                AssetSwitcher.RefreshAllAssets();
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"Could not {description.ToLower()}: {selected.Name}. Skins folder does not exist!",
                    "Fine, I'll take a look."
                );
            }

            EditorUtility.ClearProgressBar();
        }

        public static void CopyFromSkinsToUnity(DirectoryInfo directory)
        {
            string guid = directory.Name;

            if (!FileUtils.DoesFolderExistAndHaveFiles(directory.FullName))
            {
                FileUtils.SafeDelete(directory.FullName);
                return;
            }
            
            AssetSwitcher.DeleteAssetInternalCopy(guid);
            
            (string file, string meta) targetPath = GetTargetPath(guid);

            if (string.IsNullOrEmpty(targetPath.file))
            {
                ShapeShifterLogger.LogError($"Can't create asset Path for guid: {guid}");
                return;
            }

            string searchPattern = Path.GetFileName(targetPath.file) + "*";

            FileInfo[] files = directory.GetFiles(searchPattern);

            foreach (FileInfo fileInfo in files)
            {
                if (fileInfo.Extension == ".meta")
                {
                    string metaFile = targetPath.meta;

                    ShapeShifterLogger.Log($"Retrieving: {metaFile}");
                    FileUtils.SafeCopy(fileInfo.FullName, metaFile);
                }
                else
                {
                    ShapeShifterLogger.Log($"Retrieving: {targetPath.file}");
                    FileUtils.SafeCopy(fileInfo.FullName, targetPath.file);
                }
            }

            DirectoryInfo[] directories = directory.GetDirectories();

            if (directories.Length > 0)
            {
                FileUtils.SafeCopy(directories[0].FullName, targetPath.file);
            }
        }

        internal static void CopyFromUnityToSkins(DirectoryInfo skinDirectory)
        {
            if (!FileUtils.DoesFolderExistAndHaveFiles(skinDirectory.FullName) && skinDirectory.Exists)
            {
                FileUtils.SafeDelete(skinDirectory.FullName);
                return;
            }

            string guid = skinDirectory.Name;
            string origin = AssetDatabase.GUIDToAssetPath(guid);

            string originFullPath = PathUtils.GetFullPath(origin);

            if (string.IsNullOrEmpty(origin))
            {
                ShapeShifterLogger.LogError(
                    $"Getting an empty path for guid {guid}. Can't push changes to skin folder."
                );
                return;
            }

            string target = Path.Combine(skinDirectory.FullName, Path.GetFileName(origin));

            if (AssetDatabase.IsValidFolder(origin))
            {
                if (!Directory.Exists(originFullPath))
                {
                    return;
                }

                FileUtils.SafeCopy(origin, target);
            }
            else
            {
                if (!File.Exists(originFullPath))
                {
                    return;
                }

                FileUtils.TryCreateDirectory(skinDirectory.FullName, true);
                FileUtils.SafeCopy(origin, target);
                FileUtils.SafeCopy(origin + ".meta", target + ".meta");
            }

            string game = skinDirectory.Parent.Parent.Name;
            string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);

            ShapeShifter.DirtyAssets.Add(key);
        }

        private static void PerformOperationOnPath(string gameFolderPath,
            string assetFolder,
            Action<DirectoryInfo> operation,
            string description,
            float progressBarStep,
            ref float progress)
        {
            string assetFolderPath = Path.Combine(gameFolderPath, assetFolder);
            if (Directory.Exists(assetFolderPath))
            {
                DirectoryInfo internalFolder = new DirectoryInfo(assetFolderPath);

                DirectoryInfo[] infos = internalFolder.GetDirectories();
                for (int index = 0; index < infos.Length; index++)
                {
                    DirectoryInfo directory = infos[index];

                    operation(directory);

                    progress += progressBarStep;
                    EditorUtility.DisplayProgressBar("Shape Shifter", $"{description}...", progress);
                }
            }
        }

        private static (string file, string meta) GetTargetPath(string guid)
        {
            string assetPathFromGitIgnore = PathUtils.GetFullPath(GitIgnore.GetIgnoredPathByGuid(guid));
            assetPathFromGitIgnore = PathUtils.NormalizePath(assetPathFromGitIgnore);

            return (assetPathFromGitIgnore, assetPathFromGitIgnore + ".meta");
        }
    }
}