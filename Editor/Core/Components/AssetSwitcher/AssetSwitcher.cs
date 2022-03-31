using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Miniclip.ShapeShifter.Saver;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Utils.Git;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Switcher
{
    public static class AssetSwitcher
    {
        public static void DeleteAssetInternalCopy(string guid)
        {
            string assetPathFromAssetDatabase = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPathFromAssetDatabase)
                || !PathUtils.FileOrDirectoryExists(assetPathFromAssetDatabase))
            {
                return;
            }

            FileUtils.SafeDelete(assetPathFromAssetDatabase);
            FileUtils.SafeDelete(assetPathFromAssetDatabase + ".meta");
        }

        public static void RefreshAsset(string guid)
        {
            if (string.IsNullOrEmpty(guid))
            {
                return;
            }

            GameSkin currentGameSkin = ShapeShifter.ActiveGameSkin;

            AssetSkin assetSkin = currentGameSkin.GetAssetSkin(guid);

            AssetSwitcherOperations.CopyFromSkinsToUnity(new DirectoryInfo(assetSkin.FolderPath));

            RefreshAllAssets();
        }

        internal static void OverwriteSelectedSkin(GameSkin selected, bool forceOverwrite = false)
        {
            string name = selected.Name;

            if (ShapeShifter.ActiveGameSkin != selected)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append($"This will overwrite the {name} skins with the current assets. ");

                stringBuilder.Append($"The last asset switch was to {ShapeShifter.ActiveGameName}");

                stringBuilder.Append(" Are you sure?");

                if (!forceOverwrite
                    && !EditorUtility.DisplayDialog(
                        "Shape Shifter",
                        stringBuilder.ToString(),
                        "Yeah, I'm sure, go ahead.",
                        "Wait, what? No, stop!"
                    ))
                {
                    return;
                }
            }

            AssetSwitcherOperations.PerformCopiesWithTracking(
                selected,
                "Overwrite selected skin",
                AssetSwitcherOperations.CopyFromUnityToSkins,
                AssetSwitcherOperations.CopyFromOriginToSkinnedExternal
            );

            ShapeShifterConfiguration.Instance.SetDirty(false);
            UnsavedAssetsManager.ClearModifiedAssetsList();
            ShapeShifter.SaveDetected = false;
        }

        internal static void RefreshAllAssets()
        {
            if (HasAnyPackageRelatedSkin() && !Application.isBatchMode)
            {
                ForceUnityToLoseAndRegainFocus();
            }

            AssetDatabase.Refresh();
        }

        private static void ForceUnityToLoseAndRegainFocus()
        {
            // Force Unity to lose and regain focus, so it resolves any new changes on the packages
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments =
                        "-e 'tell application \"Finder\" to activate' -e 'delay 0.5' -e 'tell application \"Unity\" to activate'",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
        }

        internal static void SwitchToGame(GameSkin gameToSwitchTo, bool forceSwitch = false)
        {
            if (ShapeShifterConfiguration.Instance.IsDirty && !forceSwitch)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "Shape Shifter",
                    "There are unsaved changes in your skinned assets. You should make sure to save them into your Active Game folder",
                    $"Save changes to {ShapeShifter.ActiveGameName} and switch to {gameToSwitchTo.Name}.",
                    "Cancel Switch",
                    $"Discard changes and switch to {gameToSwitchTo.Name}"
                );

                switch (choice)
                {
                    case 0:
                        OverwriteSelectedSkin(ShapeShifter.ActiveGameSkin);
                        break;

                    case 1:
                        return;

                    case 2:
                    default:
                        break;
                }
            }

            RemoveSkinnedAssetsFromProject();

            AssetSwitcherOperations.PerformCopiesWithTracking(
                gameToSwitchTo,
                "Switch to game",
                AssetSwitcherOperations.CopyFromSkinsToUnity,
                AssetSwitcherOperations.CopyFromSkinnedExternalToOrigin
            );
            ShapeShifter.ActiveGame = gameToSwitchTo.Name;
            UnsavedAssetsManager.ClearModifiedAssetsList();
            ShapeShifterConfiguration.Instance.SetDirty(false);
        }

        private static void RemoveSkinnedAssetsFromProject()
        {
            var currentGitIgnore = GitIgnore.GitIgnoreWrapper.Instance();

            foreach (string skinnedGuid in currentGitIgnore.Keys)
            {
                DeleteAssetInternalCopy(skinnedGuid);
            }
        }

        internal static void RestoreActiveGame()
        {
            SwitchToGame(ShapeShifter.ActiveGameSkin);
        }

        private static bool HasAnyPackageRelatedSkin()
        {
            bool isManifestSkinned = ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.Any(
                externalAssetPath => externalAssetPath.Contains("manifest.json")
            );

            return isManifestSkinned;
        }
    }
}