using System.Collections.Generic;
using System.IO;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using Miniclip.ShapeShifter.Watcher;
using UnityEditor;

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
                RemoveSkinFromGame(assetPath, game);
            }

            EditorUtility.DisplayProgressBar("Asset Skinner", $"Tracking {assetPath}", 0.95f);

            GitUtils.Track(guid, assetPath);
            EditorUtility.ClearProgressBar();
        }

        public static void RemoveSkinFromGame(string assetPath, string game)
        {
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(game);
            AssetSkin assetSkin = gameSkin.GetAssetSkin(guid);
            string key = ShapeShifterUtils.GenerateUniqueAssetSkinKey(game, guid);
            
            ShapeShifter.DirtyAssets.Remove(key);
            ShapeShifter.CachedPreviewPerAssetDict.Remove(key);
            AssetWatcher.StopWatchingFolder(assetSkin.FolderPath);
            
            EditorUtility.DisplayProgressBar("Asset Skinner", $"Deleting asset skin folder", 0.5f);

            assetSkin.Delete();

            EditorUtility.DisplayProgressBar("Asset Skinner", $"Staging changes", 0.95f);

            GitUtils.Stage(assetSkin.FolderPath);

            if (!IsSkinned(assetPath))
            {
                EditorUtility.DisplayProgressBar("Asset Skinner", $"Tracking {assetPath}", 0.95f);
                GitUtils.Track(guid, assetPath);
            }

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

            if (saveFirst)
            {
                EditorUtility.DisplayProgressBar("Asset Skinner", $"Saving pending changes", 0.2f);
                ShapeShifterUtils.SavePendingChanges();
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
            {
                SkinAssetForGame(assetPath, ShapeShifterConfiguration.Instance.GetGameSkinByName(game));
            }
        }

        public static void SkinAssetForGame(string assetPath, string game)
        {
            SkinAssetForGame(assetPath, ShapeShifterConfiguration.Instance.GetGameSkinByName(game));
        }

        public static void SkinAssetForGame(string assetPath, GameSkin gameSkin)
        {
            string origin = assetPath;
            string game = gameSkin.Name;
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            string assetFolder = Path.Combine(
                gameSkin.InternalSkinsFolderPath,
                guid
            );

            EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Checking if its skinned already", 0.3f);

            if (IsSkinned(origin, game))
            {
                return;
            }

            EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Creating directory for asset", 0.3f);
            FileUtils.TryCreateDirectory(assetFolder, true);

            string target = Path.Combine(assetFolder, Path.GetFileName(origin));

            EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Copying asset to skin directory", 0.3f);

            FileUtils.SafeCopy(origin, target);
            FileUtils.SafeCopy(origin + ".meta", target + ".meta");

            EditorUtility.DisplayProgressBar($"Skinning for {game}", $"Staging changes", 0.3f);

            GitUtils.Stage(assetFolder);

            EditorUtility.DisplayProgressBar($"Asset Skinner", $"Untracking {assetPath}", 0.5f);

            GitUtils.Untrack(guid, assetPath, true);
            EditorUtility.ClearProgressBar();
        }

        public static bool IsSkinned(string assetPath) =>
            ShapeShifterConfiguration.Instance.GameNames.Any(game => IsSkinned(assetPath, game));

        public static bool IsSkinned(string assetPath, string game)
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

            return FileUtils.DoesFolderExistAndHaveFiles(assetFolder);
        }
    }
}