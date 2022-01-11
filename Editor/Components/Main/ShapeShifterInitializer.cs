using Miniclip.ShapeShifter;
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
        ShapeShifter.InitializeShapeShifterCore();
    }

    private static void EditorApplicationOnQuitting()
    {
        EditorApplication.delayCall -= Init;
        EditorApplication.quitting -= EditorApplicationOnQuitting;
    }
}