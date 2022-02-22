using JetBrains.Annotations;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
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
            Debug.Log("Saving: " + string.Join("\n", files));

            if (!ShapeShifterConfiguration.IsInitialized())
            {
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