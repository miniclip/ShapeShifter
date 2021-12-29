using Miniclip.ShapeShifter;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class ShapeShifterInitializer : MonoBehaviour
{
    static ShapeShifterInitializer()
    {
        if (ShapeShifterEditorPrefs.GetBool(ShapeShifter.IsInitializedKey)) //TODO: To be changed for a settings provider?
        {
            ShapeShifter.InitializeShapeShifterCore();
            ShapeShifter.RestoreMissingAssets();
        }
    }
}
