using System;
using System.Collections.ObjectModel;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

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
            var activeGameIndex = ShapeShifter.ActiveGame;
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.PrefixLabel("Game Names", StyleUtils.LabelStyle);

                for (int index = 0; index < gameNames.Count; index++)
                {
                    bool isActive = index == activeGameIndex;
                    DrawGameOperationsGUI(index, isActive);
                }
            }

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGameOperationsGUI(int index, bool isActive)
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
                    if (!isActive)
                    {
                        ShapeShifterConfiguration.RemoveGame(gameName, true);
                    }
                    else
                    {
                        ShapeShifterLogger.LogWarning("Can't deleted a game skin while it is active");
                    }
                }
            }
        }
    }
}