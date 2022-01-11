using System;
using System.IO;
using System.Reflection;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

namespace Miniclip.ShapeShifter
{
    [Serializable]
    public partial class ShapeShifter : EditorWindow
    {
        
        private static DirectoryInfo skinsFolder;

        public static DirectoryInfo SkinsFolder
        {
            get
            {
                if (skinsFolder == null)
                {
                    skinsFolder = new DirectoryInfo(Application.dataPath + "/../../Skins/");
                    IOUtils.TryCreateDirectory(SkinsFolder.FullName, false);
                }

                return skinsFolder;
            }
            set => skinsFolder = value;
        }
        
        private static string GenerateAssetKey(string game, string guid) => game + ":" + guid;
        
        internal static void InitializeShapeShifterCore()
        {
            ShapeShifterLogger.Log("Setting up");

            ShapeShifterConfiguration.Initialise();
            RestoreMissingAssets();
        }
        
        private static void SavePendingChanges()
        {
            AssetDatabase.SaveAssets();

            // since the above doesn't seem to work with ScriptableObjects, might as well just go for a full save
            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

        public static void SaveChanges()
        {
            if (ShapeShifterConfiguration.Instance.ModifiedAssetPaths.Count > 0)
            {
                OverwriteSelectedSkin(ActiveGame);
            }
        }
    }
}