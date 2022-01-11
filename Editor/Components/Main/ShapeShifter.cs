using System;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

namespace Miniclip.ShapeShifter
{
    public partial class ShapeShifter
    {
        internal static void InitializeShapeShifterCore()
        {
            ShapeShifterLogger.Log("Setting up");

            ShapeShifterConfiguration.Initialise();
            RestoreMissingAssets();
        }
        
    }
}