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
        private static readonly string git_status_modified = "M";
        private static readonly string git_status_added = "A";
        private static readonly string git_status_deleted = "D";
        private static readonly string git_status_renamed = "R";
        private static readonly string git_status_updated = "U";
        private static readonly string git_status_untracked = "??";
        private static readonly string GIT_IGNORE_SHAPESHIFTER_LABEL = "#ShapeShifter";

        private static string GitWorkingDirectory = string.Empty;

        internal static string CurrentBranch => RunGitCommand("rev-parse --abbrev-ref HEAD");
        
        internal static string RepositoryPath
        {
            get
            {
                string path;
                if (string.IsNullOrEmpty(GitWorkingDirectory))
                {
                    path = RunGitCommand("rev-parse --git-dir", Application.dataPath);
                    path = path.Remove(path.IndexOf(".git", StringComparison.Ordinal));
                    GitWorkingDirectory = path;
                }

                return GitWorkingDirectory;
            }
        }

        private static string GitIgnorePath() => Path.Combine(RepositoryPath, ".gitignore");

        internal static bool IsStaged(string assetPath)
        {
            using (Process process = new Process())
            {
                string arguments = $"diff --name-only --cached";
                RunProcessAndGetExitCode(arguments, process, out string output, out string errorOutput);
                return output.Contains(PathUtils.GetPathRelativeToProjectFolder(assetPath));
            }
        }

        internal static void Stage(string assetPath) => RunGitCommand($"add \"{PathUtils.GetFullPath(assetPath)}\"");

        internal static void UnStage(string assetPath) => RunGitCommand($"restore --staged \"{PathUtils.GetFullPath(assetPath)}\"");

        internal static bool IsTracked(string assetPath)
        {
            using (Process process = new Process())
            {
                string arguments = $"ls-files --error-unmatch {PathUtils.GetFullPath(assetPath)}";
                int exitCode = RunProcessAndGetExitCode(arguments, process, out string output, out string errorOutput);

                return exitCode != 1;
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
            else if (IsStaged(fullFilePath))
            {
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
            if (!TryGetGitIgnoreLines(out List<string> gitIgnoreContent))
            {
                return;
            }

            string pathRelativeToProjectFolder = PathUtils.GetPathRelativeToProjectFolder(assetPath);

            if (string.IsNullOrEmpty(pathRelativeToProjectFolder))
                return;

            string assetIgnoreIdentifier = GetAssetIgnoreIdentifier(assetPath);

            if (gitIgnoreContent.Any(line => line.Equals(assetIgnoreIdentifier, StringComparison.Ordinal)))
            {
                // a possible rename has happened, we should simply replace the existing ignore entry with this new one
                gitIgnoreContent[gitIgnoreContent.IndexOf(assetIgnoreIdentifier) + 1] = pathRelativeToProjectFolder;
                return;
            }

            gitIgnoreContent.Add(assetIgnoreIdentifier);
            gitIgnoreContent.Add($"{pathRelativeToProjectFolder}");

            SetGitIgnoreContent(gitIgnoreContent);
        }

        private static void RemoveFromGitIgnore(string pathToAcknowledge)
        {
            if (!TryGetGitIgnoreLines(out List<string> gitIgnoreContent))
            {
                return;
            }

            int lineToRemove = gitIgnoreContent.IndexOf(GetAssetIgnoreIdentifier(pathToAcknowledge));

            if (lineToRemove == -1)
            {
                ShapeShifterLogger.LogError($"Couldn't find {pathToAcknowledge} on .gitignore");
                return;
            }

            gitIgnoreContent.RemoveRange(lineToRemove, 2);

            // ShapeShifterLogger.Log($"Removing {pathToAcknowledge} from .gitignore");
            SetGitIgnoreContent(gitIgnoreContent);

            // Stage(pathToAcknowledge);
        }

        private static bool TryGetGitIgnoreLines(out List<string> gitIgnoreContent)
        {
            string gitIgnorePath = GitIgnorePath();

            if (!File.Exists(gitIgnorePath))
            {
                ShapeShifterLogger.LogError($"Could not find .gitignore file at {gitIgnorePath}");
                gitIgnoreContent = null;
                return false;
            }

            gitIgnoreContent = File.ReadAllLines(gitIgnorePath).ToList();
            return true;
        }

        private static void SetGitIgnoreContent(IEnumerable<string> newGitIgnoreContent)
        {
            string gitIgnorePath = GitIgnorePath();

            if (!File.Exists(gitIgnorePath))
            {
                ShapeShifterLogger.LogError($"Could not find .gitignore file at {gitIgnorePath}");
                return;
            }

            File.WriteAllLines(gitIgnorePath, newGitIgnoreContent);

            Stage(gitIgnorePath);
        }


        private static bool IsIgnored(string assetPath)
        {
            if (!TryGetGitIgnoreLines(out List<string> gitIgnoreContent))
                return false;

            string folderName = Directory.GetParent(Application.dataPath).Name;
            string fileRelativePath = Path.Combine(folderName, assetPath);

            return gitIgnoreContent.Any(line => line.Contains(fileRelativePath));
        }

        public static void DiscardChanges(string filePath)
        {
            RunGitCommand($"checkout \"{PathUtils.GetFullPath(filePath)}\"");
        }
        
        internal static List<string> GetUncommittedDeletedFiles()
        {
            string[] uncommittedFiles = GetUncommittedChangedFiles();
            uncommittedFiles = uncommittedFiles.Where(uncommittedFile => uncommittedFile.StartsWith(git_status_deleted)).ToArray();
            for (int i = 0; i < uncommittedFiles.Length; i++)
            {
                uncommittedFiles[i] = uncommittedFiles[i].TrimStart(Convert.ToChar(git_status_deleted), ' ');
            }

            return uncommittedFiles.ToList();
        }

        private static string[] GetUncommittedChangedFiles()
        {
            string status = RunGitCommand("status --porcelain -u");
            string[] splitted = status.Split('\n');

            for (int index = 0; index < splitted.Length; index++)
            {
                splitted[index] = splitted[index].Trim();
            }

            return splitted;
        }
        
        internal static string RunGitCommand(string arguments, string workingDirectory = null)
        {
            using (Process process = new Process())
            {
                int exitCode = RunProcessAndGetExitCode(
                    arguments,
                    process,
                    out string output,
                    out string errorOutput,
                    workingDirectory
                );

                if (exitCode == 0)
                {
                    return output;
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Failed to run git {arguments}: Exit Code: {exitCode.ToString()}"
                    );
                }
            }
        }

        private static int RunProcessAndGetExitCode(string arguments, Process process, out string output,
            out string errorOutput, string workingDirectory = null)
        {
            if (workingDirectory == null)
            {
                workingDirectory = RepositoryPath;
            }

            Debug.Log($"Running: git {arguments} at {workingDirectory}");
            return process.Run(
                application: "git",
                arguments: arguments,
                workingDirectory: workingDirectory,
                out output,
                out errorOutput
            );
        }
    }
}