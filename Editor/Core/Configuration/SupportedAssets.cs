using System;
using System.Collections.Generic;
using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Miniclip.ShapeShifter
{
    public static class SupportedAssets
    {
        private static readonly Type[] assetTypesSupported =
        {
            typeof(AnimationClip),
            typeof(AnimatorController),
            typeof(DefaultAsset),
            typeof(GameObject),
            typeof(SceneAsset),
            typeof(ScriptableObject),
            typeof(Texture2D),
            typeof(TextAsset),
            typeof(MonoScript),
            typeof(Shader)
        };

        private static readonly Type[] typesForbidden =
        {
            
        };

        public static bool IsAssetTypeSkinnable(string assetPath, out string reason)
        {
              return IsAssetTypeSkinnable(AssetDatabase.LoadAssetAtPath<Object>(assetPath), out reason);
        }

        private static bool IsAssetTypeSkinnable(Object asset, out string reason)
        {
            Type assetType = asset.GetType();

            string assetPath = AssetDatabase.GetAssetPath(asset);

            reason = "Asset type is not supported by shapeshifter.";

            if (PathUtils.IsPathRelativeToPackages(assetPath))
            {
                reason = "Unable to skin package contents";
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

            return assetTypesSupported.Any(
                typeSupported => assetType == typeSupported || assetType.IsSubclassOf(typeSupported)
            );
        }

        public static List<(Object asset, bool isSupported, string reason)> GetAssetsSupportInfo(this Object[] assets)
        {
            List<(Object, bool, string)> assetsSupportInfo = new List<(Object, bool, string)>(assets.Length);

            foreach (Object asset in assets)
            {
                bool isSupported = IsAssetTypeSkinnable(asset, out string reason);

                assetsSupportInfo.Add((asset, isSupported, reason));
            }

            return assetsSupportInfo;
        }
    }
}