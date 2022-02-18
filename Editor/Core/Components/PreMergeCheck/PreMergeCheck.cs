using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;

namespace Miniclip.ShapeShifter
{
    public static class PreMergeCheck
    {
        public static int HasShapeShifterConflictsWith(string currentBranch,
            string targetBranch,
            out List<string> filesChangedBetweenBranches)
        {
            List<string> skinnedPaths = GetCurrentSkinnedPaths();
            
            string[] allChangedFilesInTargetBranch = GitUtils
                .RunGitCommand("git diff " + currentBranch + "..." + targetBranch + " --name-only")
                .Split('\n');
            
            filesChangedBetweenBranches = allChangedFilesInTargetBranch
                .Where(diffFile => skinnedPaths.Contains(diffFile))
                .ToList();
            
            return 0;
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
    }
}