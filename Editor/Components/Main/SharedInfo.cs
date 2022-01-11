using System.Collections.Generic;
using System.IO;
using Miniclip.ShapeShifter.Utils;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    static class SharedInfo
    {
        private static readonly string SHAPESHIFTER_SKINS_FOLDER_NAME = "Skins";

        private static readonly string ACTIVE_GAME_PLAYER_PREFS_KEY = "ACTIVE_GAME_PLAYER_PREFS_KEY";

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

        internal static string ActiveGameName => ShapeShifterUtils.GetGameName(ActiveGame);

        public static int ActiveGame
        {
            get
            {
                if (!ShapeShifterEditorPrefs.HasKey(ACTIVE_GAME_PLAYER_PREFS_KEY))
                {
                    ShapeShifterLogger.Log(
                        "Could not find any active game on EditorPrefs, setting by default game 0"
                    );
                    ActiveGame = 0;
                }

                return ShapeShifterEditorPrefs.GetInt(ACTIVE_GAME_PLAYER_PREFS_KEY);
            }
            set
            {
                ShapeShifterLogger.Log(
                    $"Setting active game on EditorPrefs: {value}"
                );
                ShapeShifterEditorPrefs.SetInt(ACTIVE_GAME_PLAYER_PREFS_KEY, value);
            }
        }

        private static GameSkin activeGameSkin;

        internal static GameSkin ActiveGameSkin
        {
            get
            {
                if (activeGameSkin != null && activeGameSkin.Name == ActiveGameName)
                {
                    return activeGameSkin;
                }

                activeGameSkin = new GameSkin(ActiveGameName);

                return activeGameSkin;
            }
        }
    }
}