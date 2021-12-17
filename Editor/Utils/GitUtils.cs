using System;
using System.Diagnostics;
using Miniclip.ShapeShifter.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Miniclip.ShapeShifter.Utils
{
    public class GitUtils
    {
        public static string CurrentBranch => RunGitCommand("rev-parse --abbrev-ref HEAD");

        public static string RepositoryPath = RunGitCommand("rev-parse --git-dir");

        private static string RunGitCommand(string arguments)
        {
            using (Process process = new Process())
            {
                int exitCode = process.Run(
                    application: "git",
                    arguments: arguments,
                    workingDirectory: Application.dataPath,
                    output: out string output,
                    errors: out string errorOutput
                );

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
    }
}