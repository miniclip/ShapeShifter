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

                GUILayoutOption[] width200 = new GUILayoutOption[] {GUILayout.Width(200)};

                EditorGUILayout.BeginHorizontal(width200);
                if (GUILayout.Button("Save"))
                {
                    GameSkin currentGameSkin = ShapeShifter.ActiveGameSkin;

                    string guid = AssetDatabase.AssetPathToGUID(modifiedAssetInfo.assetPath);

                    AssetSkin assetSkin = currentGameSkin.GetAssetSkin(guid);

                    assetSkin.SaveFromUnityToSkinFolder();

                    UnsavedAssetsManager.RemovedModifiedPath(modifiedAssetInfo.assetPath);
                }

                if (GUILayout.Button("Discard"))
                {
                    
                }
                
                EditorGUILayout.EndHorizontal();

            }
        }
    }
}