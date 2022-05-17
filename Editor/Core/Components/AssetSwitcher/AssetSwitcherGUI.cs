using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Saver;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Switcher
{
    public static class AssetSwitcherGUI
    {
        private static List<string> GameNames => ShapeShifterConfiguration.Instance.GameNames;

        private static int HighlightedGame { get; set; }

        internal static void OnGUI()
        {
            GUIStyle boxStyle = StyleUtils.BoxStyle;
            using (new GUILayout.VerticalScope(boxStyle))
            {
                OnActiveGameGUI();

                OnSwitchToGUI();
            }
        }

        private static void OnSwitchToGUI()
        {
            GUILayout.Space(10.0f);

            EditorGUILayout.PrefixLabel("Switch To:");
            using (new EditorGUILayout.HorizontalScope())
            {
                HighlightedGame = EditorGUILayout.Popup(
                    HighlightedGame,
                    GameNames.ToArray(),
                    StyleUtils.ButtonStyle
                );

                if (GUILayout.Button("Switch!", StyleUtils.ButtonStyle))
                {
                    GameSkin gameSkin = new GameSkin(GameNames[HighlightedGame]);
                    AssetSwitcher.SwitchToGame(gameSkin);
                    GUIUtility.ExitGUI();
                }
            }
        }

        private static void OnActiveGameGUI()
        {
            GUIStyle titleStyle = new GUIStyle(StyleUtils.LabelStyle)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 20,
            };

            GUILayout.Box($"Active game: {ShapeShifter.ActiveGameName}", titleStyle);

            GUIContent forceSaveGUIContent = new GUIContent(
                $"Force Save To {ShapeShifter.ActiveGameName}",
                Icons.GetIconTexture(Icons.saveIcon),
                $"Save your project assets into {ShapeShifter.ActiveGameName} folder"
            );
            if (GUILayout.Button(forceSaveGUIContent, StyleUtils.ButtonStyle))
            {
                AssetSaver.SaveToActiveGameSkin(forceSave: true);
            }

            GUILayout.Space(5.0f);

            GUIContent refreshGUIContent = new GUIContent(
                $"Refresh {ShapeShifter.ActiveGameName}",
                Icons.GetIconTexture(Icons.refreshIcon),
                $"This will replace your project assets with the current version in {ShapeShifter.ActiveGameName} folder"
            );
            if (GUILayout.Button(refreshGUIContent))
            {
                AssetSwitcher.RestoreActiveGame();
            }
        }
    }
}