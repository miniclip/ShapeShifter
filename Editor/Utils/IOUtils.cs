using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Miniclip.ShapeShifter.Utils
{
    public class IOUtils
    {
        public static void TryCreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        public static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (FileInfo file in target.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo directory in target.GetDirectories())
            {
                directory.Delete(true);
            }

            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name), true);
            }

            foreach (DirectoryInfo nextSource in source.GetDirectories())
            {
                DirectoryInfo nextTarget = target.CreateSubdirectory(nextSource.Name);
                CopyFolder(nextSource, nextTarget);
            }
        }

        public static void CopyFile(string source, string destination, bool overwrite = true)
        {
            File.Copy(source, destination, overwrite);
        }
        
        public static bool IsFolderEmpty(string path) => Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any();
        public static bool IsFolderEmpty(DirectoryInfo directoryInfo) => IsFolderEmpty(directoryInfo.FullName);
    }
}