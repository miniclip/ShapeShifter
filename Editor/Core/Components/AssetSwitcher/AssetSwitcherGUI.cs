using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Switcher
{
    public static class AssetSwitcherGUI
    {
        private static int highlightedGame;

        private static bool showSwitcher = true;

        internal static void OnGUI()
        {
            showSwitcher = EditorGUILayout.Foldout(showSwitcher, "Asset Switcher");

            if (!showSwitcher || ShapeShifterConfiguration.Instance.GameNames.Count == 0)
            {
                return;
            }

            using (new GUILayout.VerticalScope(StyleUtils.BoxStyle))
            {
                GUIStyle titleStyle = new GUIStyle(StyleUtils.LabelStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                string currentGame = ShapeShifter.ActiveGameName;

                GUILayout.Box($"Current game: {currentGame}", titleStyle);

                highlightedGame = GUILayout.SelectionGrid(
                    highlightedGame,
                    ShapeShifterConfiguration.Instance.GameNames.ToArray(),
                    2,
                    StyleUtils.ButtonStyle
                );

                GUILayout.Space(10.0f);

                if (GUILayout.Button("Switch!", StyleUtils.ButtonStyle))
                {
                    AssetSwitcher.SwitchToGame(highlightedGame);
                }
            }
        }

        internal static void OnOverwriteAllSkinsGUI()
        {
            Color backgroundColor = GUI.backgroundColor;

            GUI.backgroundColor = Color.red;

            if (GUILayout.Button(
                    $"Overwrite all {ShapeShifterUtils.GetGameName(highlightedGame)} skins",
                    StyleUtils.ButtonStyle
                ))
            {
                if (EditorUtility.DisplayDialog(
                        "ShapeShifter",
                        $"This will overwrite you current {ShapeShifterUtils.GetGameName(highlightedGame)} assets with the assets currently inside unity. Are you sure?",
                        "Yes, overwrite it.",
                        "Nevermind"
                    ))
                {
                    AssetSwitcher.OverwriteSelectedSkin(highlightedGame);
                }
            }

            GUI.backgroundColor = backgroundColor;
        }
    }
}