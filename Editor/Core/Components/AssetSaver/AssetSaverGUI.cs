using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Saver
{
    public static class AssetSaverGUI
    {
        public static void OnGUI()
        {
            ModifiedAssets modifiedAssets = UnsavedAssetsManager.GetCurrentModifiedAssetsFromEditorPrefs();

            if (modifiedAssets == null || modifiedAssets?.Values.Count == 0)
            {
                return;
            }

            using (new GUILayout.VerticalScope(StyleUtils.BoxStyle))
            {
                EditorGUILayout.LabelField("Unsaved Changes", EditorStyles.boldLabel);
                EditorGUILayout.Separator();

                foreach (var modifiedAsset in modifiedAssets.Values)
                {
                    DrawUnsavedAssetGUI(modifiedAsset);
                    EditorGUILayout.Separator();
                }
            }
        }

        private static void DrawUnsavedAssetGUI(ModifiedAssetInfo modifiedAssetInfo)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField(modifiedAssetInfo.assetPath);
                EditorGUILayout.LabelField(modifiedAssetInfo.modificationType.ToString());
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
                if (GUILayout.Button("Save"))
                {
                    AssetSaver.SaveAssetForGame(modifiedAssetInfo.assetPath, ShapeShifter.ActiveGame);
                }

                if (GUILayout.Button("Discard")) { }

                EditorGUILayout.EndHorizontal();
            }
        }

        
    }
}