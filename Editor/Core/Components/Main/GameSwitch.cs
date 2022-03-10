using System;
using System.Linq;
using Miniclip.ShapeShifter.Switcher;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [InitializeOnLoad]
    public static class GameSwitch
    {
        private const string GAME_NAME_COMMAND_LINE_ARGUMENT = "-gameName";

        public static void FakeSwitchToVolley()
        {
            string[] fakeArgs = new[]
            {
                GAME_NAME_COMMAND_LINE_ARGUMENT, "Volleyball Arena"
            };
            
            SwitchInternal(fakeArgs);
        }
        
        public static void FakeSwitchToBadminton()
        {
            string[] fakeArgs = new[]
            {
                GAME_NAME_COMMAND_LINE_ARGUMENT, "Badminton"
            };
            
            SwitchInternal(fakeArgs);
        }

        public static void Switch()
        {
            SwitchInternal(Environment.GetCommandLineArgs());
        }

        private static void SwitchInternal(string[] commandLineArgs)
        {
            Debug.Log("HELLO");

            return;

            if (!commandLineArgs.Contains(GAME_NAME_COMMAND_LINE_ARGUMENT))
            {
                Debug.Log("1");
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

            if (gameNameCommandLineArgumentIndex + 1 >= commandLineArgs.Length)
            {
                Debug.Log("2");
                throw new Exception("Missing argument value with target game to switch to");
            }

            string gameName = commandLineArgs[gameNameCommandLineArgumentIndex + 1];

            if (!ShapeShifterConfiguration.IsInitialized())
            {
                ShapeShifterInitializer.Init();
            }

            if (!ShapeShifterConfiguration.Instance.GameNames.Contains(gameName))
            {
                Debug.Log("3");
                throw new Exception($"Shapeshifter unable to find {gameName} in configuration");
            }

            GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(gameName);

            AssetSwitcher.SwitchToGame(gameSkin, true);
        }
    }
}