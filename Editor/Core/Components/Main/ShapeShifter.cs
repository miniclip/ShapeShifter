using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    static class ShapeShifter
    {
        private static DirectoryInfo skinsFolder;

        internal static DirectoryInfo SkinsFolder
        {
            get
            {
                if (skinsFolder == null)
                {
                    skinsFolder = new DirectoryInfo(
                        Application.dataPath + $"/../../{ShapeShifterConstants.SKINS_MAIN_FOLDER}/"
                    );
                    IOUtils.TryCreateDirectory(SkinsFolder.FullName);
                }

                return skinsFolder;
            }
        }

        private static List<string> GameNames => config.GameNames;
        internal static HashSet<string> DirtyAssets { get; set; } = new HashSet<string>();

        internal static Dictionary<string, Texture2D> CachedPreviewPerAssetDict = new Dictionary<string, Texture2D>();

        internal static string ActiveGameName => ShapeShifterUtils.GetGameName(ActiveGame);

        private static ShapeShifterConfiguration config => ShapeShifterConfiguration.Instance;

        public static int ActiveGame
        {
            get
            {
                if (!ShapeShifterEditorPrefs.HasKey(ShapeShifterConstants.ACTIVE_GAME_PLAYER_PREFS_KEY))
                {
                    ShapeShifterLogger.LogWarning(
                        "Could not find any active game on EditorPrefs, defaulting to game 0"
                    );
                    ActiveGame = 0;
                }

                int activeGame = ShapeShifterEditorPrefs.GetInt(ShapeShifterConstants.ACTIVE_GAME_PLAYER_PREFS_KEY);

                if (activeGame >= GameNames.Count)
                {
                    ShapeShifterLogger.LogWarning("Current active game doesn't exist, defaulting to game 0.");
                    ActiveGame = 0;
                }

                return activeGame;
            }
            set
            {
                ShapeShifterLogger.Log(
                    $"Setting active game on EditorPrefs: {value}"
                );
                ShapeShifterEditorPrefs.SetInt(ShapeShifterConstants.ACTIVE_GAME_PLAYER_PREFS_KEY, value);
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

        public static void RemoveAllGames(bool deleteFolders = true)
        {
            foreach (string instanceGameName in GameNames.ToList())
            {
                config.RemoveGame(instanceGameName, deleteFolders);
            }
        }
    }
}