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

        private static string DetermineRecommendedPath() => GitUtils.RepositoryPath;

        internal static string GenerateKeyFromRelativePath(string relativePath) =>
            WebUtility.UrlEncode(relativePath).Replace(".", "{dot}");

        internal static string GenerateRelativePathFromKey(string key) =>
            WebUtility.UrlDecode(key).Replace("{dot}", ".");

        // since Path.GetRelativePath doesn't seem to be available
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
            foreach (string path in ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths)
            {
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
                    ShapeShifter.EXTERNAL_ASSETS_FOLDER,
                    key
                );

                Directory.Delete(assetFolder, true);
            }

            ShapeShifterConfiguration.Instance.SkinnedExternalAssetPaths.Remove(relativePath);

            EditorUtility.SetDirty(ShapeShifterConfiguration.Instance);
            ShapeShifterUtils.SavePendingChanges();
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
            if (absoluteAssetPath == null)
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
            EditorUtility.SetDirty(ShapeShifterConfiguration.Instance);

            // even though it's an "external" file, it still might be a Unity file (ex: ProjectSettings), so it's
            // still important to make sure any pending changes are saved before generating copies
            ShapeShifterUtils.SavePendingChanges();

            string origin = absoluteAssetPath;
            string key = GenerateKeyFromRelativePath(relativeAssetPath);

            foreach (string game in ShapeShifterConfiguration.Instance.GameNames)
            {
                string assetFolder = Path.Combine(
                    ShapeShifter.SkinsFolder.FullName,
                    game,
                    ShapeShifter.EXTERNAL_ASSETS_FOLDER,
                    key
                );

                if (!Directory.Exists(assetFolder))
                {
                    Directory.CreateDirectory(assetFolder);
                }

                string target = Path.Combine(assetFolder, Path.GetFileName(origin));

                if (overridesPerGame != null && overridesPerGame.ContainsKey(game))
                {
                    origin = overridesPerGame[game];
                }

                IOUtils.CopyFile(origin, target);
            }
        }
    }
}