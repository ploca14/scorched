using EasyBuildSystem.Features.Scripts.Core.Base.Addon;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Conditions;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Addon("The Forest Like", "Allows to instantiate a new preview of this piece to be able to interact with it. " +
    "This allow also to activate a new child's object to each new interaction.", AddonTarget.PieceBehaviour)]
public class AddonTheForestLike : AddonBehaviour
{
    #region Fields

    public Transform[] Elements;

    public float ChildrensLifeTime = 10f;
    public bool AddDynamicRigidbody;
    public bool AddDynamicBoxCollider;

    private bool AlreadyDetached;
    private bool IsExiting;

    private PieceBehaviour Piece;
    private GameObject Preview;
    private List<Renderer> CacheRenderers;

    #endregion

    #region Methods

    private void Awake()
    {
        BuildManager.Instance.DefaultState = StateType.Queue;
        Piece = GetComponent<PieceBehaviour>();
    }

    private void Start()
    {
        StartCoroutine(DelayedStart());
        BuildEvent.Instance.OnPieceDestroyed.AddListener(OnPieceDestroyed);
    }

    private void Update()
    {
        if (Piece.PhysicsCondition == null)
            return;

        if (Piece.PhysicsCondition.AffectedByPhysics)
        {
            if (IsExiting)
                return;

            IsExiting = true;

            if (Piece.CurrentState == StateType.Remove || Piece.CurrentState == StateType.Placed)
                DetachChildren();
        }
    }

    private void OnApplicationQuit()
    {
        IsExiting = true;
    }

    private void OnDestroy()
    {
        if (IsExiting)
            return;

        if (Piece.CurrentState == StateType.Remove || Piece.CurrentState == StateType.Placed)
            DetachChildren();
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.1f);

        if (Piece.CurrentState != StateType.Queue)
            yield break;

        if (Preview == null)
        {
            Preview = Instantiate(Piece.gameObject, transform.position, transform.rotation);

            Preview.GetComponent<PieceBehaviour>().EnableAllColliders();
            Preview.GetComponent<PieceBehaviour>().EnableAllCollidersTrigger();

            CacheRenderers = Preview.GetComponentsInChildren<Renderer>(true).ToList();

            gameObject.ChangeAllMaterialsInChildren(CacheRenderers.ToArray(), Piece.PreviewMaterial);
            Preview.ChangeAllMaterialsColorInChildren(CacheRenderers.ToArray(), Piece.PreviewAllowedColor);

            GameObject OnlyPreview = new GameObject("Preview");

            for (int i = 0; i < CacheRenderers.Count; i++)
                CacheRenderers[i].transform.SetParent(OnlyPreview.transform);

            Destroy(Preview.gameObject);

            OnlyPreview.transform.SetParent(transform);

            Preview = OnlyPreview;

            GameObject OnlyInteractionPreview = Instantiate(OnlyPreview);

            OnlyInteractionPreview.name = "Interaction Preview";

            OnlyInteractionPreview.transform.SetParent(transform, false);

            for (int i = 0; i < OnlyInteractionPreview.GetComponentsInChildren<Renderer>().Length; i++)
                Destroy(OnlyInteractionPreview.GetComponentsInChildren<Renderer>()[i]);

            OnlyInteractionPreview.SetLayerRecursively(LayerMask.GetMask("Interaction"));

            for (int i = 0; i < Elements.Length; i++)
                Elements[i].gameObject.SetActive(false);

            BuildEvent.Instance.OnPieceDestroyed.AddListener(OnPieceDestroyed);
            BuildEvent.Instance.OnPieceChangedState.AddListener(OnPieceChangedState);
        }
    }

    public void DetachChildren()
    {
        if (Piece.CurrentState == StateType.Preview) return;

        if (AlreadyDetached) return;

        AlreadyDetached = true;

        gameObject.ChangeAllMaterialsInChildren(Piece.Renderers.ToArray(), Piece.InitialRenderers);

        for (int i = 0; i < Elements.Length; i++)
        {
            if (Elements[i] == null)
                return;

            if (!Elements[i].gameObject.activeSelf)
                return;

            GameObject Temp = Instantiate(Elements[i].gameObject, Elements[i].transform.position, Elements[i].transform.rotation);

            if (AddDynamicRigidbody)
                Temp.AddRigibody(true, false, 2f);

            if (AddDynamicBoxCollider)
            {
                BoxCollider Collider = Temp.AddComponent<BoxCollider>();
                Bounds B = Temp.GetChildsBounds();

                Collider.size = B.size;
                Collider.center = B.center;
            }

            Destroy(Temp, ChildrensLifeTime);
        }

        Destroy(gameObject);
    }

    private void OnPieceChangedState(PieceBehaviour piece, StateType state)
    {
        if (piece == Piece)
        {
            if (state == StateType.Queue)
            {
                if (Preview == null)
                    return;

                gameObject.ChangeAllMaterialsInChildren(Piece.Renderers.ToArray(), Piece.InitialRenderers);
                Preview.ChangeAllMaterialsColorInChildren(CacheRenderers.ToArray(), Piece.PreviewAllowedColor);
            }
        }
    }

    private void OnPieceDestroyed(PieceBehaviour piece)
    {
        if (Piece != piece)
            return;

        Destroy(gameObject);
    }

    /// <summary>
    /// This method allows to upgrade the base part.
    /// </summary>
    public void Upgrade(string tag)
    {
        Piece.SkinIndex++;

        gameObject.ChangeAllMaterialsInChildren(Piece.Renderers.ToArray(), Piece.InitialRenderers);

        Elements.FirstOrDefault(x => !x.gameObject.activeSelf && x.tag == tag).gameObject.SetActive(true);

        PickableController.Instance.TempElements.Remove(tag);

        if (IsCompleted())
        {
            Piece.DisableAllCollidersTrigger();

            Destroy(Preview);

            for (int i = 0; i < Elements.Length; i++)
                if (Piece != null)
                    Elements[i].gameObject.SetActive(true);

            Piece.ChangeState(StateType.Placed);

            ExternalPhysicsCondition Physics = Piece.GetComponent<ExternalPhysicsCondition>();

            if (Physics != null)
                if (!Physics.CheckStability())
                    Physics.ApplyPhysics();
        }
    }

    /// <summary>
    /// This method allows to get the current upgrade progression.
    /// </summary>
    public int GetCurrentProgression()
    {
        return Piece.SkinIndex;
    }

    /// <summary>
    /// This method allows to check if the part progression is complete.
    /// </summary>
    public bool IsCompleted()
    {
        if (Piece == null)
            return false;

        return Elements.Length <= Piece.SkinIndex;
    }

    #endregion
}

#if UNITY_EDITOR

[UnityEditor.CustomEditor(typeof(AddonTheForestLike), true)]
public class AddonTheForestLikeInspector : UnityEditor.Editor
{
    public AddonTheForestLike Target;

    public void OnEnable()
    {
        Target = (AddonTheForestLike)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("AddDynamicRigidbody"), new GUIContent("Add Rigidbody At Childrens When Destroy"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("AddDynamicBoxCollider"), new GUIContent("Add BoxCollider At Childrens When Destroy"));

        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("ChildrensLifeTime"), new GUIContent("Childrens Lifetime After Destruction"));
        UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Elements.Array.size"), new GUIContent("Childrens Array Size"));
        for (int i = 0; i < serializedObject.FindProperty("Elements").arraySize; i++)
        {
            GUI.color = Color.black / 4;
            GUILayout.BeginVertical("helpBox");
            GUI.color = Color.white;
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Elements").GetArrayElementAtIndex(i));
            GUILayout.EndVertical();
        }

        if (GUILayout.Button("Get All Children's"))
        {
            Renderer[] Renderers = Target.GetComponentsInChildren<Renderer>();

            Target.Elements = new Transform[Renderers.Length];

            for (int i = 0; i < Renderers.Length; i++)
                Target.Elements[i] = Renderers[i].transform;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

#endif