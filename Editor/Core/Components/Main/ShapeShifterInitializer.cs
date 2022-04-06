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
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        EditorApplication.delayCall += OnDelayedCall;
        EditorApplication.quitting += EditorApplicationOnQuitting;
        WindowFocusUtility.OnUnityEditorFocusEvent -= OnEditorFocus;
        WindowFocusUtility.OnUnityEditorFocusEvent += OnEditorFocus;
    }

    private static void OnEditorFocus(bool isFocused)
    {
        //RestoreAssetsAfterCompiling(isFocused);
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
            // AssetSwitcher.RestoreMissingAssets();
        }
    }

    private static async void OnDelayedCall()
    {
        await Init();
    }

    public static async Task Init(bool restoreMissingAssets = false)
    {
        while (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            await Task.Delay(1000);
        }
        
        ShapeShifterConfiguration.Initialise();

        if (restoreMissingAssets)
        {
            //TODO: Revisit after save improvements.
            //AssetSwitcher.RestoreMissingAssets();
        }
    }

    private static void EditorApplicationOnQuitting()
    {
        EditorApplication.delayCall -= OnDelayedCall;
        EditorApplication.quitting -= EditorApplicationOnQuitting;
        WindowFocusUtility.OnUnityEditorFocusEvent -= OnEditorFocus;
    }
}