﻿using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    public class ShapeShifterAssetPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string importedAsset in importedAssets)
            {
                Debug.Log($"imported {importedAsset}");
                ShapeShifter.Instance.RegisterModifiedAssetInUnity(importedAsset);
            }

            foreach (string deletedAsset in deletedAssets)
            {
                //TODO ACPT-2755 handle deleted skinned assets

                if (ShapeShifter.IsSkinned(deletedAsset))
                {
                    ShapeShifterLogger.LogWarning("You deleted an asset that currently has skins. Shapeshifter still can't handle this correctly");
                }
            }

            foreach (string movedAsset in movedAssets)
            {
                Debug.Log($"Moved {movedAsset}");
                ShapeShifter.Instance.RegisterModifiedAssetInUnity(movedAsset);

            }
        }
    }
}