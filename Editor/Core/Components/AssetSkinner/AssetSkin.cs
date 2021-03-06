using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine.WSA;

namespace Miniclip.ShapeShifter
{
    public class AssetSkin
    {
        internal string FolderPath { get; set; }

        internal string Game { get; set; }

        internal string Guid { get; set; }

        public AssetSkin(string guid, string game)
        {
            Guid = guid;

            Game = game;

            FolderPath = Path.Combine(
                ShapeShifter.SkinsFolder.FullName,
                game,
                ShapeShifterConstants.INTERNAL_ASSETS_FOLDER,
                guid
            );
        }

        public bool IsValid()
        {
            return FileUtils.DoesFolderExistAndHaveFiles(FolderPath);
        }

        public void Rename(string newName)
        {
            if (PathUtils.IsInternalPath(newName))
            {
                newName = Path.GetFileName(newName) ?? Path.GetDirectoryName(newName);
            }

            DirectoryInfo directoryInfo = new DirectoryInfo(FolderPath);

            int totalDirectories = directoryInfo.EnumerateDirectories().Count();
            int totalFiles = directoryInfo.EnumerateFiles().Count();

            if (totalDirectories > 0)
            {
                string skinnedFolderFullPath = directoryInfo.GetDirectories().FirstOrDefault()?.FullName;

                if (!string.IsNullOrEmpty(skinnedFolderFullPath))
                {
                    string oldName = new DirectoryInfo(skinnedFolderFullPath).Name;
                    string newFullPath = skinnedFolderFullPath.Replace(oldName, newName);

                    if (!string.Equals(skinnedFolderFullPath, newFullPath))
                    {
                        Directory.Move(skinnedFolderFullPath, newFullPath);
                        File.Move(skinnedFolderFullPath + ".meta", newFullPath + ".meta");
                    }

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

                    string newFullPath = skinnedFileFullPath.Replace(oldName, newName);

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

        public void Delete()
        {
            FileUtils.SafeDelete(FolderPath);
            Stage();
        }

        public void CopyFromUnityToSkinFolder()
        {
            
            AssetSwitcherOperations.CopyFromUnityToSkins(new DirectoryInfo(FolderPath));

            return;
            
            string origin = AssetDatabase.GUIDToAssetPath(Guid);

            FileUtils.TryCreateDirectory(FolderPath, true);

            if (string.IsNullOrEmpty(origin) || !PathUtils.FileOrDirectoryExists(origin))
            {
                //nothing else to do as the skin folder was already deleted
                return;
            }
            
            string target = Path.Combine(FolderPath, Path.GetFileName(origin));

            FileUtils.SafeCopy(origin, target);
            FileUtils.SafeCopy(origin + ".meta", target + ".meta");
        }

        public void CopyFromSkinFolderToUnity()
        {
            AssetSwitcherOperations.CopyFromSkinsToUnity(new DirectoryInfo(FolderPath));
        }
    }
}