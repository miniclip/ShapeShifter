using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [Serializable]
    class GameSkin
    {
        internal string ExternalSkinsFolder { get; private set; }

        internal string InternalSkinsFolder { get; private set; }

        internal string MainFolder { get; private set; }

        [SerializeField]
        internal string Name { get; private set; }

        internal GameSkin(string name)
        {
            SetUpGameSkin(name);
        }

        public bool HasGuid(string guid) => GetAssetSkins().Any(assetSkin => assetSkin.Guid == guid);

        public AssetSkin GetAssetSkin(string guid)
        {
            List<AssetSkin> existing = GetAssetSkins();

            AssetSkin assetSkin = existing.FirstOrDefault(s => s.Guid == guid);

            return assetSkin;
        }

        public void Duplicate(string newGame)
        {
            GameSkin newGameSkin = new GameSkin(newGame);
            FileUtil.CopyFileOrDirectory(MainFolder, newGameSkin.MainFolder);
        }

        public void RenameFolder(string newName)
        {
            var newMainFolderPath = GetGameFolderPath(newName);
            FileUtil.MoveFileOrDirectory(MainFolder, newMainFolderPath);

            SetUpGameSkin(newName);
        }

        private void SetUpGameSkin(string name)
        {
            Name = name;

            SetUpFolderPaths(name);
        }

        private void SetUpFolderPaths(string name)
        {
            MainFolder = GetGameFolderPath(name);
            InternalSkinsFolder = Path.Combine(MainFolder, ShapeShifterConstants.INTERNAL_ASSETS_FOLDER);
            ExternalSkinsFolder = Path.Combine(MainFolder, ShapeShifterConstants.EXTERNAL_ASSETS_FOLDER);
        }

        private string GetGameFolderPath(string name) => Path.Combine(ShapeShifter.SkinsFolder.FullName, name);

        internal List<AssetSkin> GetAssetSkins()
        {
            List<AssetSkin> assetSkins = new List<AssetSkin>();
            if (Directory.Exists(InternalSkinsFolder))
            {
                DirectoryInfo internalFolder = new DirectoryInfo(InternalSkinsFolder);
                foreach (DirectoryInfo directory in internalFolder.GetDirectories())
                {
                    assetSkins.Add(new AssetSkin(directory.Name, directory.FullName));
                }
            }

            return assetSkins;
        }

        internal bool HasValidFolders() => HasInternalSkins() || HasExternalSkins();

        internal bool HasExternalSkins() => DoesFolderExistAndHaveFiles(ExternalSkinsFolder);

        internal bool HasInternalSkins() => DoesFolderExistAndHaveFiles(InternalSkinsFolder);

        private bool DoesFolderExistAndHaveFiles(string path) =>
            Directory.Exists(path) && Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any();

        internal void DeleteFolder()
        {
            if (Directory.Exists(MainFolder))
            {
                Directory.Delete(MainFolder, true);
            }
        }
    }
}