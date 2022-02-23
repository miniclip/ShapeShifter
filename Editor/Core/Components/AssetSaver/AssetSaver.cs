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

            if (!isSaving && (ShapeShifter.ActiveGameSkin.HasExternalSkins() || CanSave))
            {
                ShapeShifterLogger.Log($"Pushing changes to {ShapeShifter.ActiveGameName} skin folder");
                isSaving = true;
                AssetSwitcher.OverwriteSelectedSkin(ShapeShifter.ActiveGameSkin);
                isSaving = false;
            }
        }
    }
}