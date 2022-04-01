using System.IO;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Saver
{
    public class SkinnedAssetsPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            for (int index = 0; index < importedAssets.Length; index++)
            {
                string importedAsset = importedAssets[index];
                OnModifiedAsset(importedAsset, ModificationType.Modified);
            }

            for (int index = 0; index < movedAssets.Length; index++)
            {
                string newName = movedAssets[index];
                string oldName = movedFromAssetPaths[index];
                OnAssetRenamed(newName, oldName);
            }

            for (int index = 0; index < deletedAssets.Length; index++)
            {
                string deletedAsset = deletedAssets[index];
                OnModifiedAsset(deletedAsset, ModificationType.Deleted);
            }
        }

        private static void OnModifiedAsset(string modifiedAssetPath, ModificationType modificationType)
        {
            string finalModifiedPath = string.Empty;
            bool setDirty = false;
            Debug.Log($"##! Modified {modifiedAssetPath}");

            if (AssetSkinner.IsSkinned(modifiedAssetPath, ShapeShifter.ActiveGame))
            {
                setDirty = true;
                finalModifiedPath = modifiedAssetPath;
            }
            else
            {
                if (TryGetParentSkinnedFolder(modifiedAssetPath, out string skinnedFolderPath))
                {
                    setDirty = true;
                    finalModifiedPath = skinnedFolderPath;
                    modificationType = ModificationType.Modified;
                }
            }

            if (setDirty)
            {
                UnsavedAssetsManager.AddModifiedPath(finalModifiedPath, modificationType);
            }
        }

        private static void OnAssetRenamed(string newName, string oldName)
        {
            bool isSkinned = AssetSkinner.IsSkinned(newName, ShapeShifter.ActiveGame);

            if (isSkinned)
            {
                RenameAssetSkins(newName);

                ShapeShifterConfiguration.Instance.SetDirty();

                GitUtils.Stage(newName + ".meta");
                GitUtils.Stage(oldName + ".meta");
            }

            if (TryGetParentSkinnedFolder(
                    newName,
                    out string skinnedParentFolderPath,
                    ShapeShifter.ActiveGame
                ))
            {
                OnModifiedAsset(skinnedParentFolderPath, ModificationType.Modified);
            }
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

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                assetPath += Path.DirectorySeparatorChar;
            }

            GitIgnore.Add(guid, assetPath);
        }

        private static bool TryGetParentSkinnedFolder(string assetPath, out string skinnedParentFolderPath,
            string gameName = null)
        {
            string[] parentFolders = assetPath.Split('/');

            for (int index = parentFolders.Length - 1; index >= 0; index--)
            {
                string parentFolder = string.Join("/", parentFolders, 0, index);

                if (AssetSkinner.IsSkinned(parentFolder, gameName))
                {
                    skinnedParentFolderPath = parentFolder;
                    return true;
                }
            }

            skinnedParentFolderPath = null;
            return false;
        }
    }
}