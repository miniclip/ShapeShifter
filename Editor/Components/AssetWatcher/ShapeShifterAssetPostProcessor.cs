using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter
{
    public class ShapeShifterAssetPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (ShapeShifterConfiguration.Instance == null)
                return;
            
            //changed
            foreach (string importedAsset in importedAssets)
            {
                ShapeShifter.OnImportedAsset(importedAsset);
            }

            //renamed
            for (int index = 0; index < movedAssets.Length; index++)
            {
                string newName = movedAssets[index];
                string oldName = movedFromAssetPaths[index];
                ShapeShifter.OnAssetRenamed(newName, oldName);
            }

            foreach (string deletedAsset in deletedAssets)
            {
                if (AssetSkinner.IsSkinned(deletedAsset))
                {
                    ShapeShifterLogger.LogWarning(
                        "You deleted an asset that currently has skins. Shapeshifter will recover it the next chance it has."
                    );
                }
            }
        }
    }
}