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

        private static void OnModifiedAsset(string modifiedAssetPath)
        {
            if (AssetSkinner.IsSkinned(modifiedAssetPath))
            {
                ShapeShifterConfiguration.Instance.SetDirty();
            }
            else
            {
                if (TryGetParentSkinnedFolder(modifiedAssetPath, out string skinnedFolderPath))
                {
                    ShapeShifterConfiguration.Instance.SetDirty();
                }
            }
        }

        private static void OnAssetRenamed(string newName, string oldName)
        {
            bool isSkinned = AssetSkinner.IsSkinned(newName);

            if (!isSkinned)
                return;

            RenameAssetSkins(newName);

            ShapeShifterConfiguration.Instance.SetDirty();

            GitUtils.Stage(newName + ".meta");
            GitUtils.Stage(oldName + ".meta");
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
    }
}