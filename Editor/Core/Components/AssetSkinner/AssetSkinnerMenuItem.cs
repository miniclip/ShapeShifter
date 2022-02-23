using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public static class AssetSkinnerMenuItem
    {
        [MenuItem("Assets/ShapeShifter/Skin Selected Assets")]
        private static void SkinSelectedAssets()
        {
            Object[] selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);

            string[] selectedAssetPaths = selectedAssets.Select(AssetDatabase.GetAssetPath).ToArray();

            AssetSkinner.SkinAssets(selectedAssetPaths);
        }
        
        [MenuItem("Assets/ShapeShifter/Remove Skins From Selected Assets")]
        private static void RemoveSkinSelectedAssets()
        {
            Object[] selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);

            string[] selectedAssetPaths = selectedAssets.Select(AssetDatabase.GetAssetPath).ToArray();

            AssetSkinner.RemoveSkins(selectedAssetPaths);
        }
    }
}