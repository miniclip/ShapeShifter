using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[InitializeOnLoad]
public class WindowFocusUtility
{
    public static event Action<bool> OnUnityEditorFocusEvent;
    private static bool appFocused;
    private static bool IsApplicationActive => InternalEditorUtility.isApplicationActive;

    static WindowFocusUtility()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        if (appFocused == IsApplicationActive)
            return;

        appFocused = IsApplicationActive;
        OnUnityEditorFocusEvent?.Invoke(appFocused);
    }
}