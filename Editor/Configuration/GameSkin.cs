using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;

namespace Miniclip.ShapeShifter
{
    [Serializable]
    internal class GameSkin
    {
        private string name;

        private string mainFolder;
        private string internalSkinsFolder;
        private string externalSkinsFolder;

        internal DirectoryInfo mainFolderDirectoryInfo;

        internal GameSkin(string name)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));

            mainFolder = GetGameFolderPath(name);
            internalSkinsFolder = Path.Combine(GetGameFolderPath(name), ShapeShifter.InternalAssetsFolder);
            externalSkinsFolder = Path.Combine(GetGameFolderPath(name), ShapeShifter.ExternalAssetsFolder);
        }

        internal string ExternalSkinsFolder => externalSkinsFolder;

        internal string InternalSkinsFolder => internalSkinsFolder;

        internal string MainFolder => mainFolder;

        internal DirectoryInfo MainFolderDirectoryInfo
        {
            get
            {
                if (mainFolderDirectoryInfo != null)
                {
                    mainFolderDirectoryInfo.Refresh();
                    return mainFolderDirectoryInfo;
                }

                mainFolderDirectoryInfo = new DirectoryInfo(mainFolder);
                return mainFolderDirectoryInfo;
            }
            set => mainFolderDirectoryInfo = value;
        }

        private string GetGameFolderPath(string name)
        {
            return Path.Combine(ShapeShifter.SkinsFolder.FullName, name);
        }

        internal List<AssetSkin> GetAssetSkins(SkinType skinType)
        {
            List<AssetSkin> assetSkins = new List<AssetSkin>();
            if (Directory.Exists(internalSkinsFolder))
            {
                DirectoryInfo internalFolder = new DirectoryInfo(internalSkinsFolder);
                foreach (DirectoryInfo directory in internalFolder.GetDirectories())
                {
                    assetSkins.Add(new AssetSkin(directory.Name, directory.FullName));
                }
            }

            return assetSkins;
        }

        internal bool IsValid()
        {
            return Directory.Exists(mainFolder)
                   && (HasInternalSkins() || HasExternalSkins());
        }

        internal bool HasExternalSkins() => Directory.Exists(externalSkinsFolder);

        internal bool HasInternalSkins() => Directory.Exists(internalSkinsFolder);

        public bool HasGUID(string guid) =>
            GetAssetSkins(SkinType.Internal).Any(assetSkin => assetSkin.Guid == guid);

        public AssetSkin GetAssetSkin(string guid)
        {
            List<AssetSkin> existing = GetAssetSkins(SkinType.Internal);

            AssetSkin assetSkin = existing.FirstOrDefault(s => s.Guid == guid);

            return assetSkin;
        }
    }

    internal class AssetSkin
    {
        private string guid;
        private string path;

        public AssetSkin(string guid, string path)
        {
            this.Guid = guid;
            this.Path = path;
        }

        internal string Guid
        {
            get => guid;
            set => guid = value;
        }

        internal string Path
        {
            get => path;
            set => path = value;
        }

        public bool IsValid() => !IOUtils.IsFolderEmpty(path);
    }
}