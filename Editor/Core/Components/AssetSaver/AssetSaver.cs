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
    [Serializable]
    class ModifiedAssets
    {
        public List<string> Values = new List<string>();
    }

    public class AssetSaver : UnityEditor.AssetModificationProcessor
    {
        private static bool CanSave => ShapeShifterConfiguration.Instance.IsDirty;

        private const string MODIFIED_ASSETS_PERSISTENCE_KEY = "SHAPESHIFTER_UNSAVED_MODIFIED_ASSETS";

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

        internal static void RegisterModifiedPath(string newModifiedAsset)
        {
            var currentModifiedAssets = GetCurrentModifiedAssetsFromEditorPrefs();

            if (currentModifiedAssets == null)
            {
                currentModifiedAssets = new ModifiedAssets();
            }

            if (currentModifiedAssets.Values.Any(modifiedAsset => modifiedAsset == newModifiedAsset))
            {
                return;
            }

            currentModifiedAssets.Values.Add(newModifiedAsset);

            StoreCurrentModifiedAssetsInEditorPrefs(currentModifiedAssets);
        }

        internal static ModifiedAssets GetCurrentModifiedAssetsFromEditorPrefs()
        {
            var modifiedAssetsJson = Persistence.GetString(
                MODIFIED_ASSETS_PERSISTENCE_KEY,
                PersistenceType.MachinePersistent
            );

            if (string.IsNullOrEmpty(modifiedAssetsJson))
            {
                return null;
            }

            ModifiedAssets modifiedAssets = new ModifiedAssets();
            EditorJsonUtility.FromJsonOverwrite(modifiedAssetsJson, modifiedAssets);
            return modifiedAssets;
        }

        private static void StoreCurrentModifiedAssetsInEditorPrefs(ModifiedAssets modifiedAssets)
        {
            Persistence.SetString(
                MODIFIED_ASSETS_PERSISTENCE_KEY,
                EditorJsonUtility.ToJson(modifiedAssets),
                PersistenceType.MachinePersistent
            );
        }

        [MenuItem("Window/Shape Shifter/Clear Modified Assets List")]
        internal static void ClearModifiedAssetsList()
        {
            Persistence.SetString(MODIFIED_ASSETS_PERSISTENCE_KEY, string.Empty, PersistenceType.MachinePersistent);
        }
    }
}