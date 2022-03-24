using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Skinner
{
    public class Icons
    {
        public static readonly string defaultIcon = "WelcomeScreen.AssetStoreLogo";
        public static readonly string errorIcon = "console.erroricon";
        public static readonly string trashIcon = "TreeEditor.Trash";
        public static readonly string saveIcon = "d_SaveAs";
        public static readonly string refreshIcon = "d_Refresh";

        public static readonly Dictionary<string, string> iconPerExtension = new Dictionary<string, string>
        {
            {
                ".anim", "AnimationClip Icon"
            },
            {
                ".asset", "ScriptableObject Icon"
            },
            {
                ".controller", "AnimatorController Icon"
            },
            {
                ".cs", "cs Script Icon"
            },
            {
                ".gradle", "TextAsset Icon"
            },
            {
                ".json", "TextAsset Icon"
            },
            {
                ".prefab", "Prefab Icon"
            },
            {
                ".txt", "TextAsset Icon"
            },
            {
                ".unity", "SceneAsset Icon"
            },
            {
                ".xml", "TextAsset Icon"
            },
        };

        public static Texture2D GetIconTexture(string iconName)
        {
            return EditorGUIUtility.FindTexture(iconName);
        }
    }
}