using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Miniclip.ShapeShifter
{
    [Serializable]
    class GameSkin
    {
        internal string ExternalSkinsFolder { get; }

        internal string InternalSkinsFolder { get; }

        internal string MainFolder { get; }

        internal DirectoryInfo MainFolderDirectoryInfo
        {
            get
            {
                if (mainFolderDirectoryInfo != null)
                {
                    mainFolderDirectoryInfo.Refresh();
                    return mainFolderDirectoryInfo;
                }

                mainFolderDirectoryInfo = new DirectoryInfo(MainFolder);
                return mainFolderDirectoryInfo;
            }
            set => mainFolderDirectoryInfo = value;
        }

        internal string Name { get; }

        internal DirectoryInfo mainFolderDirectoryInfo;

        internal GameSkin(string name)
        {
            Name = name;

            MainFolder = GetGameFolderPath(name);
            InternalSkinsFolder = Path.Combine(MainFolder, SharedInfo.InternalAssetsFolder);
            ExternalSkinsFolder = Path.Combine(MainFolder, SharedInfo.ExternalAssetsFolder);
        }

        public bool HasGUID(string guid) => GetAssetSkins(SkinType.Internal).Any(assetSkin => assetSkin.Guid == guid);

        public AssetSkin GetAssetSkin(string guid)
        {
            List<AssetSkin> existing = GetAssetSkins(SkinType.Internal);

            AssetSkin assetSkin = existing.FirstOrDefault(s => s.Guid == guid);

            return assetSkin;
        }

        private string GetGameFolderPath(string name) => Path.Combine(ShapeShifter.SkinsFolder.FullName, name);

        internal List<AssetSkin> GetAssetSkins(SkinType skinType)
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

        internal bool IsValid() =>
            Directory.Exists(MainFolder)
            && (HasInternalSkins() || HasExternalSkins());

        internal bool HasExternalSkins() => Directory.Exists(ExternalSkinsFolder);

        internal bool HasInternalSkins() => Directory.Exists(InternalSkinsFolder);

        internal void DeleteFolder()
        {
            if(Directory.Exists(MainFolder))
                Directory.Delete(MainFolder, true);
        }
    }
}