using System.IO;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Saver
{
    public class InternalAssetsPostProcessor : AssetPostprocessor
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

            if (AssetSkinner.IsSkinned(modifiedAssetPath, ShapeShifter.ActiveGame))
            {
                setDirty = true;
                finalModifiedPath = modifiedAssetPath;
            }
            else
            {
                if (AssetSkinner.TryGetParentSkinnedFolder(modifiedAssetPath, out string skinnedFolderPath))
                {
                    setDirty = true;
                    finalModifiedPath = skinnedFolderPath;
                    modificationType = ModificationType.Modified;
                }
            }

            if (setDirty)
            {
                ModifiedAssetInfo modifiedAssetInfo = new ModifiedAssetInfo(
                    finalModifiedPath,
                    modificationType,
                    SkinType.Internal
                );

                UnsavedAssetsManager.Add(modifiedAssetInfo);
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

            if (AssetSkinner.TryGetParentSkinnedFolder(
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

                if (assetSkin == null)
                {
                    continue;
                }

                assetSkin.Rename(assetPath);
                assetSkin.Stage();
            }

            if (AssetDatabase.IsValidFolder(assetPath))
            {
                assetPath += Path.DirectorySeparatorChar;
            }

            GitIgnore.Add(guid, assetPath);
        }
    }
}