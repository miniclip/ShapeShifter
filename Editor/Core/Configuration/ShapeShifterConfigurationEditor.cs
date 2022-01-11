using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [CustomEditor(typeof(ShapeShifterConfiguration))]
    public class ShapeShifterConfigurationEditor : Editor
    {
        private SerializedProperty gameNamesProperty;
        private ReorderableList gameNamesList;

        private void DrawGameNamesElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = this.gameNamesList.serializedProperty.GetArrayElementAtIndex(index);
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

        private void DrawGameNamesHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Game names:");
        }

        private void OnEnable()
        {
            this.gameNamesProperty = this.serializedObject.FindProperty("gameNames");

            this.gameNamesList = new ReorderableList(
                this.serializedObject,
                this.gameNamesProperty,
                true,
                true,
                true,
                true
            );

            this.gameNamesList.drawElementCallback = this.DrawGameNamesElement;
            this.gameNamesList.drawHeaderCallback = this.DrawGameNamesHeader;
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            this.gameNamesList.DoLayoutList();
            this.serializedObject.ApplyModifiedProperties();
        }
    }
}