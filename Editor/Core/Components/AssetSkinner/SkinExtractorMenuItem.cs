using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public class SkinExtractorMenuItem
    {
        [MenuItem("Assets/ShapeShifter/Extract as skin to...", priority = 0)]
        private static void SkinSelectedAssets()
        {
            Object[] selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);

            string[] selectedAssetPaths = selectedAssets.Select(AssetDatabase.GetAssetPath).ToArray();

            string targetFolder =
                EditorUtility.OpenFolderPanel("Choose where to extract skin to...", Application.dataPath, "");

            if (string.IsNullOrEmpty(targetFolder))
            {
                return;
            }
            
            foreach (string assetPath in selectedAssetPaths)
            {
                SkinExtractor.ExtractAsSkin(assetPath, targetFolder);
            }
        }

    }
}