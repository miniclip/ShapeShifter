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
        
        public enum ModificationType
        {
            IMPORT = 0,
            RENAME = 1,
            DELETE = 2
        }
        
        public static void OnImportedAsset(string modifiedAssetPath)
        {
            if (configuration == null)
                return;

            bool isSkinned = IsSkinned(modifiedAssetPath);

            List<string> configurationModifiedAssetPaths = Configuration.ModifiedAssetPaths;

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

        public static void OnMovedAsset(string newName, string oldName)
        {
            if (configuration == null)
                return;

            bool isSkinned = IsSkinned(newName);

            if (!isSkinned)
                return;
            
            RenameAssetSkins(newName);
            
            if (Configuration.ModifiedAssetPaths.Contains(newName))
            {
                Configuration.ModifiedAssetPaths.Remove(newName);
            } 
            
            GitUtils.Stage(newName+".meta");
            GitUtils.Stage(oldName+".meta");
        }

        private static void RenameAssetSkins(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            foreach (string gameName in Configuration.GameNames)
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

                if (IsSkinned(parentFolder))
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
        private void StartWatchingFolder(string pathToWatch) =>
            FileSystemWatcherManager.AddPathToWatchlist(pathToWatch, OnFileChanged);

        private static void StopWatchingFolder(string pathToUnwatch) =>
            FileSystemWatcherManager.RemovePathFromWatchlist(pathToUnwatch);

        private void ClearAllWatchedPaths() => FileSystemWatcherManager.RemoveAllPathsFromWatchlist();

        private void OnFileChanged(object sender, FileSystemEventArgs args)
        {
            DirectoryInfo assetDirectory = new DirectoryInfo(Path.GetDirectoryName(args.FullPath));
            string game = assetDirectory.Parent.Parent.Name;
            string key = GenerateAssetKey(game, assetDirectory.Name);
            dirtyAssets.Add(key);
        }
#endregion
        
    }
}