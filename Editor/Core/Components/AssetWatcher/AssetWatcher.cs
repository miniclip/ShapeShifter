using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Watcher
{
    public class AssetWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (ShapeShifterConfiguration.Instance == null)
                return;

            //changed
            foreach (string importedAsset in importedAssets)
            {
                OnModifiedAsset(importedAsset);
            }

            //renamed
            for (int index = 0; index < movedAssets.Length; index++)
            {
                string newName = movedAssets[index];
                string oldName = movedFromAssetPaths[index];
                OnAssetRenamed(newName, oldName);
            }

            foreach (string deletedAsset in deletedAssets)
            {
                OnModifiedAsset(deletedAsset);
            }
        }

#region Internal
        private static void OnModifiedAsset(string modifiedAssetPath)
        {
            if (AssetSkinner.IsSkinned(modifiedAssetPath))
            {
                ShapeShifterConfiguration.Instance.HasUnsavedChanges = true;
            }
            else
            {
                if (TryGetParentSkinnedFolder(modifiedAssetPath, out string skinnedFolderPath))
                {
                    ShapeShifterConfiguration.Instance.HasUnsavedChanges = true;
                }
            }
        }

        private static void OnAssetRenamed(string newName, string oldName)
        {
            bool isSkinned = AssetSkinner.IsSkinned(newName);

            if (!isSkinned)
                return;

            RenameAssetSkins(newName);

            ShapeShifterConfiguration.Instance.HasUnsavedChanges = true;

            GitUtils.Stage(newName + ".meta");
            GitUtils.Stage(oldName + ".meta");
        }

        private static void RenameAssetSkins(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            for (int index = 0; index < ShapeShifterConfiguration.Instance.GameNames.Count; index++)
            {
                string gameName = ShapeShifterConfiguration.Instance.GameNames[index];
                GameSkin gameSkin = new GameSkin(gameName);

                var assetSkin = gameSkin.GetAssetSkin(guid);

                assetSkin.Rename(assetPath);
                assetSkin.Stage();
            }

            GitUtils.ReplaceIgnoreEntry(guid, PathUtils.GetPathRelativeToRepositoryFolder(assetPath));
        }

        private static bool TryGetParentSkinnedFolder(string assetPath, out string skinnedParentFolderPath)
        {
            string[] parentFolders = assetPath.Split('/');

            for (int index = parentFolders.Length - 1; index >= 0; index--)
            {
                string parentFolder = string.Join("/", parentFolders, 0, index);

                if (AssetSkinner.IsSkinned(parentFolder))
                {
                    skinnedParentFolderPath = parentFolder;
                    return true;
                }
            }

            skinnedParentFolderPath = null;
            return false;
        }
#endregion

#region External
        internal static void StartWatchingFolder(string pathToWatch) =>
            FileSystemWatcherManager.AddPathToWatchlist(pathToWatch, OnFileChanged);

        internal static void StopWatchingFolder(string pathToUnwatch) =>
            FileSystemWatcherManager.RemovePathFromWatchlist(pathToUnwatch);

        internal static void ClearAllWatchedPaths() => FileSystemWatcherManager.RemoveAllPathsFromWatchlist();

        private static void OnFileChanged(object sender, FileSystemEventArgs args)
        {
            DirectoryInfo assetDirectory = new DirectoryInfo(Path.GetDirectoryName(args.FullPath));
            string game = assetDirectory.Parent.Parent.Name;
            string guid = assetDirectory.Name;
            string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);

            SharedInfo.DirtyAssets.Add(key);
        }
#endregion
    }
}