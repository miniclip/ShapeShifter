using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public static class AssetSwitcher
    {
        private static Dictionary<string, string> missingGuidsToPathDictionary = new Dictionary<string, string>();

        private static string ExtractGuidFromMetaFile(string path)
        {
            if (Path.GetExtension(path) != ".meta")
            {
                ShapeShifterLogger.LogError($"Trying to extract guid from non meta file : {path}");
                return string.Empty;
            }

            using (StreamReader reader = new StreamReader(path))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();

                    if (string.IsNullOrEmpty(line))
                        continue;
                    
                    if (!line.StartsWith("guid"))
                        continue;

                    return line.Split(' ')[1];
                }
            }

            return string.Empty;
        }

        internal static void RestoreMissingAssets()
        {
            missingGuidsToPathDictionary.Clear();
            List<string> missingAssets = new List<string>();
            Stopwatch stopwatch = Stopwatch.StartNew();
            if (SharedInfo.ActiveGameSkin.HasInternalSkins())
            {
                EditorUtility.DisplayProgressBar("ShapeShifter", "Checking for missing assets", 0.5f);

                List<AssetSkin> assetSkins = SharedInfo.ActiveGameSkin.GetAssetSkins();

                foreach (AssetSkin assetSkin in assetSkins)
                {
                    if (!assetSkin.IsValid())
                    {
                        //Delete asset skin folder?
                        // assetSkin.Delete();
                    }

                    string guid = assetSkin.Guid;
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                    if (string.IsNullOrEmpty(assetPath))
                    {
                        missingAssets.Add(guid);
                        continue;
                    }

                    if (!File.Exists(PathUtils.GetFullPath(assetPath)))
                    {
                        missingAssets.Add(guid);
                    }
                }

                // get all deleted meta files
                IEnumerable<GitUtils.ChangedFileGitInfo> deletedMetasInGit = GitUtils.GetDeletedFiles()
                    .Where(
                        deletedMeta => deletedMeta.path.Contains(".meta") && PathUtils.IsInternalPath(deletedMeta.path)
                    );

                //Restore meta files and do not call AssetDatabase refresh to prevent from being deleted again
                //Store in dictionary mapping guid to path, since AssetDatabase.GUIDToAssetPath will not work
                foreach (var deletedMeta in deletedMetasInGit)
                {
                    string metaFullPath = PathUtils.GetFullPath(deletedMeta.path);

                    //need to recover deleted meta to check its contents
                    GitUtils.DiscardChanges(metaFullPath);

                    string metaGuid = ExtractGuidFromMetaFile(metaFullPath);

                    string fullPath = metaFullPath.Replace(".meta", "");

                    if (SharedInfo.ActiveGameSkin.HasGuid(metaGuid))
                    {
                        missingGuidsToPathDictionary.Add(metaGuid, PathUtils.GetPathRelativeToAssetsFolder(fullPath));
                    }
                    else
                    {
                        //if there are no skins with this guid, delete meta again
                        FileUtil.DeleteFileOrDirectory(metaFullPath);
                    }
                }

                PerformCopiesWithTracking(
                    SharedInfo.ActiveGame,
                    "Add missing skins",
                    CopyIfMissingInternal,
                    CopyFromSkinnedExternalToOrigin
                );
            }

            EditorUtility.ClearProgressBar();

            stopwatch.Stop();
            int missingCount = missingGuidsToPathDictionary.Count;
            ShapeShifterLogger.Log(
                missingCount > 0
                    ? $"Finished retrieving {missingCount} assets in {stopwatch.Elapsed.TotalSeconds}"
                    : $"Nothing to retrieve."
            );
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
            string target = Path.Combine(skinDirectory.FullName, Path.GetFileName(origin));

            if (AssetDatabase.IsValidFolder(origin))
            {
                DirectoryInfo originInfo = new DirectoryInfo(origin);
                DirectoryInfo targetInfo = new DirectoryInfo(target);
                IOUtils.CopyFolder(originInfo, targetInfo);
            }
            else
            {
                IOUtils.TryCreateDirectory(skinDirectory.FullName, true);
                IOUtils.CopyFile(origin, target);
                IOUtils.CopyFile(origin + ".meta", target + ".meta");
            }
        }

        internal static void OverwriteSelectedSkin(int selected, bool forceOverwrite = false)
        {
            ShapeShifterConfiguration.Instance.ModifiedAssetPaths.Clear();

            ShapeShifterUtils.SavePendingChanges();

            string game = ShapeShifterUtils.GetGameName(selected);

            if (SharedInfo.ActiveGameName != game)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"This will overwrite the {game} skins with the current assets. ");

                stringBuilder.Append($"The last asset switch was to {SharedInfo.ActiveGameName}");

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
                selected: selected,
                description: "Overwrite selected skin",
                CopyFromUnityToSkins,
                CopyFromOriginToSkinnedExternal
            );
        }

        private static void PerformCopiesWithTracking(int selected,
            string description,
            Action<DirectoryInfo> internalAssetOperation,
            Action<DirectoryInfo> externalAssetOperation)
        {
            ShapeShifterLogger.Log($"{description}: {ShapeShifterUtils.GetGameName(selected)}");

            string gameFolderPath = GetGameFolderPath(selected);

            if (Directory.Exists(gameFolderPath))
            {
                // this will fail the total by 0-3, as it counts the game, the internal and the external directories
                // but that doesn't make enough of a difference to justify making this a more complex calculation
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
                    SharedInfo.InternalAssetsFolder,
                    internalAssetOperation,
                    description,
                    progressBarStep,
                    ref progress
                );

                PerformOperationOnPath(
                    gameFolderPath,
                    SharedInfo.ExternalAssetsFolder,
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
                    $"Could not {description.ToLower()}: {ShapeShifterUtils.GetGameName(selected)}. Skins folder does not exist!",
                    "Fine, I'll take a look."
                );
            }

            EditorUtility.ClearProgressBar();
        }

        private static string GetGameFolderPath(int selected)
        {
            return Path.Combine(SharedInfo.SkinsFolder.FullName, ShapeShifterUtils.GetGameName(selected));
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

        private static void RefreshAllAssets()
        {
            // Force Unity to lose and regain focus, so it resolves any new changes on the packages
            // TODO: Replace this in Unity 2020 with PackageManager.Client.Resolve
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments =
                        "-e 'tell application \"Finder\" to activate' -e 'delay 0.5' -e 'tell application \"Unity\" to activate'",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            AssetDatabase.Refresh();
        }

        internal static void SwitchToGame(int gameToSwitchTo, bool forceSwitch = false)
        {
            if (ShapeShifterConfiguration.Instance.ModifiedAssetPaths.Count > 0 && !forceSwitch)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Shape Shifter",
                    $"There are unsaved changes in your skinned assets. You should make sure to save them into your Active Game folder",
                    $"Save changes to {SharedInfo.ActiveGameName} and switch to {ShapeShifterUtils.GetGameName(gameToSwitchTo)}.",
                    "Cancel Switch",
                    $"Discard changes and switch to {ShapeShifterUtils.GetGameName(gameToSwitchTo)}"
                );

                switch (choice)
                {
                    case 0:
                        OverwriteSelectedSkin(SharedInfo.ActiveGame);
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
            SharedInfo.ActiveGame = gameToSwitchTo;
            ShapeShifterConfiguration.Instance.ModifiedAssetPaths.Clear();
        }

        private static void CopyIfMissingInternal(DirectoryInfo directory)
        {
            string guid = directory.Name;

            // Ensure it has the same name, so we don't end up copying .DS_Store
            string target = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(target) && !missingGuidsToPathDictionary.TryGetValue(guid, out target))
            {
                ShapeShifterLogger.LogError($"Can't find Asset Path for guid: {guid}");
                return;
            }

            string searchPattern = Path.GetFileName(target) + "*";

            FileInfo[] files = directory.GetFiles(searchPattern);

            if (files.Length > 0)
            {
                foreach (FileInfo fileInfo in files)
                {
                    //ShapeShifterLogger.Log($"[Shape Shifter] Copying from: {origin.FullName} to {target}");
                    if (fileInfo.Extension == ".meta")
                    {
                        string metaFile = target + ".meta";
                        if (File.Exists(PathUtils.GetFullPath(metaFile)))
                        {
                            continue;
                        }

                        ShapeShifterLogger.Log($"Retrieving: {metaFile}");
                        fileInfo.CopyTo(metaFile, true);
                    }
                    else
                    {
                        if (File.Exists(PathUtils.GetFullPath(target)))
                        {
                            continue;
                        }

                        ShapeShifterLogger.Log($"Retrieving: {target}");
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
    }
}