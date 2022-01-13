using JetBrains.Annotations;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;

namespace Miniclip.ShapeShifter.Saver
{
    public class AssetSaver : UnityEditor.AssetModificationProcessor
    {
        private static bool isSaving;

        [UsedImplicitly]
        public static void OnWillSaveAssets(string[] files)
        {
            if (isSaving || !ShapeShifterConfiguration.Instance.HasUnsavedChanges)
            {
                return;
            }

            ShapeShifterLogger.Log($"Pushing changes to {SharedInfo.ActiveGameName} skin folder");
            isSaving = true;
            AssetSwitcher.OverwriteSelectedSkin(SharedInfo.ActiveGame);
            isSaving = false;
        }
    }
}