using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Extensions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Miniclip.ShapeShifter.Utils
{
    class GitUtils
    {
        private static readonly string GIT_IGNORE_SHAPESHIFTER_LABEL = "#ShapeShifter";

        internal static string CurrentBranch => RunGitCommand("rev-parse --abbrev-ref HEAD");
        internal static string RepositoryPath => RunGitCommand("rev-parse --git-dir");

        internal static void Stage(string assetPath)
        {
            RunGitCommand($"add {PathUtils.GetFullPath(assetPath)}");
        }
        
        internal static void UnStage(string assetPath)
        {
            RunGitCommand($"reset -- {PathUtils.GetFullPath(assetPath)}");
        }

        internal static bool IsTracked(string assetPath)
        {
            using (Process process = new Process())
            {
                string arguments = $"ls-files --error-unmatch {PathUtils.GetFullPath(assetPath)}";
                int exitCode = RunProcessAndGetExitCode(arguments, process, out string output, out string errorOutput);

                return exitCode != 1;
            }
        }
        
        internal static bool IsStaged(string assetPath)
        {
            using (Process process = new Process())
            {
                string arguments = $"diff --name-only --cached";
                RunProcessAndGetExitCode(arguments, process, out string output, out string errorOutput);

                return output.Contains(PathUtils.GetPathRelativeToProjectFolder(assetPath));
            }
        }

        internal static void Track(string assetPath)
        {
            // if (IsTracked(assetPath))
            // {
            //     ShapeShifterLogger.Log($"{assetPath} already tracked in git.");
            //     return;
            // }

            // if (IsIgnored(assetPath))
            // {
                RemoveFromGitIgnore(assetPath);

                // Stage(PathUtils.GetFullPath(assetPath));
                return;
            // }

            // ShapeShifterLogger.Log($"Could not git track {assetPath}");
        }

        internal static void Untrack(string assetPath, bool addToGitIgnore = false)
        {
            string fullFilePath = PathUtils.GetFullPath(assetPath);
            
            if (IsTracked(fullFilePath))
            { 
                RunGitCommand($"rm -r --cached {fullFilePath}");
            }
            else if(IsStaged(fullFilePath))
            {
                //TODO Unstage not working, file is still in the staging area after this
                UnStage(fullFilePath);
            }
        
            if (addToGitIgnore)
            {
                AddToGitIgnore(assetPath);
            }
        }

        private static string GetAssetIgnoreIdentifier(string assetPath) =>
            $"{GIT_IGNORE_SHAPESHIFTER_LABEL} {AssetDatabase.AssetPathToGUID(assetPath)}";

        private static void AddToGitIgnore(string assetPath)
        {
            if (!TryGetGitIgnoreContent(out List<string> gitIgnoreContent))
            {
                return;
            }
            
            string pathRelativeToProjectFolder = PathUtils.GetPathRelativeToProjectFolder(assetPath);

            if (string.IsNullOrEmpty(pathRelativeToProjectFolder))
                return;
            
            if (gitIgnoreContent.Any(line => line.Contains(pathRelativeToProjectFolder)))
            {
                return;
            }

            gitIgnoreContent.Add(GetAssetIgnoreIdentifier(assetPath));
            gitIgnoreContent.Add($"{pathRelativeToProjectFolder}");
            gitIgnoreContent.Add(GetAssetIgnoreIdentifier(assetPath));
            gitIgnoreContent.Add($"{pathRelativeToProjectFolder}.meta");

            SetGitIgnoreContent(gitIgnoreContent);
        }

        private static void RemoveFromGitIgnore(string pathToAcknowledge)
        {
            if (!TryGetGitIgnoreContent(out List<string> gitIgnoreContent))
            {
                return;
            }
            
            int lineToRemove = gitIgnoreContent.IndexOf(GetAssetIgnoreIdentifier(pathToAcknowledge));

            if (lineToRemove == -1)
            {
                ShapeShifterLogger.LogError($"Couldn't find {pathToAcknowledge} on .gitignore");
                return;
            }

            gitIgnoreContent.RemoveRange(lineToRemove, 4);
            // ShapeShifterLogger.Log($"Removing {pathToAcknowledge} from .gitignore");
            SetGitIgnoreContent(gitIgnoreContent);
            // Stage(pathToAcknowledge);
        }

        private static bool TryGetGitIgnoreContent(out List<string> gitIgnoreContent)
        {
            string gitIgnorePath = GetGitIgnorePath();

            if (!File.Exists(gitIgnorePath))
            {
                // ShapeShifterLogger.LogError($"Could not find .gitignore file at {gitIgnorePath}");
                gitIgnoreContent = null;
                return false;
            }

            gitIgnoreContent = File.ReadAllLines(gitIgnorePath).ToList();
            return true;
        }

        private static void SetGitIgnoreContent(IEnumerable<string> newGitIgnoreContent)
        {
            string gitIgnorePath = GetGitIgnorePath();

            if (!File.Exists(gitIgnorePath))
            {
                // ShapeShifterLogger.LogError($"Could not find .gitignore file at {gitIgnorePath}");
                return;
            }

            File.WriteAllLines(gitIgnorePath, newGitIgnoreContent);

            // Stage(gitIgnorePath);
        }

        private static string GetGitIgnorePath()
        {
            string repositoryPath = RepositoryPath;
            repositoryPath = repositoryPath.Remove(repositoryPath.IndexOf(".git", StringComparison.Ordinal));

            return Path.Combine(repositoryPath, ".gitignore");
        }

        private static bool IsIgnored(string assetPath)
        {
            if (!TryGetGitIgnoreContent(out List<string> gitIgnoreContent))
                return false;

            string folderName = Directory.GetParent(Application.dataPath).Name;
            string fileRelativePath = Path.Combine(folderName, assetPath);

            return gitIgnoreContent.Any(line => line.Contains(fileRelativePath));
        }

        internal static string RunGitCommand(string arguments)
        {
            using (Process process = new Process())
            {
                int exitCode = RunProcessAndGetExitCode(arguments, process, out string output, out string errorOutput);

                if (exitCode == 0)
                {
                    return output;
                }
                else
                {
                    // ShapeShifterLogger.LogError(errorOutput);
                    throw new InvalidOperationException($"Failed to run git {arguments}: Exit Code: {exitCode.ToString()}");
                }
            }
        }

        private static int RunProcessAndGetExitCode(string arguments, Process process, out string output,
            out string errorOutput)
        {
            Debug.Log($"Running: git {arguments}");
            return process.Run(
                application: "git",
                arguments: arguments,
                workingDirectory: Application.dataPath,
                out output,
                out errorOutput
            );
        }
    }
}