using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Miniclip.ShapeShifter.Switcher;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [InitializeOnLoad]
    public static class GameSwitch
    {
        private const string GAME_NAME_COMMAND_LINE_ARGUMENT = "-gameName";

        [UsedImplicitly]
        public static void Switch()
        {
            SwitchInternal(Environment.GetCommandLineArgs());
        }

        private static async Task SwitchInternal(string[] commandLineArgs)
        {
            if (!commandLineArgs.Contains(GAME_NAME_COMMAND_LINE_ARGUMENT))
            {
                throw new Exception("Missing game name command to continue with switch operation");
            }

            int gameNameCommandLineArgumentIndex = commandLineArgs.ToList()
                .FindIndex(
                    argument => string.Equals(
                        argument,
                        GAME_NAME_COMMAND_LINE_ARGUMENT,
                        StringComparison.OrdinalIgnoreCase
                    )
                );

            if ((gameNameCommandLineArgumentIndex + 1) >= commandLineArgs.Length)
            {
                throw new Exception("Missing argument value with target game to switch to");
            }

            string gameName = commandLineArgs[gameNameCommandLineArgumentIndex + 1];

            if (!ShapeShifterConfiguration.IsInitialized())
            {
                await ShapeShifterInitializer.Init();
            }

            if (!ShapeShifterConfiguration.Instance.GameNames.Contains(gameName))
            {
                throw new Exception($"Shapeshifter unable to find {gameName} in configuration");
            }

            GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(gameName);

            AssetSwitcher.SwitchToGame(gameSkin, true);
            
            // Temporary fix until GICache freezing execution bug gets fixed
            EditorApplication.Exit(0);
        }
    }
}