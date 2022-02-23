using System;
using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Miniclip.ShapeShifter
{
    public static class SupportedTypes
    {
        private static readonly Type[] typesSupported =
        {
            typeof(AnimationClip),
            typeof(AnimatorController),
            typeof(DefaultAsset),
            typeof(GameObject),
            typeof(SceneAsset),
            typeof(ScriptableObject),
            typeof(Texture2D),
            typeof(TextAsset),
        };

        private static readonly Type[] typesForbidden =
        {
            typeof(MonoScript),
        };

        public static bool IsSupported(string assetPath, out string reason)
        {
            return IsSupported(AssetDatabase.LoadAssetAtPath<Object>(assetPath), out reason);
        }

        public static bool IsSupported(Object asset, out string reason)
        {
            Type assetType = asset.GetType();

            string path = AssetDatabase.GetAssetPath(asset);

            reason = "Asset type is supported by shapeshifter.";

            if (PathUtils.IsPathRelativeToPackages(path))
            {
                reason = "Shapeshifter still can't handle skinning package contents";
                return false;
            }

            if (assetType == typeof(DefaultAsset))
            {
                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(asset)))
                {
                    return true;
                }
            }

            if (typesForbidden.Any(
                    typeForbidden => assetType == typeForbidden || assetType.IsSubclassOf(typeForbidden)
                ))
            {
                reason = $"{assetType.Name} type is not skinnable.";
                return false;
            }

            return typesSupported.Any(
                typeSupported => assetType == typeSupported || assetType.IsSubclassOf(typeSupported)
            );
        }

        public static List<(Object asset, bool isSupported, string reason)> GetAssetsSupportInfo(this Object[] assets)
        {
            List<(Object, bool, string)> assetsSupportInfo = new List<(Object, bool, string)>(assets.Length);

            foreach (Object asset in assets)
            {
                var isSupported = IsSupported(asset, out string reason);

                assetsSupportInfo.Add((asset, isSupported, reason));
            }

            return assetsSupportInfo;
        }
    }
}