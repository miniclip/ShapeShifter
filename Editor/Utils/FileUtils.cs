using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    class FileUtils
    {
        private const int BYTES_TO_READ = sizeof(long);

        public static bool DoesFolderExistAndHaveFiles(string path)
        {
            bool directoryExists = Directory.Exists(path);

            if (!directoryExists)
            {
                return false;
            }

            IEnumerable<string> enumerateFiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

            enumerateFiles = enumerateFiles.Where(file => !file.Contains(".DS_Store"));

            bool directoryContainsFiles = enumerateFiles.Any();

            return directoryContainsFiles;
        }

        public static void TryCreateDirectory(string directoryPath, bool deleteIfExists = false)
        {
            ValidatePathSafety(directoryPath);

            if (Directory.Exists(directoryPath))
            {
                if (!deleteIfExists)
                {
                    return;
                }

                SafeDelete(directoryPath);
            }

            Directory.CreateDirectory(directoryPath);
        }

        public static void SafeDelete(string path)
        {
            ValidatePathSafety(path);
            FileUtil.DeleteFileOrDirectory(path);
        }

        public static void SafeCopy(string source, string destination)
        {
            ValidatePathSafety(source, destination);
            SafeDelete(destination);
            FileUtil.CopyFileOrDirectory(source, destination);
        }

        public static void ValidatePathSafety(params string[] paths)
        {
            DirectoryInfo assetsDirectoryInfo = new DirectoryInfo(Application.dataPath);
            DirectoryInfo projectDirectoryInfo = assetsDirectoryInfo.Parent;
            string repositoryPath = GitUtils.MainRepositoryPath;

            foreach (string path in paths)
            {
                string fullPath = PathUtils.GetFullPath(path);

                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException("Path given is empty or null or whitespace");
                }

                string normalizedPath = PathUtils.NormalizePath(fullPath);

                if (normalizedPath == PathUtils.NormalizePath(assetsDirectoryInfo.FullName))
                {
                    throw new ArgumentException($"Path given ({normalizedPath}) is Application.DataPath");
                }

                if (normalizedPath == PathUtils.NormalizePath(projectDirectoryInfo?.FullName))
                {
                    throw new ArgumentException($"Path given ({normalizedPath}) is the project root folder");
                }

                if (normalizedPath == PathUtils.NormalizePath(repositoryPath))
                {
                    throw new ArgumentException($"Path given ({normalizedPath}) is the repository root folder");
                }

                if (!normalizedPath.Contains(PathUtils.NormalizePath(repositoryPath)))
                {
                    throw new ArgumentException($"Path given ({normalizedPath}) is outside the repository folder");
                }
            }
        }

        public static bool FilesAreEqual(string first, string second)
        {
            FileInfo firstFileInfo = new FileInfo(first);
            FileInfo secondFileInfo = new FileInfo(second);

            if (firstFileInfo.Length != secondFileInfo.Length)
            {
                return false;
            }

            int iterations = (int) Math.Ceiling((double) first.Length / BYTES_TO_READ);

            using FileStream fs1 = firstFileInfo.OpenRead();
            using FileStream fs2 = secondFileInfo.OpenRead();

            byte[] one = new byte[BYTES_TO_READ];
            byte[] two = new byte[BYTES_TO_READ];

            for (int i = 0; i < iterations; i++)
            {
                fs1.Read(one, 0, BYTES_TO_READ);
                fs2.Read(two, 0, BYTES_TO_READ);

                if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                {
                    return false;
                }
            }

            return true;
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