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
        private static string GetAssetIgnoreIdentifier(string assetPath) =>
            $"{GIT_IGNORE_SHAPESHIFTER_LABEL} {AssetDatabase.AssetPathToGUID(assetPath)}";
        
        internal static bool CanStage(string assetPath)
        {
            ChangedFileGitInfo[] unstagedFiles = GetUnstagedFiles();

            return unstagedFiles.Any(file => file.path == PathUtils.GetPathRelativeToRepositoryFolder(assetPath));
        }

        internal static bool CanUnstage(string assetPath)
        {
            ChangedFileGitInfo[] stagedFiles = GetStagedFiles();

            return stagedFiles.Any(file => file.path == PathUtils.GetPathRelativeToRepositoryFolder(assetPath));
        }
        
        internal static void Stage(string assetPath)
        {
            if (CanStage(assetPath))
                RunGitCommand($"add \"{PathUtils.GetFullPath(assetPath)}\"");
        }

        internal static void UnStage(string assetPath)
        {
            if (CanUnstage(assetPath))
                RunGitCommand($"restore --staged \"{PathUtils.GetFullPath(assetPath)}\"");
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
            Stage(assetPath);
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
            else if (CanUnstage(fullFilePath))
            {
                UnStage(fullFilePath);
            }

            if (addToGitIgnore)
            {
                AddToGitIgnore(assetPath);
            }
        }

       

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

        public static void DiscardChanges(string assetPath)
        {
            RunGitCommand($"checkout \"{PathUtils.GetFullPath(assetPath)}\"");
        }
        
        internal static ChangedFileGitInfo[] GetAllChangedFilesGitInfo()
        {
            string status = RunGitCommand("status --porcelain -u");
            string[] splitted = status.Split('\n');

            return splitted.Select(line => new ChangedFileGitInfo(line)).ToArray();
        }

        internal static ChangedFileGitInfo[] GetUnstagedFiles(ChangedFileGitInfo[] changedFiles = null)
        {
            if (changedFiles == null)
            {
                return GetAllChangedFilesGitInfo().Where(file => file.hasUnstagedChanges).ToArray();
            }

            return changedFiles.Where(file => file.hasUnstagedChanges).ToArray();
        }

        internal static ChangedFileGitInfo[] GetStagedFiles(ChangedFileGitInfo[] changedFiles = null)
        {
            if (changedFiles == null)
            {
                return GetAllChangedFilesGitInfo().Where(file => file.hasStagedChanges).ToArray();
            }

            return changedFiles.Where(file => file.hasStagedChanges).ToArray();
        }

        internal static List<ChangedFileGitInfo> GetDeletedFiles(ChangedFileGitInfo[] changedFiles = null)
        {
            if (changedFiles == null)
            {
                return GetAllChangedFilesGitInfo().Where(file => file.status.Contains(git_status_deleted)).ToList();
            }

            return changedFiles.Where(file => file.status.Contains(git_status_deleted)).ToList();
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
        
        internal struct ChangedFileGitInfo
        {
            public string status;
            public bool hasStagedChanges;
            public bool hasUnstagedChanges;
            public bool isTracked;
            public bool isFullyStaged => hasStagedChanges && !hasUnstagedChanges;
            public string path;

            public ChangedFileGitInfo(string line)
            {
                status = line.Substring(0, 2);
                path = line.Substring(3);
                
                if (status == git_status_untracked)
                {
                    isTracked = false;
                    hasUnstagedChanges = true;
                    hasStagedChanges = false;
                    
                    //git status surrounds untracked files path with "", need to remove them
                    path = path.Trim('\"');
                    return;
                }

                isTracked = true;
                hasStagedChanges = !status.StartsWith(" ");
                hasUnstagedChanges = !status.EndsWith(" ");
            }
        }
    }
    
}