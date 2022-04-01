using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

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

            // PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            // if (stage != null)
            // {
            //     //Skipping saving as we're in prefab mode. Due To the auto save, this method is called every frame
            //     //The solution is to only save the changes after leaving the prefab mode.
            //     return;
            // }

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

            UnsavedAssetsManager.RemovedModifiedPath(assetPath);
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

            UnsavedAssetsManager.RemovedModifiedPath(assetPath);
        }
    }
}