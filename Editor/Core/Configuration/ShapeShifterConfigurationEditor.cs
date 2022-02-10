using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [CustomEditor(typeof(ShapeShifterConfiguration))]
    public class ShapeShifterConfigurationEditor : Editor
    {
        private SerializedProperty gameSkinsProperty;
        private ShapeShifterConfiguration configuration;
        private ReadOnlyCollection<string> gameNames;

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
                    string gameName = gameNames[index];
                    using (new EditorGUILayout.HorizontalScope(StyleUtils.BoxStyle))
                    {
                        EditorGUILayout.LabelField(gameName, StyleUtils.LabelStyle);

                        if (GUILayout.Button("Rename", StyleUtils.ButtonStyle))
                        {
                            throw new NotImplementedException();
                        }

                        if (GUILayout.Button("Duplicate", StyleUtils.ButtonStyle))
                        {
                            string newGame = ShapeShifterUtils.GetUniqueTemporaryGameName(gameName);

                            GameSkin gameSkin = new GameSkin(gameName);
                            gameSkin.Duplicate(newGame);
                            
                            ShapeShifterConfiguration.AddGame(newGame);
                        }

                        if (GUILayout.Button("Delete", StyleUtils.ButtonStyle))
                        {
                            ShapeShifterConfiguration.RemoveGame(gameName, true);
                        }
                    }
                }
            }

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}