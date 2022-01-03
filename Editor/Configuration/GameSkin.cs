using System;
using System.Collections.Generic;
using System.IO;

namespace Miniclip.ShapeShifter
{
    [Serializable]
    public class GameSkin
    {
        private string name;

        private string mainFolder;
        private string internalSkinsFolder;
        private string externalSkinsFolder;

        public DirectoryInfo mainFolderDirectoryInfo;

        public GameSkin(string name)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));

            mainFolder = GetGameFolderPath(name);
            internalSkinsFolder = Path.Combine(GetGameFolderPath(name), ShapeShifter.InternalAssetsFolder);
            externalSkinsFolder = Path.Combine(GetGameFolderPath(name), ShapeShifter.ExternalAssetsFolder);
        }

        public string ExternalSkinsFolder => externalSkinsFolder;

        public string InternalSkinsFolder => internalSkinsFolder;

        public string MainFolder => mainFolder;

        public DirectoryInfo MainFolderDirectoryInfo
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

        public List<string> GetGUIDs(SkinType skinType)
        {
            List<string> guids = new List<string>();
            if (Directory.Exists(internalSkinsFolder))
            {
                DirectoryInfo internalFolder = new DirectoryInfo(internalSkinsFolder);

                foreach (DirectoryInfo directory in internalFolder.GetDirectories())
                {
                    guids.Add(directory.Name);
                }
            }

            return guids;
        }

        public bool IsValid()
        {
            return Directory.Exists(mainFolder)
                   && (HasInternalSkins() || HasExternalSkins());
        }

        internal bool HasExternalSkins() => Directory.Exists(externalSkinsFolder);

        internal bool HasInternalSkins() => Directory.Exists(internalSkinsFolder);
    }
}