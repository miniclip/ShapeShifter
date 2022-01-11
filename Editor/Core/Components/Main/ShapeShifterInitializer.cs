using Miniclip.ShapeShifter;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

[InitializeOnLoad]
public class ShapeShifterInitializer
{
    static ShapeShifterInitializer()
    {
        EditorApplication.delayCall += Init;
        EditorApplication.quitting += EditorApplicationOnQuitting;
    }

    private static void Init()
    {
        ShapeShifterLogger.Log("Setting up");

        ShapeShifterConfiguration.Initialise();
        AssetSwitcher.RestoreMissingAssets();
    }

    private static void EditorApplicationOnQuitting()
    {
        EditorApplication.delayCall -= Init;
        EditorApplication.quitting -= EditorApplicationOnQuitting;
    }
}