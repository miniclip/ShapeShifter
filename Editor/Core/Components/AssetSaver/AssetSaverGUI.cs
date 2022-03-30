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
                EditorGUILayout.LabelField("Unsaved Changes", EditorStyles.foldoutHeader);

                foreach (var modifiedAsset in modifiedAssets.Values)
                {
                    EditorGUILayout.LabelField(modifiedAsset);
                }
            }
        }
    }
}