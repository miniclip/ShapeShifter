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
        internal static void InitializeShapeShifterCore()
        {
            ShapeShifterLogger.Log("Setting up");

            ShapeShifterConfiguration.Initialise();
            RestoreMissingAssets();
        }
        
    }
}