using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public class PreMergeCheckGUI
    {
        private static bool showSwitcher = true;

        private static string targetBranch = "develop";

        public static void OnGUI()
        {
            showSwitcher = EditorGUILayout.Foldout(showSwitcher, "Pre Merge Conflict Checker");

            if (!showSwitcher || !ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            GUIStyle boxStyle = StyleUtils.BoxStyle;
            using (new GUILayout.VerticalScope(boxStyle))
            {
                targetBranch = EditorGUILayout.TextField(targetBranch);

                if (GUILayout.Button("Check for possible conflits"))
                {
                    var result = PreMergeCheck.HasShapeShifterConflictsWith(targetBranch);
                }
            }
        }
    }
}