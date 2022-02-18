using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public static class PreMergeCheck
    {
        public static int HasShapeShifterConflictsWith(string targetBranch)
        {
            List<string> skinnedAssetsGUIDs = GitIgnore.GitIgnoreWrapper.Instance().Keys.ToList();

            ShapeShifterLogger.Log(skinnedAssetsGUIDs);

            string commitWhereCurrentBranchForkedFromTargetBranch =
                GitUtils.RunGitCommand($"merge-base --fork-point {targetBranch}");

            ShapeShifterLogger.Log("commitWhereCurrentBranchForkedFromTargetBranch:" + commitWhereCurrentBranchForkedFromTargetBranch);

            string latestCommitFromTargetBranch = GitUtils.RunGitCommand($"log --format=\"%H\" -n 1 {targetBranch}");

            ShapeShifterLogger.Log("latestCommitFromTargetBranch:" + latestCommitFromTargetBranch);
            
            //get list of skinned assets that were not skinned at fork point
            
            // check if they remain unchanged between fork point and latest commit from target branch
            

            return 0;
        }
    }
}