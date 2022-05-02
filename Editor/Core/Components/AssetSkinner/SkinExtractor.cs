using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter.Skinner
{
    internal static class SkinExtractor
    {
        public static bool ExtractAsSkin(string assetPathToExtract, string targetFolder)
        {
            if (string.IsNullOrEmpty(targetFolder))
            {
                return false;
            }
            
            if (AssetSkinner.IsSkinned(assetPathToExtract))
            {
                ShapeShifterLogger.LogWarning($"{assetPathToExtract} is already a skinned asset");
                return false;
            }

            if (!AssetSkinner.TryGetParentSkinnedFolder(assetPathToExtract, out string skinnedParentFolder))
            {
                ShapeShifterLogger.LogWarning($"{assetPathToExtract} is not inside a skinned folder. Therefore it cannot be extracted");
                return false;
            }

            if (targetFolder.Contains(PathUtils.GetFullPath(skinnedParentFolder)))
            {
                ShapeShifterLogger.LogWarning($"Target destination is inside a skinned folder. Select an unskinned destination folder.");
                return false;
            }
            
            string parentGuid = AssetDatabase.AssetPathToGUID(skinnedParentFolder);
            string parentFolderName = Path.GetFileName(skinnedParentFolder);

            string guid = AssetDatabase.AssetPathToGUID(assetPathToExtract);

            string assetName = Path.GetFileName(assetPathToExtract);
            string destination = Path.Combine(
                PathUtils.GetPathRelativeToAssetsFolder(targetFolder),
                assetName
            );

            FileUtil.MoveFileOrDirectory(assetPathToExtract, destination);
            FileUtil.MoveFileOrDirectory(assetPathToExtract + ".meta", destination + ".meta");
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
                    continue;
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

            return true;
        }
    }
}