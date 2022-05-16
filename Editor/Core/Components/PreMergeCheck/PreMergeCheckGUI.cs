using System;
using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public static class PreMergeCheckGUI
    {
        private static string DIALOG_TITLE = "ShapeShifter Pre Merge Checker";

        private static bool showSection = true;
        private static string branchToMergeIntoCurrentBranch = "develop";
        private static List<string> changedFiles = null;
        private static bool isTargetBranchOnLatest = false;
        private static int conflictCheckResult;

        public static void OnGUI()
        {
            showSection = EditorGUILayout.Foldout(
                showSection,
                "Pre Merge Conflict Checker"
            );
            if (!showSection || !ShapeShifterConfiguration.IsInitialized())
            {
                return;
            }

            using (new GUILayout.VerticalScope(StyleUtils.BoxStyle, Array.Empty<GUILayoutOption>()))
            {
                EditorGUILayout.PrefixLabel("Target Branch");
                branchToMergeIntoCurrentBranch = EditorGUILayout.TextField(branchToMergeIntoCurrentBranch);

                if (GUILayout.Button("Check for possible conflicts"))
                {
                    conflictCheckResult = 0;

                    if (string.IsNullOrEmpty(branchToMergeIntoCurrentBranch))
                    {
                        return;
                    }

                    EditorUtility.DisplayProgressBar(
                        "Shape Shifter Pre Merge Checker",
                        "Checking if VPN is connected",
                        0
                    );

                    if (!GitUtils.IsConnectedToVPN())
                    {
                        EditorUtility.ClearProgressBar();

                        EditorUtility.DisplayDialog(
                            DIALOG_TITLE,
                            "Please be connected to VPN when using the check",
                            "OK"
                        );

                        return;
                    }

                    if (!IsBranchOnLatest(branchToMergeIntoCurrentBranch))
                    {
                        return;
                    }

                    conflictCheckResult = TryGetConflictResult();

                    EditorUtility.ClearProgressBar();
                }

                switch (conflictCheckResult)
                {
                    case 0:
                        EditorGUILayout.LabelField("No possible conflict detected");
                        break;

                    case 1:
                        ShowPossibleConflictsGUI();
                        break;
                }
            }
        }

        private static int TryGetConflictResult()
        {
            EditorUtility.DisplayProgressBar(
                "Shape Shifter Pre Merge Check",
                $"Checking Checking for possible conflicts",
                0.5f
            );

            return PreMergeCheck.HasShapeShifterConflictsBetweenBranches(
                GitUtils.GetCurrentBranch(new DirectoryInfo(GitUtils.MainRepositoryPath)),
                branchToMergeIntoCurrentBranch,
                out changedFiles
            );
        }

        private static bool IsBranchOnLatest(string branch)
        {
            EditorUtility.DisplayProgressBar(
                "Shape Shifter Pre Merge Checker",
                $"Checking is {branch} is up to date with remote",
                0.2f
            );

            string result = GitUtils.RunGitCommand($"git diff {branch} origin/{branch} --name-only");

            bool isOnLatest = string.IsNullOrEmpty(result);

            if (!isOnLatest)
            {
                EditorUtility.ClearProgressBar();

                EditorUtility.DisplayDialog(
                    DIALOG_TITLE,
                    $"Your local branch {branch} is not up to date with remote",
                    "OK"
                );
            }

            return isOnLatest;
        }

        private static void ShowPossibleConflictsGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Possible Merge Conflicts");

                if (changedFiles?.Count > 0)
                {
                    foreach (string changedFile in changedFiles)
                    {
                        EditorGUILayout.LabelField(changedFile, StyleUtils.LabelStyle);
                    }
                }
            }
        }
    }
}