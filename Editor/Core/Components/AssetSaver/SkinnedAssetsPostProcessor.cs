using System.IO;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using UnityEditor;

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

        private static void OnModifiedAsset(string modifiedAssetPath)
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
                if (TryGetParentSkinnedFolder(modifiedAssetPath, out string skinnedFolderPath))
                {
                    setDirty = true;
                    finalModifiedPath = skinnedFolderPath;
                }
            }

            if (setDirty)
            {
                UnsavedAssetsManager.AddModifiedPath(finalModifiedPath);
                ShapeShifterConfiguration.Instance.SetDirty();
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
                OnModifiedAsset(skinnedParentFolderPath);
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