using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;

namespace Miniclip.ShapeShifter
{
    class AssetSkin
    {
        public AssetSkin(string guid, string folderPath)
        {
            Guid = guid;
            FolderPath = folderPath;
        }

        internal string Guid { get; set; }

        internal string FolderPath { get; set; }

        public bool IsValid() => !IOUtils.IsFolderEmpty(FolderPath);

        public void Rename(string newName)
        {
            if (PathUtils.IsInternalPath(newName))
            {
                newName = Path.GetFileName(newName) ?? Path.GetDirectoryName(newName);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(FolderPath);

            int totalDirectories = directoryInfo.EnumerateDirectories().Count();
            int totalFiles = directoryInfo.EnumerateFiles().Count();

            string newFullPath;

            if (totalDirectories > 0)
            {
                string skinnedFolderFullPath = directoryInfo.GetDirectories().FirstOrDefault()?.FullName;

                if (!string.IsNullOrEmpty(skinnedFolderFullPath))
                {
                    string oldName = new DirectoryInfo(skinnedFolderFullPath).Name;

                    newFullPath = skinnedFolderFullPath.Replace(oldName, newName);
                    Directory.Move(skinnedFolderFullPath, newFullPath);
                    File.Move(skinnedFolderFullPath + ".meta", newFullPath + ".meta");
                    return;
                }

                ShapeShifterLogger.LogError("Skinned Folder Full Path not found");
            }

            if (totalFiles > 0)
            {
                string fileFullPath = directoryInfo.GetFiles().FirstOrDefault()?.FullName;
                if (fileFullPath != null)
                {
                    string skinnedFileFullPath = fileFullPath;
                    string oldName = Path.GetFileName(skinnedFileFullPath);

                    newFullPath = skinnedFileFullPath.Replace(oldName, newName);

                    File.Move(skinnedFileFullPath, newFullPath);
                    File.Move(skinnedFileFullPath + ".meta", newFullPath + ".meta");

                    return;
                }

                ShapeShifterLogger.LogError("Skinned File Full Path not found");
            }

            ShapeShifterLogger.LogError($"Empty skin contents on {FolderPath}");
        }

        public void Stage()
        {
            GitUtils.Stage(FolderPath);
        }
    }
}