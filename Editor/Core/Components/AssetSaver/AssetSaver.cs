using JetBrains.Annotations;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;

namespace Miniclip.ShapeShifter.Saver
{
    public class AssetSaver : UnityEditor.AssetModificationProcessor
    {
        private static bool isSaving;
        private static bool CanSave => ShapeShifterConfiguration.Instance.HasUnsavedChanges;

        [UsedImplicitly]
        public static void OnWillSaveAssets(string[] files)
        {
            if (!isSaving && (SharedInfo.ActiveGameSkin.HasExternalSkins() || CanSave))
            {
                ShapeShifterLogger.Log($"Pushing changes to {SharedInfo.ActiveGameName} skin folder");
                isSaving = true;
                AssetSwitcher.OverwriteSelectedSkin(SharedInfo.ActiveGame);
                isSaving = false;
            }
        }
    }
}