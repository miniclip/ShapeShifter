using System;
using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public class PreMergeCheckGUI
    {
        private static bool showSwitcher = true;
        private static string targetBranch = "develop";
        private static List<string> changedFiles;

        public static void OnGUI()
        {
            showSwitcher = EditorGUILayout.Foldout(
                showSwitcher,
                "Pre Merge Conflict Checker"
            );
            if (!showSwitcher || !ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            using (new GUILayout.VerticalScope(StyleUtils.BoxStyle, Array.Empty<GUILayoutOption>()))
            {
                targetBranch = EditorGUILayout.TextField(targetBranch);
                if (GUILayout.Button("Check for possible conflicts"))
                {
                    PreMergeCheck.HasShapeShifterConflictsWith(
                        GitUtils.GetCurrentBranch(new DirectoryInfo(GitUtils.MainRepositoryPath)),
                        targetBranch,
                        out PreMergeCheckGUI.changedFiles
                    );
                }

                List<string> changedFiles = PreMergeCheckGUI.changedFiles;

                if (changedFiles == null || changedFiles.Count <= 0)
                {
                    return;
                }

                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.PrefixLabel("Possible Merge Conflicts");
                    foreach (string changedFile in PreMergeCheckGUI.changedFiles)
                    {
                        GUILayout.Label(changedFile);
                    }
                }
            }
        }
    }
}