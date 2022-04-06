using JetBrains.Annotations;
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

        public static void SaveAssetForGame(string assetPath, string game)
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

        public static void DiscardAssetChanges(string assetPath, string game)
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