using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public static class ShapeShifter
    {
        public static bool SaveDetected { get; set; }

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
                    FileUtils.TryCreateDirectory(SkinsFolder.FullName);
                }

                return skinsFolder;
            }
        }

        private static List<string> GameNames => config.GameNames;

        internal static HashSet<string> DirtyAssets { get; set; } = new HashSet<string>();

        internal static Dictionary<string, Texture2D> CachedPreviewPerAssetDict = new Dictionary<string, Texture2D>();

        internal static string ActiveGameName => ActiveGame;

        private static ShapeShifterConfiguration config => ShapeShifterConfiguration.Instance;

        public static string ActiveGame
        {
            get => GetActiveGame();

            set
            {
                if (!GameNames.Contains(value))
                {
                    SetDefaultGameSkin();
                }

                ShapeShifterLogger.Log(
                    $"Setting active game on EditorPrefs: {value}"
                );
                Persistence.SetString(
                    ShapeShifterConstants.ACTIVE_GAME_PLAYER_PREFS_KEY,
                    value,
                    PersistenceType.MachinePersistent
                );
                ActiveGameSkin = new GameSkin(value);
            }
        }

        private static string GetActiveGame()
        {
            if (!Persistence.HasKey(
                    ShapeShifterConstants.ACTIVE_GAME_PLAYER_PREFS_KEY,
                    PersistenceType.MachinePersistent
                ))
            {
                ShapeShifterLogger.LogWarning(
                    "Could not find any active game on EditorPrefs, defaulting to game 0"
                );
                ActiveGame = GameNames.FirstOrDefault();
            }

            string activeGame =
                Persistence.GetString(
                    ShapeShifterConstants.ACTIVE_GAME_PLAYER_PREFS_KEY,
                    PersistenceType.MachinePersistent
                );

            if (!GameNames.Contains(activeGame))
            {
                ShapeShifterLogger.LogWarning("Current active game doesn't exist, defaulting to game 0.");
                SetDefaultGameSkin();
            }

            return activeGame;
        }

        private static void SetDefaultGameSkin()
        {
            ActiveGame = GameNames.FirstOrDefault();
        }

        private static GameSkin activeGameSkin;

        internal static GameSkin ActiveGameSkin
        {
            get
            {
                if (activeGameSkin?.Name == ActiveGame)
                {
                    return activeGameSkin;
                }

                activeGameSkin = new GameSkin(ActiveGame);

                return activeGameSkin;
            }

            private set => activeGameSkin = value;
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