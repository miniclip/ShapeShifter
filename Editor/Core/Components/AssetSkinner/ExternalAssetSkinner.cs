using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter.Skinner
{
    public static class ExternalAssetSkinner
    {
        private static Editor externalConfigurationEditor;

        private static string DetermineRecommendedPath() => GitUtils.MainRepositoryPath;

        internal static string GenerateKeyFromRelativePath(string relativePath) =>
            WebUtility.UrlEncode(relativePath).Replace(".", "{dot}");

        internal static string GenerateRelativePathFromKey(string key) =>
            WebUtility.UrlDecode(key).Replace("{dot}", ".");

        private static string GetRelativeURIPath(string absolutePath, string relativeTo)
        {
            if (!relativeTo.EndsWith("/"))
            {
                relativeTo += "/";
            }

            Uri assetPathIdentifier = new Uri(absolutePath);
            Uri relativeToPathIdentifier = new Uri(relativeTo);
            return relativeToPathIdentifier.MakeRelativeUri(assetPathIdentifier).ToString();
        }

        private static string PickFile(string recommendedPath)
        {
            string assetPath = EditorUtility.OpenFilePanel(
                "Pick a file, any file!",
                recommendedPath,
                string.Empty
            );

            if (string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            if (!assetPath.Contains(recommendedPath)
                && !EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"The chosen asset is outside of the recommended path ({recommendedPath}). Are you sure?",
                    "Yeah, go for it!",
                    "Hmm... not sure, let me check!"
                ))
            {
                return null;
            }

            return assetPath;
        }

        public static void RemoveAllExternalSkins()
        {
            var listCopy = ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.ToArray();

            for (var index = 0; index < listCopy.Length; index++)
            {
                string path = listCopy[index];
                RemoveExternalSkins(path);
            }
        }

        internal static void RemoveExternalSkins(string relativePath)
        {
            string key = GenerateKeyFromRelativePath(relativePath);

            foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
            {
                ShapeShifter.DirtyAssets.Remove(key);
                ShapeShifter.CachedPreviewPerAssetDict.Remove(key);

                string assetFolder = Path.Combine(
                    ShapeShifter.SkinsFolder.FullName,
                    game,
                    ShapeShifterConstants.EXTERNAL_ASSETS_FOLDER,
                    key
                );

                if (Directory.Exists(assetFolder))
                {
                    Directory.Delete(assetFolder, true);
                }

                GitUtils.Stage(assetFolder);
            }

            GitUtils.Track(key, relativePath);

            ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.Remove(relativePath);

            EditorUtility.SetDirty(ShapeShifterConfiguration.Instance);
            ShapeShifterUtils.SavePendingChanges();
            
            GitUtils.Stage(AssetDatabase.GetAssetPath(ShapeShifterConfiguration.Instance));
        }

        internal static void SkinExternalFile()
        {
            string recommendedPath = DetermineRecommendedPath();
            string absoluteAssetPath = PickFile(recommendedPath);

            SkinExternalFile(absoluteAssetPath);
        }

        private static void SkinExternalFile(string absoluteAssetPath,
            Dictionary<string, string> overridesPerGame = null)
        {
            if (string.IsNullOrEmpty(absoluteAssetPath))
            {
                return;
            }

            string relativeAssetPath = GetRelativeURIPath(absoluteAssetPath, Application.dataPath);

            if (ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.Contains(relativeAssetPath))
            {
                EditorUtility.DisplayDialog(
                    "Shape Shifter",
                    $"Could not skin: {relativeAssetPath}. It was already skinned.",
                    "Oops!"
                );

                return;
            }

            ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.Add(relativeAssetPath);
            ShapeShifterConfiguration.Instance.SetDirty(true);

            // even though it's an "external" file, it still might be a Unity file (ex: ProjectSettings), so it's
            // still important to make sure any pending changes are saved before generating copies
            ShapeShifterUtils.SavePendingChanges();

            string origin = absoluteAssetPath;
            string key = GenerateKeyFromRelativePath(relativeAssetPath);

            foreach (string gameName in ShapeShifterConfiguration.Instance.GameNames)
            {
                GameSkin gameSkin = ShapeShifterConfiguration.Instance.GetGameSkinByName(gameName);

                string assetFolder = Path.Combine(gameSkin.ExternalSkinsFolderPath,
                    key
                );

                IOUtils.TryCreateDirectory(assetFolder);

                string target = Path.Combine(assetFolder, Path.GetFileName(origin));

                if (overridesPerGame != null && overridesPerGame.ContainsKey(gameName))
                {
                    origin = overridesPerGame[gameName];
                }

                IOUtils.CopyFile(origin, target);

                GitUtils.Stage(target);
            }

            GitUtils.Untrack(key, origin, false);

            GitUtils.Stage(AssetDatabase.GetAssetPath(ShapeShifterConfiguration.Instance));
        }
    }
}