using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Watcher;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Skinner
{
    public static class AssetSkinner
    {
        public static void RemoveSkins(string[] assetPaths, bool saveFirst = true)
        {
            foreach (string assetPath in assetPaths)
            {
                RemoveSkins(assetPath);
            }
        }

        public static void RemoveSkins(string assetPath)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            EditorUtility.DisplayProgressBar("Asset Skinner", $"Removing {assetPath} skin", 0);
            foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
            {
                string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);
                ShapeShifter.DirtyAssets.Remove(key);
                ShapeShifter.CachedPreviewPerAssetDict.Remove(key);

                string assetFolder = Path.Combine(
                    ShapeShifter.SkinsFolder.FullName,
                    game,
                    ShapeShifterConstants.INTERNAL_ASSETS_FOLDER,
                    guid
                );

                AssetWatcher.StopWatchingFolder(assetFolder);
                EditorUtility.DisplayProgressBar("Asset Skinner", $"Deleting asset skin folder", 0.5f);

                if (Directory.Exists(assetFolder))
                {
                    Directory.Delete(assetFolder, true);
                }

                EditorUtility.DisplayProgressBar("Asset Skinner", $"Staging changes", 0.95f);

                GitUtils.Stage(assetFolder);
            }

            EditorUtility.DisplayProgressBar("Asset Skinner", $"Tracking {assetPath}", 0.95f);

            GitUtils.Track(guid);
            EditorUtility.ClearProgressBar();
        }

        internal static void RemoveAllInternalSkins()
        {
            List<AssetSkin> assetSkins = ShapeShifter.ActiveGameSkin.GetAssetSkins();
            foreach (AssetSkin assetSkin in assetSkins)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetSkin.Guid);
                RemoveSkins(assetPath);
            }
        }

        public static void SkinAssets(string[] assetPaths, bool saveFirst = true)
        {
            if (saveFirst)
            {
                ShapeShifterUtils.SavePendingChanges();
            }

            foreach (string assetPath in assetPaths)
            {
                SkinAsset(assetPath, false);
            }
        }

        public static void SkinAsset(string assetPath, bool saveFirst = true)
        {
            EditorUtility.DisplayProgressBar("Asset Skinner", $"Checking if {assetPath} is supported", 0);
            if (!SupportedTypes.IsSupported(assetPath, out string reason))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            if (IsSkinned(assetPath))
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            if (saveFirst)
            {
                EditorUtility.DisplayProgressBar("Asset Skinner", $"Saving pending changes", 0.2f);
                // make sure any pending changes are saved before generating copies
                ShapeShifterUtils.SavePendingChanges();
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
            {
                string origin = assetPath;
                string assetFolder = Path.Combine(
                    ShapeShifter.SkinsFolder.FullName,
                    game,
                    ShapeShifterConstants.INTERNAL_ASSETS_FOLDER,
                    guid
                );

                EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Checking if its skinned already", 0.3f);

                if (IsSkinned(origin, game))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Creating directory for asset", 0.3f);
                IOUtils.TryCreateDirectory(assetFolder, true);

                string target = Path.Combine(assetFolder, Path.GetFileName(origin));

                EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Copying asset to skin directory", 0.3f);
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    DirectoryInfo targetFolder = Directory.CreateDirectory(target);
                    IOUtils.CopyFolder(new DirectoryInfo(origin), targetFolder);
                    IOUtils.CopyFile(origin + ".meta", target + ".meta");
                }
                else
                {
                    IOUtils.CopyFile(origin, target);
                    IOUtils.CopyFile(origin + ".meta", target + ".meta");
                }

                EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Staging changes", 0.3f);

                GitUtils.Stage(assetFolder);
            }

            EditorUtility.DisplayProgressBar($"Asset Skinner", $"Untracking {assetPath}", 0.5f);
            GitUtils.Untrack(guid);
            EditorUtility.ClearProgressBar();
        }

        public static bool IsSkinned(string assetPath) =>
            ShapeShifterConfiguration.Instance.GameNames.All(game => IsSkinned(assetPath, game));

        private static bool IsSkinned(string assetPath, string game)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid))
                return false;

            string assetFolder = Path.Combine(
                ShapeShifter.SkinsFolder.FullName,
                game,
                ShapeShifterConstants.INTERNAL_ASSETS_FOLDER,
                guid
            );

            return Directory.Exists(assetFolder) && !IOUtils.IsFolderEmpty(assetFolder);
        }

        private static void OnDisable()
        {
            ShapeShifter.DirtyAssets.Clear();
            ShapeShifter.CachedPreviewPerAssetDict.Clear();
        }

        private static IEnumerable<string> GetEligibleAssetPaths(Object[] assets)
        {
            IEnumerable<string> assetPaths =
                assets.Select(AssetDatabase.GetAssetPath);
            RemoveEmptyAssetPaths(ref assetPaths);
            RemoveDuplicatedAssetPaths(ref assetPaths);
            RemoveAlreadySkinnedAssets(ref assetPaths);
            return assetPaths;
        }

        private static void RemoveEmptyAssetPaths(ref IEnumerable<string> assetPaths) =>
            assetPaths = assetPaths.Where(assetPath => !string.IsNullOrEmpty(assetPath));

        private static void RemoveDuplicatedAssetPaths(ref IEnumerable<string> assetPaths) =>
            assetPaths = assetPaths.Distinct();

        private static void RemoveAlreadySkinnedAssets(ref IEnumerable<string> assetPaths) =>
            assetPaths = assetPaths.Where(assetPath => !IsSkinned(assetPath));

        internal static void CreateGameSkinFolder(string gameName)
        {
            GameSkin gameSkin = new GameSkin(gameName);

            IOUtils.TryCreateDirectory(gameSkin.MainFolderPath);
        }
    }
}