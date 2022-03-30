using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Saver
{
    public static class AssetSaverGUI
    {
        public static void OnGUI()
        {
            ModifiedAssets modifiedAssets = AssetSaver.GetCurrentModifiedAssetsFromEditorPrefs();

            if (modifiedAssets == null || modifiedAssets?.Values.Count == 0)
            {
                return;
            }

            using (new GUILayout.VerticalScope(StyleUtils.BoxStyle))
            {
                EditorGUILayout.LabelField("Unsaved Changes", EditorStyles.boldLabel);

                foreach (var modifiedAsset in modifiedAssets.Values)
                {
                    DrawUnsavedAssetGUI(modifiedAsset);
                }
            }
        }

        private static void DrawUnsavedAssetGUI(string modifiedAssetPath)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(modifiedAssetPath);

                if (GUILayout.Button("Save"))
                {
                    GameSkin currentGameSkin = ShapeShifter.ActiveGameSkin;

                    string guid = AssetDatabase.AssetPathToGUID(modifiedAssetPath);

                    AssetSkin assetSkin = currentGameSkin.GetAssetSkin(guid);

                    assetSkin.SaveFromUnityToSkinFolder();

                    AssetSaver.RemovedModifiedPath(modifiedAssetPath);
                }

                if (GUILayout.Button("Discard")) { }
            }
        }
    }
}