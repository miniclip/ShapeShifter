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

                OnRenameGUI(gameName, index);

                OnDuplicateGUI(gameName);

                OnDeleteGUI(isActive, gameName);
            }
        }

#region Rename
        private int renamingIndex = -1;
        private string newName = string.Empty;

        private void OnRenameGUI(string gameName, int index)
        {
            if (index == renamingIndex)
            {
                RenameOperation(gameName, index);
                return;
            }

            if (GUILayout.Button("Rename", StyleUtils.ButtonStyle))
            {
                renamingIndex = index;
                newName = gameName;
            }
        }

        private void RenameOperation(string gameName, int index)
        {
            var oldColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.white;
            using (new EditorGUILayout.HorizontalScope())
            {
                newName = EditorGUILayout.TextField(newName);
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("OK", StyleUtils.ButtonStyle))
                {
                    if (ValidateNewName(gameName, newName))
                    {
                        configuration.RenameGame(index, newName);
                        renamingIndex = -1;
                    }
                }

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Cancel", StyleUtils.ButtonStyle))
                {
                    renamingIndex = -1;
                    newName = string.Empty;
                }
            }

            GUI.backgroundColor = oldColor;
        }

        private bool ValidateNewName(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(newName))
            {
                ShapeShifterLogger.LogWarning("Invalid name");
                return false;
            }

            if (string.Equals(oldName, newName, StringComparison.OrdinalIgnoreCase))
            {
                ShapeShifterLogger.LogWarning("Can't rename to same name");
                return false;
            }

            if (configuration.GameNames.Contains(newName))
            {
                ShapeShifterLogger.LogError("Can't rename to an already existing game name");
                return false;
            }

            return true;
        }
#endregion

#region Duplicate
        private void OnDuplicateGUI(string gameName)
        {
            if (GUILayout.Button("Duplicate", StyleUtils.ButtonStyle))
            {
                DuplicateOperation(gameName);
            }
        }

        private void DuplicateOperation(string gameName)
        {
            string newGame = ShapeShifterUtils.GetUniqueTemporaryGameName(gameName);

            GameSkin gameSkin = new GameSkin(gameName);
            gameSkin.Duplicate(newGame);

            ShapeShifterConfiguration.Instance.AddGame(newGame);
        }
#endregion

#region Delete
        private void OnDeleteGUI(bool isActive, string gameName)
        {
            if (GUILayout.Button("Delete", StyleUtils.ButtonStyle))
            {
                DeleteOperation(isActive, gameName);
            }
        }

        private void DeleteOperation(bool isActive, string gameName)
        {
            if (!isActive)
            {
                ShapeShifterConfiguration.Instance.RemoveGame(gameName, true);
            }
            else
            {
                ShapeShifterLogger.LogWarning("Can't deleted a game skin while it is active");
            }
        }
#endregion
    }
}