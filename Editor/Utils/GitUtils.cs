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

        internal static bool IsFileTracked(string filePath)
        {
            using (Process process = new Process())
            {
                string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, filePath);

                string arguments = $"ls-files --error-unmatch {fullPath}";
                int exitCode = RunProcessAndGetExitCode(arguments, process, out string output, out string errorOutput);

                return exitCode != 1;
            }
        }

        internal static void Untrack(string path, bool addToGitIgnore = false)
        {
            //TODO Needs to work with folders 
            string fullFilePath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, path);

            if (IsFileTracked(fullFilePath))
            {
                RunGitCommand($"rm --cached {fullFilePath}");
            }

            if (addToGitIgnore)
            {
                AddToGitIgnore(path);
            }
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
                    Debug.LogError(errorOutput);
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

        private static void AddToGitIgnore(string fileToIgnore)
        {
            var repositoryPath = RepositoryPath;
            repositoryPath = repositoryPath.Remove(repositoryPath.IndexOf(".git", StringComparison.Ordinal));

            var gitIgnorePath = Path.Combine(repositoryPath, ".gitignore");

            if (!File.Exists(gitIgnorePath))
            {
                Debug.LogError($"Could not find .gitignore file at {repositoryPath}");
                return;
            }

            List<string> gitIgnoreContent = File.ReadAllLines(gitIgnorePath).ToList();
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
            string fileRelativePath = Path.Combine(folderName, fileToIgnore);
            
            gitIgnoreContent.Insert(end, fileRelativePath);
            File.WriteAllLines(gitIgnorePath, gitIgnoreContent);
        }
    }
}