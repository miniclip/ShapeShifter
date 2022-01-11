// Decompiled with JetBrains decompiler
// Type: Miniclip.ShapeShifter.Utils.ShapeShifterEditorPrefs
// Assembly: MUShapeShifter.Editor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 823D6AC0-B8BC-4CA3-8882-C67921F772A0
// Assembly location: /Users/joao.vieira/Work/ACPT/mutestautomationci/MUTA/Library/ScriptAssemblies/MUShapeShifter.Editor.dll

using System.IO;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public class ShapeShifterEditorPrefs
    {
        private const string SHAPESHIFTER_KEY = "shapeshifter";

        private static string UniqueProjectID => Path.GetFullPath(Application.dataPath);

        private static string GetProjectSpecificKey(string key) => UniqueProjectID + "_shapeshifter_" + key;

        [MenuItem("Window/Shape Shifter/Clear Editor Prefs", false, 71)]
        public static void OpenShapeShifter() => EditorPrefs.DeleteAll();

        internal static bool HasKey(string key) => EditorPrefs.HasKey(GetProjectSpecificKey(key));

        internal static int GetInt(string key) => EditorPrefs.GetInt(GetProjectSpecificKey(key));

        internal static void SetInt(string key, int value) => EditorPrefs.SetInt(GetProjectSpecificKey(key), value);

        public static bool GetBool(string key) => EditorPrefs.GetBool(GetProjectSpecificKey(key));

        public static void SetBool(string key, bool value) => EditorPrefs.SetBool(GetProjectSpecificKey(key), value);
    }
}