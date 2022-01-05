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

        internal static bool IsFolderEmpty(string path) =>
            Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any();

        internal static bool IsFolderEmpty(DirectoryInfo directoryInfo) => IsFolderEmpty(directoryInfo.FullName);
    }
}