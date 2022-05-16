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

        private static string GitIgnorePath => Path.Combine(GitUtils.MainRepositoryPath, ".gitignore");

        private static string GenerateAssetIgnoreIdentifierFromGuid(string guid) =>
            $"{GIT_IGNORE_SHAPESHIFTER_LABEL} {guid}";

        internal static bool IsIgnored(string guid) => GitIgnoreWrapper.Instance().ContainsKey(guid);

        internal static string GetIgnoredPathByGuid(string guid)
        {
            GitIgnoreWrapper gitIgnoreWrapper = GitIgnoreWrapper.Instance();
            if (gitIgnoreWrapper.TryGetValue(guid, out List<string> ignoredPaths))
            {
                return ignoredPaths.FirstOrDefault()?.TrimStart('/');
            }
            else
            {
                return null;
            }
            
        }

        public static void Add(string key, string pathToIgnore)
        {
            if (string.IsNullOrEmpty(pathToIgnore))
            {
                throw new ArgumentNullException("Trying to add an empty path to git ignore");
            }

            GitIgnoreWrapper gitIgnore = GitIgnoreWrapper.Instance();

            List<string> ignoredPaths = gitIgnore.ContainsKey(key) ? gitIgnore[key] : new List<string>();
            ignoredPaths.Clear();

            string fullPathToIgnore = PathUtils.GetFullPath(pathToIgnore);

            string sanitizedPathToAdd = "/" + PathUtils.GetPathRelativeToRepositoryFolder(fullPathToIgnore);

            ignoredPaths.Add(sanitizedPathToAdd);

            if (File.Exists(fullPathToIgnore.TrimEnd(Path.DirectorySeparatorChar) + ".meta"))
            {
                string metaPathRelativeToProjectFolder = sanitizedPathToAdd.TrimEnd(Path.DirectorySeparatorChar) + ".meta";
                ignoredPaths.Add(metaPathRelativeToProjectFolder);
            }

            gitIgnore[key] = ignoredPaths;
            gitIgnore.WriteToFile();
        }

        internal static void Remove(string key)
        {
            GitIgnoreWrapper gitIgnore = GitIgnoreWrapper.Instance();

            if (gitIgnore.ContainsKey(key))
            {
                gitIgnore.Remove(key);
            }

            gitIgnore.WriteToFile();
        }

        internal static void ClearShapeShifterEntries()
        {
            GitIgnoreWrapper gitIgnore = GitIgnoreWrapper.Instance();

            var listCopy = new List<string>(gitIgnore.Keys.ToList());

            foreach (string key in listCopy.Where(key => key != NON_SHAPE_SHIFTER_LINES_KEY))
            {
                gitIgnore.Remove(key);
            }

            gitIgnore.WriteToFile();
        }

        public class GitIgnoreWrapper : Dictionary<string, List<string>>
        {
            public static GitIgnoreWrapper Instance() => new GitIgnoreWrapper();

            private GitIgnoreWrapper()
            {
                // EditorUtility.DisplayProgressBar("Git Ignore", "Fetching current git ignore contents", 0f);
                List<string> ignoredContent = FileUtils.ReadAllLines(GitIgnorePath);

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
                // EditorUtility.ClearProgressBar();
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