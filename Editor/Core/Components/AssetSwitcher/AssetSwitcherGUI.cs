using System.Collections.Generic;
using System.Linq;
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
            if (!ShapeShifterConfiguration.IsInitialized())
                return;

            GUILayout.Space(10.0f);

            HighlightedGame = EditorGUILayout.Popup(
                "Switch To",
                HighlightedGame,
                GameNames.ToArray()
            );

            if (GUILayout.Button("Switch!", StyleUtils.ButtonStyle))
            {
                GameSkin gameSkin = new GameSkin(GameNames[HighlightedGame]);
                AssetSwitcher.SwitchToGame(gameSkin);
                GUIUtility.ExitGUI();
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
        }
    }
}