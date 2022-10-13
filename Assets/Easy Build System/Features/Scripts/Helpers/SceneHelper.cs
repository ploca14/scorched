#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class SceneHelper
{
    #region Methods

    public static void Focus(Object target, bool autoSelect = true)
    {
        EditorWindow.GetWindow<SceneView>("", typeof(SceneView));

        if (autoSelect)
        {
            Selection.activeObject = target;
        }

        if (SceneView.lastActiveSceneView != null)
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }
    }

    #endregion
}
#endif