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
        public static bool DoesFolderExistAndHaveFiles(string path)
        {
            bool directoryExists = Directory.Exists(path);

            if (!directoryExists)
            {
                return false;
            }

            IEnumerable<string> enumerateFiles = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);

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

                FileUtils.SafeDelete(directoryPath);
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
            DirectoryInfo assets = new DirectoryInfo(Application.dataPath);
            DirectoryInfo project = assets.Parent;
            string repositoryPath = GitUtils.MainRepositoryPath;

            foreach (string path in paths)
            {
                string fullPath = PathUtils.GetFullPath(path);

                if (string.IsNullOrEmpty(path) || string.IsNullOrWhiteSpace(path))
                {
                    throw new ArgumentException("Path given is empty or null or whitespace");
                }

                if (PathUtils.NormalizePath(fullPath) == PathUtils.NormalizePath(assets.FullName))
                {
                    throw new ArgumentException("Path given is Application.DataPath");
                }

                if (PathUtils.NormalizePath(fullPath) == PathUtils.NormalizePath(project?.FullName))
                {
                    throw new ArgumentException("Path given is the project root folder");
                }

                if (PathUtils.NormalizePath(fullPath) == PathUtils.NormalizePath(repositoryPath))
                {
                    throw new ArgumentException("Path given is the repository root folder");
                }

                if (!PathUtils.NormalizePath(fullPath).Contains(PathUtils.NormalizePath(repositoryPath)))
                {
                    throw new ArgumentException("Path given is outside the repository folder");
                }
            }
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