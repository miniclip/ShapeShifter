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
        private static bool CanSave => ShapeShifterConfiguration.Instance.IsDirty;

        private static bool isSaving;

        [UsedImplicitly]
        public static void OnWillSaveAssets(string[] files)
        {
            ShapeShifter.SaveDetected = true;
            return;

            if (Application.isBatchMode)
            {
                return;
            }

            if (!ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                //Skipping saving as we're in prefab mode. Due To the auto save, this method is called every frame
                //The solution is to only save the changes after leaving the prefab mode.
                return;
            }

            SaveToActiveGameSkin();
        }

        public static void SaveToActiveGameSkin()
        {
            if (isSaving || (!ShapeShifter.ActiveGameSkin.HasExternalSkins() && !CanSave))
            {
                return;
            }

            ShapeShifterLogger.Log($"Pushing changes to {ShapeShifter.ActiveGameName} skin folder");
            isSaving = true;
            AssetSwitcher.OverwriteSelectedSkin(ShapeShifter.ActiveGameSkin);
            isSaving = false;
        }

        public static void SaveAssetForGame(string assetPath, string game)
        {
            GameSkin currentGameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(game);

            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            AssetSkin assetSkin = currentGameSkin.GetAssetSkin(guid);

            if (assetSkin == null)
            {
                ShapeShifterLogger.LogWarning($"Something went wrong. Asset ({assetPath} | {guid}) not found in skins folder");
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
                ShapeShifterLogger.LogWarning($"Something went wrong. Asset ({assetPath} | {guid}) not found in skins folder");
                return;
            }
            
            assetSkin.CopyFromSkinFolderToUnity();

            AssetDatabase.Refresh();
            
            UnsavedAssetsManager.RemovedModifiedPath(assetPath);
        }
    }
}