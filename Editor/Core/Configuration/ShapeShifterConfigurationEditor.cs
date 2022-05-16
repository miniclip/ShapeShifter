using System.Collections.ObjectModel;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter
{
    [CustomEditor(typeof(ShapeShifterConfiguration))]
    public class ShapeShifterConfigurationEditor : Editor
    {
        private ShapeShifterConfiguration configuration;
        private ReadOnlyCollection<string> gameNames;
        private SerializedProperty gameSkinsProperty;

        private void OnEnable()
        {
            configuration = target as ShapeShifterConfiguration;
        }

        public override void OnInspectorGUI()
        {
            gameNames = new ReadOnlyCollection<string>(configuration.GameNames.AsReadOnly());
            
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.PrefixLabel("Game Names", StyleUtils.LabelStyle);

                for (int index = 0; index < gameNames.Count; index++)
                {
                    string name = gameNames[index];
                    bool isActive = name == ShapeShifter.ActiveGameName;
                    GameSkinGUI.DrawGameOperationsGUI(name, isActive);
                }
            }

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}