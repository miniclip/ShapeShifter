using System.IO;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public static class ShapeShifterEditorPrefs
    {
        private const string SHAPESHIFTER_KEY = "shapeshifter";

        private static string UniqueProjectID => Path.GetFullPath(Application.dataPath);

        private static string GetProjectSpecificKey(string key) => $"{UniqueProjectID}_{SHAPESHIFTER_KEY}_{key}";

        [MenuItem("Window/Shape Shifter/Clear Editor Prefs", false, 71)]
        public static void OpenShapeShifter() => EditorPrefs.DeleteAll();

        internal static bool HasKey(string key) => EditorPrefs.HasKey(GetProjectSpecificKey(key));

        internal static int GetInt(string key) => EditorPrefs.GetInt(GetProjectSpecificKey(key));

        internal static void SetInt(string key, int value) => EditorPrefs.SetInt(GetProjectSpecificKey(key), value);

        public static bool GetBool(string key) => EditorPrefs.GetBool(GetProjectSpecificKey(key));

        public static void SetBool(string key, bool value) => EditorPrefs.SetBool(GetProjectSpecificKey(key), value);
    }
}