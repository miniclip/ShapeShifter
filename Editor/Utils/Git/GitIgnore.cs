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

        private static string GenerateAssetIgnoreIdentifierFromGuid(string guid) =>
            $"{GIT_IGNORE_SHAPESHIFTER_LABEL} {guid}";

        internal static bool IsIgnored(string guid) => GitIgnoreWrapper.Instance().ContainsKey(guid);

        internal static string GetIgnoredPathByGuid(string guid) =>
            GitIgnoreWrapper.Instance().TryGetValue(guid, out List<string> ignoredPaths)
                ? ignoredPaths.FirstOrDefault()
                : null;

        internal static void Add(string guid)
        {
            GitIgnoreWrapper gitIgnore = GitIgnoreWrapper.Instance();

            List<string> ignoredPaths = gitIgnore.ContainsKey(guid) ? gitIgnore[guid] : new List<string>();

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

            gitIgnore.WriteToFile();
        }

        internal static void Remove(string guid)
        {
            GitIgnoreWrapper gitIgnore = GitIgnoreWrapper.Instance();

            if (gitIgnore.ContainsKey(guid))
            {
                gitIgnore.Remove(guid);
            }

            gitIgnore.WriteToFile();
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
        internal static void ClearShapeShifterEntries()
        {
            GitIgnoreWrapper gitIgnore = GitIgnoreWrapper.Instance();

            foreach (string key in gitIgnore.Keys.Where(key => key != NON_SHAPE_SHIFTER_LINES_KEY))
            {
                gitIgnore.Remove(key);
            }

            gitIgnore.WriteToFile();
        }

        private class GitIgnoreWrapper : Dictionary<string, List<string>>
        {
            public static GitIgnoreWrapper Instance() => new GitIgnoreWrapper();

            private GitIgnoreWrapper()
            {
                List<string> ignoredContent = ReadGitIgnore();

                string currentKey = string.Empty;
                int i;
                Add(NON_SHAPE_SHIFTER_LINES_KEY, new List<string>());
                for (i = 0; i < ignoredContent.Count; i++)
                {
                    string line = ignoredContent[i];

                    if (line.Contains(GIT_IGNORE_SHAPESHIFTER_LABEL))
                    {
                        break;
                    }

                    this[NON_SHAPE_SHIFTER_LINES_KEY].Add(line);
                }

                for (; i < ignoredContent.Count; i++)
                {
                    string line = ignoredContent[i];

                    if (line.Contains(GIT_IGNORE_SHAPESHIFTER_LABEL))
                    {
                        string guid = line.Split(' ')[1]; //#ShapeShifter 8d5cf74b6c07945baa7484f8777682ea
                        currentKey = guid;
                        Add(guid, new List<string>());
                        continue;
                    }

                    if (string.IsNullOrEmpty(currentKey))
                    {
                        continue;
                    }

                    this[currentKey].Add(line);
                }
            }

            internal void WriteToFile()
            {
                File.WriteAllLines(GitIgnorePath, ToListString());
                GitUtils.Stage(GitIgnorePath);
            }

            private IEnumerable<string> ToListString()
            {
                List<string> list = new List<string>();

                list.AddRange(this[NON_SHAPE_SHIFTER_LINES_KEY]);

                Remove(NON_SHAPE_SHIFTER_LINES_KEY);

                foreach (KeyValuePair<string, List<string>> keyValuePair in this)
                {
                    list.Add(GenerateAssetIgnoreIdentifierFromGuid(keyValuePair.Key));

                    foreach (string path in keyValuePair.Value)
                    {
                        list.Add(path);
                    }
                }

                return list;
            }
        }
    }
}