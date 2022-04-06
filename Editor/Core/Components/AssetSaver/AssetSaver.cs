using System;
using System.IO;
using JetBrains.Annotations;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter.Saver
{
    public class AssetSaver : UnityEditor.AssetModificationProcessor
    {
        [UsedImplicitly]
        public static void OnWillSaveAssets(string[] files)
        {
            if (!ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            if (!Settings.IsAutoSaveEnabled)
            {
                return;
            }

            SaveToActiveGameSkin(forceSave: false);
        }

        public static void SaveToActiveGameSkin(bool forceSave)
        {
            if (forceSave || UnsavedAssetsManager.HasUnsavedChanges())
            {
                AssetSwitcher.OverwriteSelectedSkin(ShapeShifter.ActiveGameSkin);
            }
        }

        public static void SaveModificationForGame(ModifiedAssetInfo modifiedAssetInfo, string game)
        {
            switch (modifiedAssetInfo.skinType)
            {
                case SkinType.Internal:
                    SaveInternalAssetForGame(modifiedAssetInfo.assetPath, game);
                    return;
                case SkinType.External:
                    SaveExternalAssetForGame(modifiedAssetInfo.assetPath, game);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void SaveExternalAssetForGame(string assetPath, string game)
        {
            GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(game);
            string relativePath = ExternalAssetSkinner.ConvertToRelativePath(assetPath);
            string skinnedVersionFolder = Path.Combine(
                gameSkin.ExternalSkinsFolderPath,
                ExternalAssetSkinner.GenerateKeyFromRelativePath(relativePath));
            
            AssetSwitcherOperations.CopyFromOriginToSkinnedExternal(new DirectoryInfo(skinnedVersionFolder));
            
            UnsavedAssetsManager.RemoveByPath(assetPath);
        }

        private static void SaveInternalAssetForGame(string assetPath, string game)
        {
            GameSkin currentGameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(game);

            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            AssetSkin assetSkin = currentGameSkin.GetAssetSkin(guid);

            if (assetSkin == null)
            {
                ShapeShifterLogger.LogWarning(
                    $"Something went wrong. Asset ({assetPath} | {guid}) not found in skins folder"
                );
                return;
            }

            assetSkin.CopyFromUnityToSkinFolder();

            UnsavedAssetsManager.RemoveByPath(assetPath);
        }

        public static void DiscardModificationForGame(ModifiedAssetInfo modifiedAssetInfo, string game)
        {
            switch (modifiedAssetInfo.skinType)
            {
                case SkinType.Internal:
                    DiscardInternalModificationForGame(modifiedAssetInfo.assetPath, game);
                    break;
                case SkinType.External:
                    DiscardExternalModificationForGame(modifiedAssetInfo.assetPath, game);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DiscardExternalModificationForGame(string assetPath, string game)
        {
            GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(game);
            string relativePath = ExternalAssetSkinner.ConvertToRelativePath(assetPath);
            string skinnedVersionFolder = Path.Combine(
                gameSkin.ExternalSkinsFolderPath,
                ExternalAssetSkinner.GenerateKeyFromRelativePath(relativePath));
            
            AssetSwitcherOperations.CopyFromSkinnedExternalToOrigin(new DirectoryInfo(skinnedVersionFolder));
            
            UnsavedAssetsManager.RemoveByPath(assetPath);
        }

        private static void DiscardInternalModificationForGame(string assetPath, string game)
        {
            GameSkin currentGameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(game);

            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            AssetSkin assetSkin = currentGameSkin.GetAssetSkin(guid);

            if (assetSkin == null)
            {
                ShapeShifterLogger.LogWarning(
                    $"Something went wrong. Asset ({assetPath} | {guid}) not found in skins folder"
                );
                return;
            }

            assetSkin.CopyFromSkinFolderToUnity();

            AssetDatabase.Refresh();

            UnsavedAssetsManager.RemoveByPath(assetPath);
        }
    }
}