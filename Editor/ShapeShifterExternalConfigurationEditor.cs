using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Miniclip.MUShapeShifter {
    
    [CustomEditor(typeof(ShapeShifterConfiguration))]
    public class ShapeShifterExternalConfigurationEditor : Editor {
        private SerializedProperty externalAssetsProperty;
        private ReorderableList externalAssetsList;
    
        private void DrawGameNamesElement(Rect rect, int index, bool isActive, bool isFocused) {        
            SerializedProperty element = this.externalAssetsList.serializedProperty.GetArrayElementAtIndex(index);
            element.stringValue = EditorGUI.TextField(
                new Rect(
                    rect.x, 
                    rect.y, 
                    EditorGUIUtility.currentViewWidth - 40.0f, // buffer to account for scrollbars, padding, etc 
                    EditorGUIUtility.singleLineHeight
                ), 
                element.stringValue
            );
        }

        private void DrawGameNamesHeader(Rect rect) {
            EditorGUI.LabelField(rect, "Skinned external assets:");
        }
        
        private void OnEnable() {
            this.externalAssetsProperty = this.serializedObject.FindProperty("skinnedExternalAssetPaths");

            this.externalAssetsList = new ReorderableList(
                this.serializedObject,
                this.externalAssetsProperty, 
                true, 
                true, 
                true, 
                true
            );

            this.externalAssetsList.drawElementCallback = this.DrawGameNamesElement;
            this.externalAssetsList.drawHeaderCallback = this.DrawGameNamesHeader;
        }

        public override void OnInspectorGUI() {
            this.serializedObject.Update();
            
            this.externalAssetsList.DoLayoutList();
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}