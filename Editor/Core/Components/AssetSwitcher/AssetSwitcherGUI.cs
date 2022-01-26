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

            GUIStyle boxStyle = GUI.skin.GetStyle("Box");
            GUIStyle buttonStyle = GUI.skin.GetStyle("Button");
            GUIStyle labelStyle = GUI.skin.GetStyle("Label");

            using (new GUILayout.VerticalScope(boxStyle))
            {
                GUIStyle titleStyle = new GUIStyle(labelStyle)
                {
                    alignment = TextAnchor.MiddleCenter
                };

                string currentGame = ShapeShifter.ActiveGameName;

                GUILayout.Box($"Current game: {currentGame}", titleStyle);

                highlightedGame = GUILayout.SelectionGrid(
                    highlightedGame,
                    ShapeShifterConfiguration.Instance.GameNames.ToArray(),
                    2,
                    buttonStyle
                );

                GUILayout.Space(10.0f);

                if (GUILayout.Button("Switch!", buttonStyle))
                {
                    AssetSwitcher.SwitchToGame(highlightedGame);
                }

                if (GUILayout.Button(
                        $"Overwrite all {ShapeShifterUtils.GetGameName(highlightedGame)} skins",
                        buttonStyle
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
            }
        }
    }
}