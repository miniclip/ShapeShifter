using System.Linq;
using Miniclip.ShapeShifter.Skinner;
using Miniclip.ShapeShifter.Switcher;
using UnityEngine;

namespace Miniclip.ShapeShifter.Saver
{
    public class AssetSaver : UnityEditor.AssetModificationProcessor
    {
        public static void OnWillSaveAssets(string[] files)
        {
            Debug.Log("SAVE!!!");
            // AssetSwitcher.OverwriteSelectedSkin(SharedInfo.ActiveGame);
            
            // var unityFiles = files.Where(x =>
            // {
            //     var lower = x.ToLower();
            //     return lower.EndsWith(".unity") || lower.EndsWith(".prefab");
            // });
            //
        }
    }
}