using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Miniclip.ShapeShifter
{
    public static class SupportedTypes
    {
        private static readonly Type[] Types =
        {
            typeof(AnimationClip),
            typeof(AnimatorController),
            typeof(DefaultAsset),
            typeof(GameObject),
            typeof(MonoScript),
            typeof(SceneAsset),
            typeof(ScriptableObject),
            typeof(Texture2D)
        };

        public static bool IsSupported(Object asset)
        {
            Type assetType = asset.GetType();

            if (assetType == typeof(DefaultAsset))
            {
                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(asset)))
                {
                    return true;
                }
            }

            foreach (Type supportedType in Types)
            {
                if (assetType == supportedType || assetType.IsSubclassOf(supportedType))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<Object> GetSupportedAssetsFromArray(this Object[] assets)
        {
            List<Object> supportedAssets = new List<Object>(assets.Length);

            foreach (Object asset in assets)
            {
                if (SupportedTypes.IsSupported(asset))
                {
                    supportedAssets.Add(asset);
                }
            }

            return supportedAssets;
        }
    }
}