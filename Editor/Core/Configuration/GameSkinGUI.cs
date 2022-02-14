using System;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public static class GameSkinGUI
    {
        public static void DrawGameOperationsGUI(string gameName, bool isActive)
        {
            using (new EditorGUILayout.HorizontalScope(StyleUtils.BoxStyle))
            {
                EditorGUILayout.LabelField(gameName, StyleUtils.LabelStyle);

                OnRenameGUI(gameName);

                OnDuplicateGUI(gameName);

                OnDeleteGUI(isActive, gameName);
            }
        }

#region Rename
        private static GameSkin currentGameSkinInRenameMode;
        private static string newName = string.Empty;

        private static void OnRenameGUI(string gameName)
        {
            GameSkin gameSkinToRename = new GameSkin(gameName);

            if (currentGameSkinInRenameMode != null && currentGameSkinInRenameMode.Equals(gameSkinToRename))
            {
                OnRenameModeOperationGUI(gameName);
            }

            if (GUILayout.Button("Rename", StyleUtils.ButtonStyle))
            {
                currentGameSkinInRenameMode = gameSkinToRename;
                newName = gameName;
            }
        }

        private static void OnRenameModeOperationGUI(string gameName)
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
                        GameSkin gameSkinToRename = new GameSkin(gameName);
                        ShapeShifterConfiguration.Instance.RenameGame(gameSkinToRename, newName);
                    }
                }

                GUI.backgroundColor = Color.green;
                if (GUILayout.Button("Cancel", StyleUtils.ButtonStyle))
                {
                    currentGameSkinInRenameMode = null;
                    newName = string.Empty;
                }
            }

            GUI.backgroundColor = oldColor;
        }

        private static bool ValidateNewName(string oldName, string newName)
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

            if (ShapeShifterConfiguration.Instance.GameNames.Contains(newName))
            {
                ShapeShifterLogger.LogError("Can't rename to an already existing game name");
                return false;
            }

            return true;
        }
#endregion

#region Duplicate
        private static void OnDuplicateGUI(string gameName)
        {
            if (GUILayout.Button("Duplicate", StyleUtils.ButtonStyle))
            {
                DuplicateOperation(gameName);
            }
        }

        private static void DuplicateOperation(string gameName)
        {
            string newName = ShapeShifterUtils.GetUniqueTemporaryGameName(gameName);

            GameSkin sourceGameSkin = new GameSkin(gameName);

            sourceGameSkin.Duplicate(newName);

            ShapeShifterConfiguration.Instance.AddGame(newName);
        }
#endregion

#region Delete
        private static void OnDeleteGUI(bool isActive, string gameName)
        {
            if (GUILayout.Button("Delete", StyleUtils.ButtonStyle))
            {
                DeleteOperation(isActive, gameName);
            }
        }

        private static void DeleteOperation(bool isActive, string gameName)
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