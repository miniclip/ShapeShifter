using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public static class AssetSkinnerMenuItem
    {
        [MenuItem("Assets/ShapeShifter/Skin For All Games", priority = 0)]
        private static void SkinSelectedAssets()
        {
            Object[] selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);

            string[] selectedAssetPaths = selectedAssets.Select(AssetDatabase.GetAssetPath).ToArray();

            AssetSkinner.SkinAssets(selectedAssetPaths);
        }

        [MenuItem("Assets/ShapeShifter/Skin exclusive for current Game", priority = 1)]
        private static void ExclusiveSkinSelectedItems()
        {
            Object[] selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);

            string[] selectedAssetPaths = selectedAssets.Select(AssetDatabase.GetAssetPath).ToArray();

            string currentGame = ShapeShifter.ActiveGameName;

            foreach (string selectedAssetPath in selectedAssetPaths)
            {
                AssetSkinner.SkinAssetForGame(selectedAssetPath, currentGame);
            }
        }

        [MenuItem("Assets/ShapeShifter/Remove Skins From All Games", priority = 2)]
        private static void RemoveSkinSelectedAssets()
        {
            Object[] selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);

            string[] selectedAssetPaths = selectedAssets.Select(AssetDatabase.GetAssetPath).ToArray();

            AssetSkinner.RemoveSkins(selectedAssetPaths);
        }

        [MenuItem("Assets/ShapeShifter/Exclude from current Game", priority = 3)]
        private static void ExcludeSelectedItems()
        {
            Object[] selectedAssets = Selection.GetFiltered<Object>(SelectionMode.Assets);

            string[] selectedAssetPaths = selectedAssets.Select(AssetDatabase.GetAssetPath).ToArray();

            string currentGame = ShapeShifter.ActiveGameName;

            foreach (string selectedAssetPath in selectedAssetPaths)
            {
                AssetSkinner.ExcludeFromGame(selectedAssetPath, currentGame);
            }
        }
    }
}