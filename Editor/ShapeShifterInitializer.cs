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
        EditorApplication.delayCall -= Init;
        EditorApplication.delayCall += Init;
    }

    private static void Init()
    {
        Debug.Log("##!1 InitializeOnLoad");
        if (!ShapeShifterEditorPrefs.GetBool(ShapeShifter.IsInitializedKey)) //TODO: To be changed for a settings provider?
        {
            Debug.Log("##!2 InitializeOnLoad");

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
