using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Conditions;
using EasyBuildSystem.Features.Scripts.Extensions;
using UnityEngine;

public class Demo_DestructionEffect : MonoBehaviour
{
    #region Public Fields

    public bool AutomaticallyDefineChildrens;
    public Transform[] ManuallyDefinedChildrens;
    public float ChildrensLifeTime = 10f;
    public bool AddDynamicRigidbody;
    public float MaxDepenetrationVelocity = 2f;
    public bool AddDynamicBoxCollider;

    private PieceBehaviour Piece;
    private bool AlreadyDetached;
    private bool IsExiting;

    #endregion

    #region Private Methods

    private void Awake()
    {
        Piece = GetComponent<PieceBehaviour>();

        if (AutomaticallyDefineChildrens)
        {
            Renderer[] Renderers = GetComponentsInChildren<Renderer>();

            ManuallyDefinedChildrens = new Transform[Renderers.Length];

            for (int i = 0; i < Renderers.Length; i++)
                ManuallyDefinedChildrens[i] = Renderers[i].transform;
        }
    }

    private void Start()
    {
        BuildEvent.Instance.OnPieceDestroyed.AddListener(OnPieceDestroyed);
    }

    private void Update()
    {
        if (Piece.GetComponent<ExternalPhysicsCondition>() == null) return;

        if (Piece.GetComponent<ExternalPhysicsCondition>().AffectedByPhysics)
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

    private void OnPieceDestroyed(PieceBehaviour piece)
    {
        if (piece != Piece)
            return;

        if (Piece.GetComponent<ExternalPhysicsCondition>() == null) return;

        if (!Piece.GetComponent<ExternalPhysicsCondition>().CheckStability())
            DetachChildren();
    }

    public void DetachChildren()
    {
        if (Piece.CurrentState == StateType.Preview) return;

        if (AlreadyDetached) return;

        AlreadyDetached = true;

        gameObject.ChangeAllMaterialsInChildren(Piece.Renderers.ToArray(), Piece.InitialRenderers);

        for (int i = 0; i < ManuallyDefinedChildrens.Length; i++)
        {
            if (ManuallyDefinedChildrens[i] == null)
                return;

            if (!ManuallyDefinedChildrens[i].gameObject.activeSelf)
                return;

            GameObject Temp = Instantiate(ManuallyDefinedChildrens[i].gameObject, ManuallyDefinedChildrens[i].transform.position, ManuallyDefinedChildrens[i].transform.rotation);

            if (AddDynamicRigidbody)
                Temp.AddRigibody(true, false, MaxDepenetrationVelocity);

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

    #endregion
}
