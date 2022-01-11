using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public partial class ShapeShifter
    {
#region Internal
        
        public static void OnImportedAsset(string modifiedAssetPath)
        {

            bool isSkinned = AssetSkinner.IsSkinned(modifiedAssetPath);

            List<string> configurationModifiedAssetPaths = ShapeShifterConfiguration.Instance.ModifiedAssetPaths;

            if (isSkinned && !configurationModifiedAssetPaths.Contains(modifiedAssetPath))
            {
                configurationModifiedAssetPaths.Add(modifiedAssetPath);
                return;
            }

            if (configurationModifiedAssetPaths.Contains(modifiedAssetPath))
            {
                configurationModifiedAssetPaths.Remove(modifiedAssetPath);
                return;
            }

            if (TryGetParentSkinnedFolder(modifiedAssetPath, out string skinnedFolderPath))
            {
                OnImportedAsset(skinnedFolderPath);
            }
        }

        public static void OnAssetRenamed(string newName, string oldName)
        {

            bool isSkinned = AssetSkinner.IsSkinned(newName);

            if (!isSkinned)
                return;
            
            RenameAssetSkins(newName);
            
            if (ShapeShifterConfiguration.Instance.ModifiedAssetPaths.Contains(newName))
            {
                ShapeShifterConfiguration.Instance.ModifiedAssetPaths.Remove(newName);
            } 
            
            GitUtils.Stage(newName+".meta");
            GitUtils.Stage(oldName+".meta");
        }

        private static void RenameAssetSkins(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            foreach (string gameName in ShapeShifterConfiguration.Instance.GameNames)
            {
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