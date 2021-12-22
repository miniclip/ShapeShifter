using Miniclip.ShapeShifter;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ShapeShifterInitializer : MonoBehaviour
{
    private static bool initialized = false;
    static ShapeShifterInitializer()
    {
        if (initialized)
            return;

        ShapeShifter.InitializeShapeShifterCore();
    }
}
