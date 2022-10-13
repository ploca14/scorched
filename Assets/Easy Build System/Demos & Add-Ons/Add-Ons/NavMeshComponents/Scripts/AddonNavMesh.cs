using EasyBuildSystem.Features.Scripts.Core.Base.Addon;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage;
using UnityEngine;
using UnityEngine.AI;

[Addon("NavMeshComponents Add-On", "Update the NavMeshSurface components to each action (placement, destruction, edit).\n" +
    "You can find more information about this component in the documentation.", AddonTarget.BuildManager)]
public class AddonNavMesh : AddonBehaviour
{
    #region Public Fields

    public static AddonNavMesh Instance;

    #endregion

    #region Private Fields

    public NavMeshSurface[] Surfaces;

    #endregion

    #region Private Methods

    private void OnEnable()
    {
        if (BuildEvent.Instance == null) return;

        if (FindObjectOfType<BuildStorage>() != null && FindObjectOfType<BuildStorage>().ExistsStorageFile())
            BuildEvent.Instance.OnStorageLoadingResult.AddListener(OnStorageLoadingDone);

        BuildEvent.Instance.OnPieceInstantiated.AddListener(OnPlacedPart);
        BuildEvent.Instance.OnPieceDestroyed.AddListener(OnDestroyedPart);
    }

    private void OnDisable()
    {
        BuildEvent.Instance.OnPieceInstantiated.RemoveListener(OnPlacedPart);
        BuildEvent.Instance.OnPieceDestroyed.RemoveListener(OnDestroyedPart);
    }

    private void Awake()
    {
        UpdateMeshData();

        Instance = this;

        if (Surfaces == null)
            Debug.LogWarning("AddonNavMesh: Please complete empty field to use NavMeshSurface component.");
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < Surfaces.Length; i++)
            Surfaces[i].BuildNavMesh();
    }

    private void OnStorageLoadingDone(PieceBehaviour[] pieces)
    {
        if (pieces == null) return;

        UpdateMeshData();

        BuildEvent.Instance.OnPieceInstantiated.AddListener(OnPlacedPart);
        BuildEvent.Instance.OnPieceDestroyed.AddListener(OnDestroyedPart);
    }

    private void OnPlacedPart(PieceBehaviour piece, SocketBehaviour socket)
    {
        if (piece.CurrentState != EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums.StateType.Placed) return;

        UpdateMeshData();
    }

    private void OnDestroyedPart(PieceBehaviour piece)
    {
        UpdateMeshData();
    }

    #endregion

    #region Public Methods

    public void UpdateMeshData()
    {
        for (int i = 0; i < Surfaces.Length; i++)
            Surfaces[i].UpdateNavMesh(Surfaces[i].navMeshData);
    }

    #endregion
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(AddonNavMesh), true)]
public class AddonNavMeshInspector : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Surfaces.Array.size"), new GUIContent("NavMesh Surface Array Size"));
        for (int i = 0; i < serializedObject.FindProperty("Surfaces").arraySize; i++)
        {
            GUI.color = Color.black / 4;
            GUILayout.BeginVertical("helpBox");
            GUI.color = Color.white;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Surfaces").GetArrayElementAtIndex(i));
            GUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif