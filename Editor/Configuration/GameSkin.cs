using System;
using System.Collections.Generic;
using System.IO;

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

        internal List<string> GetExistingGUIDs(SkinType skinType)
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

        internal bool IsValid()
        {
            return Directory.Exists(mainFolder)
                   && (HasInternalSkins() || HasExternalSkins());
        }

        internal bool HasExternalSkins() => Directory.Exists(externalSkinsFolder);

        internal bool HasInternalSkins() => Directory.Exists(internalSkinsFolder);
    }
}