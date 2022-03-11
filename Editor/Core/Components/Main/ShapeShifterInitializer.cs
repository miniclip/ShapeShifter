using System.Threading.Tasks;
using Miniclip.ShapeShifter;
using Miniclip.ShapeShifter.Switcher;
using Miniclip.ShapeShifter.Utils;
using UnityEditor;

[InitializeOnLoad]
public class ShapeShifterInitializer
{
    static ShapeShifterInitializer()
    {
        EditorApplication.delayCall += OnDelayedCall;
        EditorApplication.quitting += EditorApplicationOnQuitting;
        WindowFocusUtility.OnUnityEditorFocus -= OnEditorFocus;
        WindowFocusUtility.OnUnityEditorFocus += OnEditorFocus;
    }

    private static void OnEditorFocus(bool isFocused)
    {
        RestoreAssetsAfterCompiling(isFocused);
    }

    private static async void RestoreAssetsAfterCompiling(bool isFocused)
    {
        if (!isFocused)
            return;

        if (!ShapeShifterConfiguration.IsInitialized())
            return;

        await Task.Delay(1000);

        while (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            await Task.Delay(1000);
        }

        if (isFocused && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            AssetSwitcher.RestoreMissingAssets();
        }
    }

    private static async void OnDelayedCall()
    {
        await Init();
    }

    public static async Task Init()
    {
        while (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            await Task.Delay(1000);
        }

        ShapeShifterLogger.Log("Setting up");

        ShapeShifterConfiguration.Initialise();
        AssetSwitcher.RestoreMissingAssets();
    }

    private static void EditorApplicationOnQuitting()
    {
        EditorApplication.delayCall -= OnDelayedCall;
        EditorApplication.quitting -= EditorApplicationOnQuitting;
        WindowFocusUtility.OnUnityEditorFocus -= OnEditorFocus;
    }
}