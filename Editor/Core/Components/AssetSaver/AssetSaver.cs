using JetBrains.Annotations;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;

namespace Miniclip.ShapeShifter.Saver
{
    public class AssetSaver : UnityEditor.AssetModificationProcessor
    {
        private static bool CanSave => ShapeShifterConfiguration.Instance.HasUnsavedChanges;
        private static bool isSaving;

        [UsedImplicitly]
        public static void OnWillSaveAssets(string[] files)
        {
            if (!ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            if (!isSaving && (ShapeShifter.ActiveGameSkin.HasExternalSkins() || CanSave))
            {
                ShapeShifterLogger.Log($"Pushing changes to {ShapeShifter.ActiveGameName} skin folder");
                isSaving = true;
                AssetSwitcher.OverwriteSelectedSkin(ShapeShifter.ActiveGame);
                isSaving = false;
            }
        }
    }
}