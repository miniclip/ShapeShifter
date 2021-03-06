using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    internal static class StyleUtils
    {
        internal static GUIStyle BoxStyle => GUI.skin.GetStyle("Box");
        internal static GUIStyle ButtonStyle => GUI.skin.GetStyle("Button");
        internal static GUIStyle LabelStyle => GUI.skin.GetStyle("Label");

        internal static GUIStyle GetCopy(this GUIStyle style) => new GUIStyle(style);
    }
}