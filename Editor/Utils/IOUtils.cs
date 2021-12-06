using System;
using System.IO;

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
            GC.Collect();
            
            GC.WaitForPendingFinalizers();
            // using (var sourceFile = File.OpenRead(source))
            // using (var targetFile = File.Create(destination))
            // {
            //     byte[] buffer = new byte[sourceFile.Length];
            //     int bytesRead = sourceFile.Read(buffer, 0, buffer.Length);
            //     if (bytesRead == 0)
            //     {
            //         Debug.LogWarning($"Did not copy {source}");
            //         return;
            //     }
            //
            //     targetFile.Write(buffer, 0, bytesRead);
            //     Debug.Log($"Copied {source}");
            // }
        }

    }
}