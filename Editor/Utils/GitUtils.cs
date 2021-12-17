using System;
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

        private static string ShapeShifterGitIgnoreLabel = "# ShapeShifter";
        
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

        internal static void UntrackFile(string filePath, bool addFileToGitIgnore = false)
        {
            string fullPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, filePath);

            RunGitCommand($"rm --cached {fullPath}");

            if (addFileToGitIgnore)
            {
                AddToGitIgnore(fullPath);
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
            repositoryPath =  repositoryPath.Remove(repositoryPath.IndexOf(".git", StringComparison.Ordinal));

            var gitIgnorePath = Path.Combine(repositoryPath, ".gitignore");

            if (!File.Exists(gitIgnorePath))
            {
                Debug.LogError($"Could not find .gitignore file at {repositoryPath}");
                return;
            }

            var gitIgnoreContent = File.ReadAllLines(gitIgnorePath).ToList();

            if (!gitIgnoreContent.Contains(ShapeShifterGitIgnoreLabel))
            {
                gitIgnoreContent.Add(ShapeShifterGitIgnoreLabel);
            }
            
            
            
        }
    }
}