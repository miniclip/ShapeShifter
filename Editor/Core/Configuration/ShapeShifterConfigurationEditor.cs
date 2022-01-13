using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [CustomEditor(typeof(ShapeShifterConfiguration))]
    public class ShapeShifterConfigurationEditor : Editor
    {
        private ReorderableList gameNamesList;
        private SerializedProperty gameNamesProperty;
        private SerializedProperty hasUnsavedChangesProperty;

        private void OnEnable()
        {
            gameNamesProperty = serializedObject.FindProperty("gameNames");
            gameNamesList = new ReorderableList(
                serializedObject,
                gameNamesProperty,
                true,
                true,
                true,
                true
            );

            gameNamesList.drawElementCallback = DrawGameNamesElement;
            gameNamesList.drawHeaderCallback = DrawGameNamesHeader;

            hasUnsavedChangesProperty = serializedObject.FindProperty("hasUnsavedChanges");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            gameNamesList.DoLayoutList();
            EditorGUILayout.PropertyField(hasUnsavedChangesProperty);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGameNamesElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = gameNamesList.serializedProperty.GetArrayElementAtIndex(index);
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
    }
}