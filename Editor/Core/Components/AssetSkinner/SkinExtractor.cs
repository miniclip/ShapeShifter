using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter.Skinner
{
    static class SkinExtractor
    {
        public static bool ExtractAsSkin(string originalAssetPath, string targetFolder)
        {
            if (!IsExtractionValid(originalAssetPath, targetFolder))
            {
                return false;
            }

            string newAssetPath = GetDestinationPath(originalAssetPath, targetFolder);

            MoveAsset(originalAssetPath, newAssetPath);
            AssetDatabase.Refresh();

            AssetSkinner.SkinAsset(newAssetPath);

            MoveSkinVersionsToNewLocation(
                originalAssetPath,
                newAssetPath
            );

            return true;
        }

        private static bool IsExtractionValid(string assetPathToExtract, string targetFolder)
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
                ShapeShifterLogger.LogWarning(
                    $"{assetPathToExtract} is not inside a skinned folder. Therefore it cannot be extracted"
                );
                return false;
            }

            if (targetFolder.Contains(PathUtils.GetFullPath(skinnedParentFolder)))
            {
                ShapeShifterLogger.LogWarning(
                    "Target destination is inside a skinned folder. Select an unskinned destination folder."
                );
                return false;
            }

            return true;
        }

        private static string GetDestinationPath(string assetPathToExtract, string targetFolder)
        {
            return Path.Combine(
                PathUtils.GetPathRelativeToAssetsFolder(targetFolder),
                Path.GetFileName(assetPathToExtract)
            );
        }

        private static void MoveAsset(string source, string destination)
        {
            FileUtils.SafeMove(source, destination);
            FileUtils.SafeMove(source + ".meta", destination + ".meta");
        }

        private static void MoveSkinVersionsToNewLocation(string originalAssetPath,
            string newAssetPath)
        {
            List<string> gameNames = ShapeShifterConfiguration.Instance.GameNames;

            AssetSkinner.TryGetParentSkinnedFolder(originalAssetPath, out string originalParentFolder);

            string parentGuid = AssetDatabase.AssetPathToGUID(originalParentFolder);
            string parentFolderName = Path.GetFileName(originalParentFolder);

            foreach (string gameName in gameNames)
            {
                GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(gameName);

                AssetSkin folderAssetSkin = gameSkin.GetAssetSkin(parentGuid);

                if (folderAssetSkin == null)
                {
                    continue;
                }

                string originalSkinPath = Path.Combine(
                    folderAssetSkin.FolderPath,
                    PathUtils.GetRelativeTo(originalAssetPath, parentFolderName)
                );

                AssetSkinner.RemoveSkinFromGame(newAssetPath, gameName);

                if (PathUtils.FileOrDirectoryExists(originalSkinPath))
                {
                    string newSkinPath = Path.Combine(
                        gameSkin.InternalSkinsFolderPath,
                        AssetDatabase.AssetPathToGUID(newAssetPath),
                        Path.GetFileName(originalAssetPath)
                    );
                    string skinFolder = Path.GetDirectoryName(newSkinPath);
                    FileUtils.TryCreateDirectory(skinFolder);
                    MoveAsset(originalSkinPath, newSkinPath);
                }
            }
        }
    }
}