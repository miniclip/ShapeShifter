using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;

namespace Miniclip.ShapeShifter
{
    public static class PreMergeCheck
    {
        public static int HasShapeShifterConflictsBetweenBranches(string mainBranch,
            string branchToMergeIntoMainBranch,
            out List<string> possibleConflictingFiles)
        {
            List<string> skinnedPaths = GetCurrentSkinnedPaths();

            possibleConflictingFiles = GetPossibleConflictingFiles(mainBranch, branchToMergeIntoMainBranch, skinnedPaths);

            return possibleConflictingFiles.Count > 0 ? 1 : 0;
        }

        private static List<string> GetCurrentSkinnedPaths()
        {
            GitIgnore.GitIgnoreWrapper gitIgnoreWrapper = GitIgnore.GitIgnoreWrapper.Instance();
            List<string> currentSkinnedPaths = new List<string>();
            foreach (KeyValuePair<string, List<string>> keyValuePair in
                     gitIgnoreWrapper)
            {
                currentSkinnedPaths.AddRange(keyValuePair.Value);
            }

            return currentSkinnedPaths;
        }

        private static List<string> GetPossibleConflictingFiles(string currentBranch, string targetBranch,
            List<string> localSkinnedPaths)
        {
            string[] changedFileInTargetBranchSinceCommonAncestor = GitUtils
                .RunGitCommand("git diff " + currentBranch + "..." + targetBranch + " --name-only")
                .Split('\n');

            return changedFileInTargetBranchSinceCommonAncestor
                .Where(diffFile => (!string.IsNullOrEmpty(diffFile) && localSkinnedPaths.Contains(diffFile)))
                .ToList();
        }
    }
}