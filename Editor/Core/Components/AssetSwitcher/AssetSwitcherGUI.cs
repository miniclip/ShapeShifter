using System.Linq;
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

            if (!showSwitcher || !ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            GUIStyle boxStyle = StyleUtils.BoxStyle;
            using (new GUILayout.VerticalScope(boxStyle))
            {
                using (new GUILayout.VerticalScope())
                {
                    OnActiveGameGUI();

                    OnSwitchToGUI();
                }
            }
        }

        private static void OnSwitchToGUI()
        {
            if (!ShapeShifterConfiguration.IsInitialized())
                return;

            GUILayout.Space(10.0f);

            highlightedGame = EditorGUILayout.Popup(
                "Switch To",
                highlightedGame,
                ShapeShifterConfiguration.Instance.GameNames.ToArray()
            );

            if (GUILayout.Button("Switch!", StyleUtils.ButtonStyle))
            {
                AssetSwitcher.SwitchToGame(highlightedGame);
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