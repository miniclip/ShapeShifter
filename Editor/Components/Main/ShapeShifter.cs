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
        
        internal static void InitializeShapeShifterCore()
        {
            ShapeShifterLogger.Log("Setting up");

            ShapeShifterConfiguration.Initialise();
            RestoreMissingAssets();
        }
        
    }
}