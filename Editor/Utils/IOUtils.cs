using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Miniclip.ShapeShifter.Utils
{
    class IOUtils
    {
        internal static void TryCreateDirectory(string directoryPath, bool deleteIfExists = false)
        {
            if (Directory.Exists(directoryPath))
            {
                if (!deleteIfExists)
                {
                    return;
                }

                Directory.Delete(directoryPath, true);
            }

            Directory.CreateDirectory(directoryPath);
        }

        internal static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            CopyFolder(source.FullName, target.FullName);
        }

        internal static void CopyFolder(string source, string target)
        {
            FileUtil.ReplaceDirectory(source, target);
        }

        internal static void CopyFile(string source, string destination)
        {
            FileUtil.ReplaceFile(source, destination);
        }

        public static bool DoesFolderExistAndHaveFiles(string path)
        {
            bool directoryExists = Directory.Exists(path);

            if (!directoryExists)
                return false;

            IEnumerable<string> enumerateFiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

            bool directoryContainsFiles = enumerateFiles.Any();

            return directoryContainsFiles;
        }

        internal static List<string> ReadAllLines(string filepath)
        {
            if (!File.Exists(filepath))
            {
                throw new FileNotFoundException($"Could not find file at {filepath}");
            }

            List<string> lines = File.ReadAllLines(filepath).ToList();

            if (lines.Count == 0)
            {
                throw new Exception("Could not read file");
            }

            return lines;
        }
    }
}