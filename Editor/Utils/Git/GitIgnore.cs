using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Miniclip.ShapeShifter.Utils.Git
{
    static class GitIgnore
    {
        private const string GIT_IGNORE_SHAPESHIFTER_LABEL = "#ShapeShifter";
        private const string NON_SHAPE_SHIFTER_LINES_KEY = "RestOfGitIgnore";

        private static string GitIgnorePath => Path.Combine(GitUtils.RepositoryPath, ".gitignore");

        private static string GenerateAssetIgnoreIdentifierFromGUID(string guid) =>
            $"{GIT_IGNORE_SHAPESHIFTER_LABEL} {guid}";

        [MenuItem("Window/Shape Shifter/Parse git ignore")]
        private static GitIgnoreWrapper GetGitIgnore()
        {
            List<string> ignoredContent = ReadGitIgnore();

            string currentKey = string.Empty;
            GitIgnoreWrapper guidToPathsIgnoredDict = new GitIgnoreWrapper();

            int i;
            guidToPathsIgnoredDict.Add(NON_SHAPE_SHIFTER_LINES_KEY, new List<string>());
            for (i = 0; i < ignoredContent.Count; i++)
            {
                string line = ignoredContent[i];
                
                if (line.Contains(GIT_IGNORE_SHAPESHIFTER_LABEL))
                {
                    break;
                }

                guidToPathsIgnoredDict[NON_SHAPE_SHIFTER_LINES_KEY].Add(line);
            }

            for (; i < ignoredContent.Count; i++)
            {
                string line = ignoredContent[i];

                if (line.Contains(GIT_IGNORE_SHAPESHIFTER_LABEL))
                {
                    string guid = line.Split(' ')[1]; //#ShapeShifter 8d5cf74b6c07945baa7484f8777682ea
                    currentKey = guid;
                    guidToPathsIgnoredDict.Add(guid, new List<string>());
                    continue;
                }

                if (string.IsNullOrEmpty(currentKey))
                {
                    continue;
                }

                guidToPathsIgnoredDict[currentKey].Add(line);
            }

            return guidToPathsIgnoredDict;
        }

        internal static void Add(string guid)
        {
            GitIgnoreWrapper gitIgnore = GetGitIgnore();

            List<string> ignoredPaths;

            if (gitIgnore.ContainsKey(guid))
            {
                ignoredPaths = gitIgnore[guid];
            }
            else
            {
                ignoredPaths = new List<string>();
            }

            ignoredPaths.Clear();

            string assetPath = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(assetPath))
            {
                throw new Exception($"GUID {guid} not found in AssetDatabase");
            }

            string pathRelativeToProjectFolder = PathUtils.GetPathRelativeToProjectFolder(assetPath);
            string metaPathRelativeToProjectFolder = pathRelativeToProjectFolder + ".meta";

            ignoredPaths.Add(pathRelativeToProjectFolder);
            ignoredPaths.Add(metaPathRelativeToProjectFolder);
            gitIgnore[guid] = ignoredPaths;
            WriteGitIgnore(gitIgnore);
        }

        internal static void Remove(string guid)
        {
            GitIgnoreWrapper gitIgnore = GetGitIgnore();

            if (gitIgnore.ContainsKey(guid))
            {
                gitIgnore.Remove(guid);
            }

            WriteGitIgnore(gitIgnore);
        }

        private static List<string> ReadGitIgnore()
        {
            if (!File.Exists(GitIgnorePath))
            {
                throw new FileNotFoundException($"Could not find .gitignore file at {GitIgnorePath}");
            }

            List<string> gitIgnoreContent = File.ReadAllLines(GitIgnorePath).ToList();

            if (gitIgnoreContent.Count == 0)
            {
                throw new Exception("Could not read git ignore file");
            }

            return gitIgnoreContent;
        }

        [MenuItem("Window/Shape Shifter/Remove all git ignore entries")]
        public static void ClearShapeShifterEntries()
        {
            GitIgnoreWrapper gitIgnore = GetGitIgnore();

            foreach (string key in gitIgnore.Keys.Where(key => key != NON_SHAPE_SHIFTER_LINES_KEY))
            {
                gitIgnore.Remove(key);
            }

            WriteGitIgnore(gitIgnore);
        }

        private static void WriteGitIgnore(GitIgnoreWrapper gitIgnore)
        {
            IEnumerable<string> linesToWrite = ConvertDictionaryIntoStringList(gitIgnore);
            File.WriteAllLines(GitIgnorePath, linesToWrite);
            GitUtils.Stage(GitIgnorePath);
        }

        private static IEnumerable<string> ConvertDictionaryIntoStringList(GitIgnoreWrapper gitIgnore)
        {
            List<string> list = new List<string>();

            list.AddRange(gitIgnore[NON_SHAPE_SHIFTER_LINES_KEY]);

            gitIgnore.Remove(NON_SHAPE_SHIFTER_LINES_KEY);

            foreach (KeyValuePair<string, List<string>> keyValuePair in gitIgnore)
            {
                list.Add(GenerateAssetIgnoreIdentifierFromGUID(keyValuePair.Key));

                foreach (string path in keyValuePair.Value)
                {
                    list.Add(path);
                }
            }

            return list;
        }

        public static bool IsIgnored(string guid) => GetGitIgnore().ContainsKey(guid);

        public static string GetIgnoredPathByGUID(string guid) =>
            GetGitIgnore().TryGetValue(guid, out List<string> ignoredPaths) ? ignoredPaths.FirstOrDefault() : null;

        private class GitIgnoreWrapper : Dictionary<string, List<string>> { }
    }
}