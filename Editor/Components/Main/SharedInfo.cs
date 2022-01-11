using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    static class SharedInfo
    {
        internal static readonly string SHAPESHIFTER_NAME = "ShapeShifter";
        private static readonly string SHAPESHIFTER_SKINS_FOLDER_NAME = "Skins";

        internal static readonly string ExternalAssetsFolder = "external";
        internal static readonly string InternalAssetsFolder = "internal";
        
        private static DirectoryInfo skinsFolder;

        internal static DirectoryInfo SkinsFolder
        {
            get
            {
                if (skinsFolder == null)
                {
                    skinsFolder = new DirectoryInfo(Application.dataPath + $"/../../{SHAPESHIFTER_SKINS_FOLDER_NAME}/");
                    IOUtils.TryCreateDirectory(SkinsFolder.FullName);
                }

                return skinsFolder;
            }
            set => skinsFolder = value;
        }

        internal static HashSet<string> DirtyAssets { get; set; } = new HashSet<string>();
        
        internal static Dictionary<string, Texture2D> CachedPreviewPerAssetDict = new Dictionary<string, Texture2D>();

    }
}