using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Switcher
{
    public static class AssetSwitcher
    {
        internal static void RestoreMissingAssets()
        {
            List<string> missingAssets = new List<string>();
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (ShapeShifter.ActiveGameSkin.HasInternalSkins())
            {
                List<AssetSkin> assetSkins = ShapeShifter.ActiveGameSkin.GetAssetSkins();

                foreach (AssetSkin assetSkin in assetSkins)
                {
                    if (!assetSkin.IsValid())
                    {
                        //Delete asset skin folder?
                        // assetSkin.Delete();
                    }

                    string guid = assetSkin.Guid;

                    string assetDatabasePath = PathUtils.GetFullPath(AssetDatabase.GUIDToAssetPath(guid));
                    string assetGitIgnorePath = PathUtils.GetFullPath(GitIgnore.GetIgnoredPathByGuid(guid));

                    if (!string.Equals(assetDatabasePath, assetGitIgnorePath))
                    {
                        missingAssets.Add(guid);
                        continue;
                    }

                    if (!PathUtils.FileOrDirectoryExists(assetDatabasePath))
                    {
                        missingAssets.Add(guid);
                    }
                }

                if (missingAssets.Count == 0)
                {
                    ShapeShifterLogger.Log("Nothing to sync from skins folder.");
                }
                else
                {
                    PerformCopiesWithTracking(
                        ShapeShifter.ActiveGameSkin,
                        "Add missing skins",
                        CopyIfMissingInternal,
                        CopyFromSkinnedExternalToOrigin
                    );
                    stopwatch.Stop();
                    ShapeShifterLogger.Log(
                        missingAssets.Count > 0
                            ? $"Synced {missingAssets.Count} assets in {stopwatch.Elapsed.TotalSeconds} seconds"
                            : "Nothing to retrieve."
                    );
                }

                stopwatch.Stop();
            }
        }

        private static void CopyFromOriginToSkinnedExternal(DirectoryInfo directory)
        {
            string relativePath = ExternalAssetSkinner.GenerateRelativePathFromKey(directory.Name);
            string origin = Path.Combine(Application.dataPath, relativePath);
            string target = Path.Combine(directory.FullName, Path.GetFileName(origin));
            IOUtils.CopyFile(origin, target);
        }

        private static void CopyFromSkinnedExternalToOrigin(DirectoryInfo directory)
        {
            string relativePath = ExternalAssetSkinner.GenerateRelativePathFromKey(directory.Name);
            string target = Path.Combine(Application.dataPath, relativePath);
            string searchPattern = Path.GetFileName(target);
            FileInfo origin = directory.GetFiles(searchPattern)[0];
            origin.CopyTo(target, true);
        }

        private static void CopyFromSkinsToUnity(DirectoryInfo directory)
        {
            string guid = directory.Name;

            // Ensure it has the same name, so we don't end up copying .DS_Store
            string target = AssetDatabase.GUIDToAssetPath(guid);
            string searchPattern = Path.GetFileName(target) + "*";

            FileInfo[] files = directory.GetFiles(searchPattern);

            if (files.Length > 0)
            {
                foreach (FileInfo fileInfo in files)
                {
                    if (fileInfo.Extension == ".meta")
                    {
                        fileInfo.CopyTo(target + ".meta", true);
                    }
                    else
                    {
                        fileInfo.CopyTo(target, true);
                    }
                }
            }

            DirectoryInfo[] directories = directory.GetDirectories();

            if (directories.Length > 0)
            {
                target = Path.Combine(
                    Application.dataPath.Replace("/Assets", string.Empty),
                    target
                );

                IOUtils.CopyFolder(directories[0], new DirectoryInfo(target));
            }
        }

        private static void CopyFromUnityToSkins(DirectoryInfo skinDirectory)
        {
            if (IOUtils.IsFolderEmpty(skinDirectory))
            {
                //There shouldn't be an empty skin folder, most likely it was removed outside of ShapeShifter. E.g. discarding changes in a git client.
                skinDirectory.Delete();
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

                DirectoryInfo originInfo = new DirectoryInfo(origin);
                DirectoryInfo targetInfo = new DirectoryInfo(target);
                IOUtils.CopyFolder(originInfo, targetInfo);
            }
            else
            {
                if (!File.Exists(originFullPath))
                {
                    return;
                }

                IOUtils.TryCreateDirectory(skinDirectory.FullName, true);
                IOUtils.CopyFile(origin, target);
                IOUtils.CopyFile(origin + ".meta", target + ".meta");
            }

            string game = skinDirectory.Parent.Parent.Name;
            string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);

            ShapeShifter.DirtyAssets.Add(key);
        }

        internal static void OverwriteSelectedSkin(GameSkin selected, bool forceOverwrite = false)
        {
            ShapeShifterUtils.SavePendingChanges();

            string name = selected.Name;

            if (ShapeShifter.ActiveGameSkin != selected)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"This will overwrite the {name} skins with the current assets. ");

                stringBuilder.Append($"The last asset switch was to {ShapeShifter.ActiveGameName}");

                stringBuilder.Append(" Are you sure?");

                if (!forceOverwrite
                    && !EditorUtility.DisplayDialog(
                        "Shape Shifter",
                        stringBuilder.ToString(),
                        "Yeah, I'm sure, go ahead.",
                        "Wait, what? No, stop!"
                    ))
                {
                    return;
                }
            }

            PerformCopiesWithTracking(
                selected,
                "Overwrite selected skin",
                CopyFromUnityToSkins,
                CopyFromOriginToSkinnedExternal
            );

            ShapeShifterConfiguration.Instance.SetDirty(false);
        }

        private static void PerformCopiesWithTracking(GameSkin selected,
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

                RefreshAllAssets();
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

                foreach (DirectoryInfo directory in internalFolder.GetDirectories())
                {
                    operation(directory);

                    progress += progressBarStep;
                    EditorUtility.DisplayProgressBar("Shape Shifter", $"{description}...", progress);
                }
            }
        }

        [MenuItem("Window/Shape Shifter/Refresh All Assets", false, 72)]
        private static void RefreshAllAssets()
        {
#if UNITY_2020
            throw new NotImplementedException("// TODO: Replace this in Unity 2020 with PackageManager.Client.Resolve");
#else
            if (HasAnyPackageRelatedSkin())
            {
                ForceUnityToLoseAndRegainFocus();
            }
#endif

            AssetDatabase.Refresh();
        }

        private static bool HasAnyPackageRelatedSkin()
        {
            bool isManifestSkinned = ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.Any(
                externalAssetPath => externalAssetPath.Contains("manifest.json")
            );

            return isManifestSkinned;
        }

        private static void ForceUnityToLoseAndRegainFocus()
        {
            // Force Unity to lose and regain focus, so it resolves any new changes on the packages
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments =
                        "-e 'tell application \"Finder\" to activate' -e 'delay 0.5' -e 'tell application \"Unity\" to activate'",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
        }

        internal static void SwitchToGame(GameSkin gameToSwitchTo, bool forceSwitch = false)
        {
            if (ShapeShifterConfiguration.Instance.IsDirty && !forceSwitch)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Shape Shifter",
                    "There are unsaved changes in your skinned assets. You should make sure to save them into your Active Game folder",
                    $"Save changes to {ShapeShifter.ActiveGameName} and switch to {gameToSwitchTo.Name}.",
                    "Cancel Switch",
                    $"Discard changes and switch to {gameToSwitchTo.Name}"
                );

                switch (choice)
                {
                    case 0:
                        OverwriteSelectedSkin(ShapeShifter.ActiveGameSkin);
                        break;

                    case 1:
                        return;

                    case 2:
                    default:
                        break;
                }
            }

            PerformCopiesWithTracking(
                gameToSwitchTo,
                "Switch to game",
                CopyFromSkinsToUnity,
                CopyFromSkinnedExternalToOrigin
            );
            ShapeShifter.ActiveGame = gameToSwitchTo.Name;
            ShapeShifterConfiguration.Instance.SetDirty(false);

            GameSkin gameSkin = ShapeShifter.ActiveGameSkin;

            foreach (AssetSkin assetSkin in gameSkin.GetAssetSkins())
            {
                string guid = assetSkin.Guid;

                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);
            }
        }

        private static void CopyIfMissingInternal(DirectoryInfo directory)
        {
            string guid = directory.Name;

            string assetDatabasePath = PathUtils.GetFullPath(AssetDatabase.GUIDToAssetPath(guid));
            string assetGitIgnorePath = PathUtils.GetFullPath(GitIgnore.GetIgnoredPathByGuid(guid));

            //prioritize path from gitignore as is the only one version controlled
            string targetPath = assetGitIgnorePath;

            if (string.IsNullOrEmpty(targetPath))
            {
                ShapeShifterLogger.LogError($"Can't find Asset Path for guid: {guid}");
                return;
            }

            string assetFolder = Path.GetDirectoryName(targetPath);

            IOUtils.TryCreateDirectory(assetFolder);

            if (!string.Equals(assetDatabasePath, assetGitIgnorePath))
            {
                if (PathUtils.FileOrDirectoryExists(assetDatabasePath))
                {
                    //delete any file on AssetDatabasePath as is probably outdated and should not be there
                    FileUtil.DeleteFileOrDirectory(assetDatabasePath);
                    FileUtil.DeleteFileOrDirectory(assetDatabasePath + ".meta");
                }
            }

            string searchPattern = Path.GetFileName(targetPath) + "*";

            FileInfo[] files = directory.GetFiles(searchPattern);

            foreach (FileInfo fileInfo in files)
            {
                if (fileInfo.Extension == ".meta")
                {
                    string metaFile = targetPath + ".meta";
                    if (File.Exists(PathUtils.GetFullPath(metaFile)))
                    {
                        continue;
                    }

                    ShapeShifterLogger.Log($"Retrieving: {metaFile}");
                    FileUtil.CopyFileOrDirectory(fileInfo.FullName, metaFile);
                }
                else
                {
                    if (File.Exists(PathUtils.GetFullPath(targetPath)))
                    {
                        continue;
                    }

                    ShapeShifterLogger.Log($"Retrieving: {targetPath}");
                    FileUtil.CopyFileOrDirectory(fileInfo.FullName, targetPath);
                }
            }

            DirectoryInfo[] directories = directory.GetDirectories();

            if (directories.Length > 0)
            {
                targetPath = Path.Combine(
                    Application.dataPath.Replace("/Assets", string.Empty),
                    targetPath
                );

                IOUtils.CopyFolder(directories[0], new DirectoryInfo(targetPath));
            }
        }
    }
}