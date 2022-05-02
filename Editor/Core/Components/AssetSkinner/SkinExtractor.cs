using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter.Skinner
{
    internal static class SkinExtractor
    {
        public static void ExtractAsSkin(string assetPathToExtract, string targetFolder)
        {
            if (AssetSkinner.IsSkinned(assetPathToExtract))
            {
                ShapeShifterLogger.LogWarning($"{assetPathToExtract} is already a skinned asset");
                return;
            }

            if (!AssetSkinner.TryGetParentSkinnedFolder(assetPathToExtract, out string parentFolder))
            {
                ShapeShifterLogger.LogWarning($"{assetPathToExtract} is not inside a skinned folder. Therefore it cannot be extracted");
                return;
            }

            if (string.IsNullOrEmpty(targetFolder))
            {
                return;
            }
            
            string parentGuid = AssetDatabase.AssetPathToGUID(parentFolder);
            string parentFolderName = Path.GetFileName(parentFolder);

            string guid = AssetDatabase.AssetPathToGUID(assetPathToExtract);

            string source = assetPathToExtract;
            string assetName = Path.GetFileName(assetPathToExtract);
            string destination = Path.Combine(
                PathUtils.GetPathRelativeToAssetsFolder(targetFolder),
                assetName
            );

            FileUtil.MoveFileOrDirectory(source, destination);
            FileUtil.MoveFileOrDirectory(source + ".meta", destination + ".meta");
            AssetDatabase.Refresh();

            AssetSkinner.SkinAsset(destination);

            List<string> gameNames = ShapeShifterConfiguration.Instance.GameNames;

            foreach (string gameName in gameNames)
            {
                GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(gameName);
                
                AssetSkin extractedAssetSkin = gameSkin.GetAssetSkin(guid);

                AssetSkin folderAssetSkin = gameSkin.GetAssetSkin(parentGuid);

                if (folderAssetSkin == null)
                {
                    return;
                }
                
                string originalSkinPath = Path.Combine(
                    folderAssetSkin.FolderPath,
                    PathUtils.GetRelativeTo(assetPathToExtract, parentFolderName)
                );

                AssetSkinner.RemoveSkinFromGame(destination, gameName);

                if (PathUtils.FileOrDirectoryExists(originalSkinPath))
                {
                    string newSkinPath = Path.Combine(extractedAssetSkin.FolderPath, assetName);
                    string skinFolder = Path.GetDirectoryName(newSkinPath);
                    FileUtils.TryCreateDirectory(skinFolder);
                    FileUtil.MoveFileOrDirectory(originalSkinPath, newSkinPath);
                    FileUtil.MoveFileOrDirectory(originalSkinPath + ".meta", newSkinPath + ".meta");
                }
            }
        }
    }
}