using Miniclip.ShapeShifter;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[InitializeOnLoad]
public class ShapeShifterInitializer
{
    static ShapeShifterInitializer()
    {
        Debug.Log("##! InitializeOnLoad");
        if (ShapeShifterEditorPrefs.GetBool(ShapeShifter.IsInitializedKey)) //TODO: To be changed for a settings provider?
        {
            ShapeShifter.InitializeShapeShifterCore();
            ShapeShifter.RestoreMissingAssets();
        }
    }

    [DidReloadScripts]
    static void ScriptsReloaded()
    {
        Debug.Log("##! Scripts Reloaded");
    }
}
