using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Miniclip.ShapeShifter.Utils
{
    public class GitUtils
    {
        private static readonly string GIT_IGNORE_BEGIN_LABEL = "# Begin ShapeShifter";
        private static readonly string GIT_IGNORE_END_LABEL = "# End ShapeShifter";

        internal static string CurrentBranch => RunGitCommand("rev-parse --abbrev-ref HEAD");
        internal static string RepositoryPath => RunGitCommand("rev-parse --git-dir");

        private static bool IsTracked(string filePath)
        {
            using (Process process = new Process())
            {
                string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, filePath);

                string arguments = $"ls-files --error-unmatch {fullPath}";
                int exitCode = RunProcessAndGetExitCode(arguments, process, out string output, out string errorOutput);

                return exitCode != 1;
            }
        }
        
        internal static void Track(string assetPath)
        {
            if (IsTracked(assetPath))
            {
                return;
            }

            if (IsIgnored(assetPath))
            {
                RemoveFromGitIgnore(assetPath);
            }
        }

        internal static void Untrack(string assetPath, bool addToGitIgnore = false)
        {
            string fullFilePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, assetPath);

            if (IsTracked(fullFilePath))
            {
                RunGitCommand($"rm -r --cached {fullFilePath}");
            }

            if (addToGitIgnore)
            {
                AddToGitIgnore(assetPath);
            }
        }

        private static void AddToGitIgnore(string pathToIgnore)
        {
            if (!TryGetGitIgnoreContent(out List<string> gitIgnoreContent))
            {
                return;
            }

            int start = gitIgnoreContent.IndexOf(GIT_IGNORE_BEGIN_LABEL);
            int end = gitIgnoreContent.IndexOf(GIT_IGNORE_END_LABEL);
            if (start == -1)
            {
                gitIgnoreContent.Add(GIT_IGNORE_BEGIN_LABEL);
                start = gitIgnoreContent.IndexOf(GIT_IGNORE_BEGIN_LABEL);
            }

            if (end == -1)
            {
                gitIgnoreContent.Add(GIT_IGNORE_END_LABEL);
                end = gitIgnoreContent.IndexOf(GIT_IGNORE_END_LABEL);
            }

            string folderName = Directory.GetParent(Application.dataPath).Name;
            string fileRelativePath = Path.Combine(folderName, pathToIgnore);

            if (gitIgnoreContent.Contains(fileRelativePath))
            {
                //path already in git ignore
                return;
            }

            gitIgnoreContent.Insert(end, fileRelativePath);

            SetGitIgnoreContent(gitIgnoreContent);
        }

        private static void RemoveFromGitIgnore(string pathToAcknowledge)
        {
            if (!TryGetGitIgnoreContent(out List<string> gitIgnoreContent))
            {
                return;
            }

            string folderName = Directory.GetParent(Application.dataPath).Name;
            string fileRelativePath = Path.Combine(folderName, pathToAcknowledge);
            
            gitIgnoreContent.Remove(fileRelativePath);

            SetGitIgnoreContent(gitIgnoreContent);
        }

        private static bool TryGetGitIgnoreContent(out List<string> gitIgnoreContent)
        {
            string gitIgnorePath = GetGitIgnorePath();

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
            string gitIgnorePath = GetGitIgnorePath();

            if (!File.Exists(gitIgnorePath))
            {
                ShapeShifterLogger.LogError($"Could not find .gitignore file at {gitIgnorePath}");
                return;
            }

            File.WriteAllLines(gitIgnorePath, newGitIgnoreContent);
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
            
            return gitIgnoreContent.Contains(fileRelativePath);
        }

        private static string RunGitCommand(string arguments)
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
                    ShapeShifterLogger.LogError(errorOutput);
                    throw new InvalidOperationException($"Failed to run git command: Exit Code: {exitCode.ToString()}");
                }
            }
        }

        private static int RunProcessAndGetExitCode(string arguments, Process process, out string output,
            out string errorOutput) =>
            process.Run(
                application: "git",
                arguments: arguments,
                workingDirectory: Application.dataPath,
                out output,
                out errorOutput
            );
    }
}