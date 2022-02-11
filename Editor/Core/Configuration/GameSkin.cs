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
        [SerializeField]
        internal string Name { get; }
        
        internal string ExternalSkinsFolder { get; }

        internal string InternalSkinsFolder { get; }

        internal string MainFolder { get; }
        
        internal GameSkin(string name)
        {
            Name = name;

            MainFolder = GetGameFolderPath(name);
            InternalSkinsFolder = Path.Combine(MainFolder, ShapeShifterConstants.INTERNAL_ASSETS_FOLDER);
            ExternalSkinsFolder = Path.Combine(MainFolder, ShapeShifterConstants.EXTERNAL_ASSETS_FOLDER);
        }

        public bool HasGuid(string guid) => GetAssetSkins().Any(assetSkin => assetSkin.Guid == guid);

        public AssetSkin GetAssetSkin(string guid)
        {
            List<AssetSkin> existing = GetAssetSkins();

            AssetSkin assetSkin = existing.FirstOrDefault(s => s.Guid == guid);

            return assetSkin;
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

        public void Duplicate(string newGame)
        {
            GameSkin newGameSkin = new GameSkin(newGame);
            FileUtil.CopyFileOrDirectory(MainFolder, newGameSkin.MainFolder);
        }
    }
}