using System;

#if EBS_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

using UnityEditor;
using UnityEngine;

using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket.Data;
using EasyBuildSystem.Features.Scripts.Extensions;

#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Builder
{
    [DefaultExecutionOrder(999)]
    [AddComponentMenu("Easy Build System/Builders/Builder Behaviour")]
    public partial class BuilderBehaviour : MonoBehaviour
    {
        #region Fields

        public static BuilderBehaviour Instance;

        public RaycastType RaycastViewType;
        public Transform RaycastOriginParent;

        public LayerMask RaycastLayer = 1 << 0;

        public float RaycastTopDownSnapThreshold = 5f;

        public float RaycastActionDistance = 10f;
        public float RaycastMaxDistance = 0f;
        public Vector3 RaycastOffsetPosition = new Vector3(0, 0, 1);

        public DetectionType SocketDetectionType = DetectionType.Overlap;
        public float SocketDetectionMaxAngles = 35f;

        public virtual Ray GetRay
        {
            get
            {
                if (RaycastViewType == RaycastType.TopDown)
                {
#if EBS_NEW_INPUT_SYSTEM
                    return Camera.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f) + RaycastOffsetPosition);
#else
                    return Camera.ScreenPointToRay(Input.mousePosition + RaycastOffsetPosition);
#endif
                }
                else if (RaycastViewType == RaycastType.FirstPerson)
                {
                    return new Ray(Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f) + RaycastOffsetPosition), Camera.transform.forward);
                }
                else if (RaycastViewType == RaycastType.ThirdPerson)
                {
                    if (RaycastOriginParent != null && Camera != null)
                    {
                        return new Ray(RaycastOriginParent.position + RaycastOriginParent.TransformDirection(RaycastOffsetPosition), Camera.transform.forward);
                    }
                }
                else if (RaycastViewType == RaycastType.VirtualReality)
                {
                    return new Ray(RaycastOriginParent.transform.position, RaycastOriginParent.transform.forward);
                }

                return new Ray();
            }
        }

        private Transform _Caster;
        public virtual Transform GetCaster
        {
            get
            {
                if (RaycastViewType == RaycastType.VirtualReality)
                {
                    _Caster = RaycastOriginParent.transform;
                }
                else
                {
                    _Caster = transform;
                }

                return _Caster;
            }
        }

        public MovementType PreviewMovementType;
        public bool PreviewMovementOnlyAllowed;
        public float PreviewGridSize = 1.0f;
        public float PreviewGridOffset;
        public float PreviewSmoothTime = 5.0f;
        public bool PreviewLockRotation;

        public AudioSource Source;
        public AudioClip[] PlacementClips;
        public AudioClip[] DestructionClips;
        public AudioClip[] EditionClips;

        public BuildMode CurrentMode { get; set; }

        public BuildMode LastMode { get; set; }

        public PieceBehaviour SelectedPiece { get; set; }

        public PieceBehaviour CurrentPreview { get; set; }
        public PieceBehaviour CurrentEditionPreview { get; set; }
        public PieceBehaviour CurrentRemovePreview { get; set; }

        public bool AllowPlacement { get; set; }
        public bool AllowDestruction { get; set; }
        public bool AllowEdition { get; set; }

        public GroupBehaviour NearestGroup { get; set; }

        public bool HasSocket { get; set; }

        public SocketBehaviour CurrentSocket { get; set; }
        public SocketBehaviour LastSocket { get; set; }

        private Camera Camera;

        private Vector3 LastAllowedPoint;

        private readonly RaycastHit[] Hits = new RaycastHit[PhysicExtension.MAX_ALLOC_COUNT];

        public Vector3 CurrentPreviewRotationOffset { get; set; }
        public Vector3 CurrentPreviewInitialScale { get; set; }

        #endregion Fields

        #region Methods

        public virtual void Awake()
        {
            Instance = this;
        }

        public virtual void Start()
        {
            if (Camera == null)
            {
                Camera = GetComponent<Camera>();

                if (Camera == null)
                    Debug.LogWarning("<b>Easy Build System</b> : Builder Behaviour component need to be attached on a camera!");
            }
        }

        public virtual void Update()
        {
            if (BuildManager.Instance == null)
                return;

            if (CurrentMode == BuildMode.Placement)
            {
                if (SelectedPiece == null)
                    return;

                if (CurrentPreview == null)
                {
                    CreatePreview(SelectedPiece.gameObject);
                    return;
                }
                else
                    UpdatePreview();
            }
            else if (CurrentMode == BuildMode.Destruction)
                UpdateRemovePreview();
            else if (CurrentMode == BuildMode.Edit)
                UpdateEditionPreview();
            else if (CurrentMode == BuildMode.None)
                ClearPreview();
        }

        private void Reset()
        {
            Camera = GetComponent<Camera>();
        }

        private void OnDrawGizmosSelected()
        {
            if (Camera == null)
            {
                Camera = GetComponent<Camera>();
                return;
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(GetRay.origin, GetRay.direction * RaycastActionDistance);
        }

        #region Placement

        /// <summary>
        /// This method allows to update the placement preview.
        /// </summary>
        public void UpdatePreview()
        {
            HasSocket = false;

            if (SocketDetectionType == DetectionType.Overlap)
            {
                SocketBehaviour[] NeighboursSockets =
                    BuildManager.Instance.GetAllNearestSockets(GetCaster.position, RaycastActionDistance);

                SocketBehaviour[] ApplicableSockets = new SocketBehaviour[NeighboursSockets.Length];

                for (int i = 0; i < NeighboursSockets.Length; i++)
                {
                    if (NeighboursSockets[i] == null)
                    {
                        continue;
                    }

                    foreach (SocketBehaviour Socket in NeighboursSockets)
                    {
                        if (NeighboursSockets[i].gameObject.activeInHierarchy && !Socket.IsDisabled && Socket.AllowPiece(CurrentPreview))
                        {
                            ApplicableSockets[i] = NeighboursSockets[i];
                            break;
                        }
                    }
                }

                if (ApplicableSockets.Length > 0)
                {
                    UpdateMultipleSocket(ApplicableSockets);
                }
                else
                {
                    UpdateFreeMovement();
                }
            }
            else if (SocketDetectionType == DetectionType.Raycast)
            {
                SocketBehaviour Socket = null;

                int ColliderCount = Physics.RaycastNonAlloc(GetRay, Hits, RaycastActionDistance, LayerMask.GetMask(Constants.LAYER_SOCKET));
                for (int i = 0; i < ColliderCount; i++)
                {
                    if (Hits[i].collider.GetComponent<SocketBehaviour>() != null)
                    {
                        Socket = Hits[i].collider.GetComponent<SocketBehaviour>();
                    }
                }

                if (Socket != null)
                {
                    UpdateSingleSocket(Socket);
                }
                else
                {
                    UpdateFreeMovement();
                }
            }

            CurrentPreview.IsSnapped = HasSocket;

            CurrentPreview.gameObject.ChangeAllMaterialsColorInChildren(CurrentPreview.Renderers.ToArray(),
                CheckPlacementConditions() ? CurrentPreview.PreviewAllowedColor : CurrentPreview.PreviewDeniedColor, SelectedPiece.PreviewColorLerpTime);
        }

        /// <summary>
        /// This method allows to check the internal placement conditions.
        /// </summary>
        public bool CheckPlacementConditions()
        {
            if (CurrentPreview == null)
            {
                return false;
            }

            if (RaycastMaxDistance != 0)
            {
                if (Vector3.Distance(GetCaster.position, CurrentPreview.transform.position) > RaycastActionDistance)
                {
                    return false;
                }
            }

            if (!CurrentPreview.CheckExternalPlacementConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to rotate the current preview.
        /// </summary>
        public void RotatePreview(Vector3 rotateAxis)
        {
            if (CurrentPreview == null)
                return;

            CurrentPreviewRotationOffset += rotateAxis;
        }

        /// <summary>
        /// This method allows to move the preview in free movement.
        /// </summary>
        public void UpdateFreeMovement()
        {
            if (CurrentPreview == null)
            {
                return;
            }

            if (PreviewLockRotation)
            {
                CurrentPreview.transform.rotation = Quaternion.Euler(new Vector3(0, GetCaster.eulerAngles.y, 0)) *
                    SelectedPiece.transform.localRotation * Quaternion.Euler(CurrentPreviewRotationOffset);
            }
            else
            {
                CurrentPreview.transform.rotation = Quaternion.Euler(CurrentPreviewRotationOffset);
            }

            CurrentPreview.transform.localScale = CurrentPreviewInitialScale * 1.005f;

            float Distance = RaycastMaxDistance == 0 ? RaycastActionDistance : RaycastMaxDistance;

            Physics.Raycast(GetRay, out RaycastHit Hit, Distance, RaycastLayer);

            if (Hit.collider != null)
            {
                if (Hit.collider.GetComponentInParent<GroupBehaviour>() != null)
                {
                    NearestGroup = Hit.collider.GetComponentInParent<GroupBehaviour>();
                }
                else
                {
                    NearestGroup = null;
                }

                if (CurrentPreview.RotateAccodingSurface)
                {
                    CurrentPreviewRotationOffset = Quaternion.LookRotation(Hit.normal).eulerAngles;
                }

                Vector3 TargetPoint = Hit.point + CurrentPreview.PreviewOffset;
                Vector3 NextPoint = TargetPoint;

                if (CurrentPreview.PreviewClampPosition)
                    NextPoint = MathExtension.Clamp(NextPoint, CurrentPreview.PreviewClampMinPosition, CurrentPreview.PreviewClampMaxPosition);

                if (PreviewMovementType == MovementType.Smooth)
                    NextPoint = Vector3.Lerp(CurrentPreview.transform.position, NextPoint, PreviewSmoothTime * Time.deltaTime);
                else if (PreviewMovementType == MovementType.Grid && !CurrentPreview.IgnoreGrid)
                    NextPoint = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, NextPoint);

                if (PreviewMovementOnlyAllowed)
                {
                    CurrentPreview.transform.position = NextPoint;

                    if (CurrentPreview.CheckExternalPlacementConditions() && CheckPlacementConditions())
                    {
                        LastAllowedPoint = CurrentPreview.transform.position;
                    }
                    else
                    {
                        CurrentPreview.transform.position = LastAllowedPoint;
                    }
                }
                else
                    CurrentPreview.transform.position = NextPoint;

                float Angle = Vector3.Angle(Hit.normal, Vector3.up);

                if (CurrentPreview.RotateAccordingSlope)
                {
                    if (Angle < CurrentPreview.RotateAccordingSlopeAngleLimition)
                    {
                        if (PreviewLockRotation)
                        {
                            CurrentPreview.transform.rotation = Quaternion.FromToRotation(GetCaster.up, Hit.normal) * Quaternion.Euler(CurrentPreviewRotationOffset) *
                                GetCaster.rotation * SelectedPiece.transform.localRotation * Quaternion.Euler(CurrentPreviewRotationOffset);
                        }
                        else
                        {
                            CurrentPreview.transform.rotation = Quaternion.FromToRotation(new Vector3(0, GetCaster.eulerAngles.y, 0), Hit.normal) * 
                                SelectedPiece.transform.localRotation * Quaternion.Euler(CurrentPreviewRotationOffset);
                        }
                    }
                }

                return;
            }

            Transform StartTransform = (CurrentPreview.GroundUpperHeight == 0) ? GetCaster : Camera.transform;
            Vector3 LookDistance = StartTransform.position + StartTransform.forward * Distance;

            if (CurrentPreview.UseGroundUpper)
            {
                LookDistance.y = Mathf.Clamp(LookDistance.y, CurrentPreview.GroundUpperHeight,
                    GetCaster.position.y + CurrentPreview.GroundUpperHeight);
            }
            else
            {
                if (Physics.Raycast(CurrentPreview.transform.position + new Vector3(0, 0.3f, 0),
                        Vector3.down, out RaycastHit HitLook, Mathf.Infinity, RaycastLayer, QueryTriggerInteraction.Ignore))
                {
                    LookDistance.y = HitLook.point.y;
                }
            }

            CurrentPreview.transform.position = LookDistance;

            if (PreviewMovementType == MovementType.Grid && !CurrentPreview.IgnoreGrid)
            {
                CurrentPreview.transform.position = MathExtension.PositionToGridPosition(PreviewGridSize, PreviewGridOffset, LookDistance + CurrentPreview.PreviewOffset);
            }
            else if (PreviewMovementType == MovementType.Smooth)
            {
                CurrentPreview.transform.position = Vector3.Lerp(CurrentPreview.transform.position, LookDistance, PreviewSmoothTime * Time.deltaTime);
            }

            CurrentSocket = null;
            LastSocket = null;
            HasSocket = false;
        }

        /// <summary>
        /// This method allows to move the preview only on available socket(s).
        /// </summary>
        public void UpdateMultipleSocket(SocketBehaviour[] sockets)
        {
            if (CurrentPreview == null || sockets == null)
            {
                return;
            }

            float closestAngle = Mathf.Infinity;

            CurrentSocket = null;

            RaycastHit raycastTopDown = new RaycastHit();

            if (RaycastViewType == RaycastType.TopDown)
                Physics.Raycast(GetRay, out raycastTopDown, Mathf.Infinity, LayerMask.GetMask(Constants.LAYER_SOCKET), QueryTriggerInteraction.Ignore);

            foreach (SocketBehaviour Socket in sockets)
            {
                if (Socket != null)
                {
                    if (Socket.gameObject.activeSelf && !Socket.IsDisabled)
                    {
                        if (Socket.AllowPiece(CurrentPreview) && !CurrentPreview.IgnoreSocket)
                        {
                            if ((Socket.transform.position - (RaycastViewType != RaycastType.TopDown ? GetCaster.position : raycastTopDown.point)).sqrMagnitude <
                                Mathf.Pow(RaycastViewType != RaycastType.TopDown ? RaycastActionDistance : RaycastTopDownSnapThreshold, 2))
                            {
                                float angle = Vector3.Angle(GetRay.direction, Socket.transform.position - GetRay.origin);

                                if (angle < closestAngle && angle < SocketDetectionMaxAngles)
                                {
                                    closestAngle = angle;

                                    if (RaycastViewType != RaycastType.TopDown && CurrentSocket == null)
                                    {
                                        CurrentSocket = Socket;
                                    }
                                    else
                                    {
                                        CurrentSocket = Socket;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (CurrentSocket != null)
            {
                Offset OffsetPiece = CurrentSocket.GetOffset(CurrentPreview);

                if (OffsetPiece != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(OffsetPiece.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation *
                        (CurrentPreview.RotateOnSockets ? Quaternion.Euler(OffsetPiece.Rotation + CurrentPreviewRotationOffset) : Quaternion.Euler(OffsetPiece.Rotation));

                    if (OffsetPiece.Scale != Vector3.one)
                    {
                        CurrentPreview.transform.localScale = OffsetPiece.Scale * 1.005f;
                    }
                    else
                    {
                        CurrentPreview.transform.localScale = CurrentSocket.transform.parent != null ? CurrentSocket.transform.parent.localScale * 1.005f : transform.localScale * 1.005f;
                    }

                    HasSocket = true;

                    if (!CheckPlacementConditions())
                    {
                        HasSocket = false;
                    }
                    else
                    {
                        LastSocket = CurrentSocket;
                        HasSocket = true;
                        return;
                    }
                }
            }
            else
                CurrentPreview.transform.localScale = CurrentPreviewInitialScale;

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to move the preview only on available socket.
        /// </summary>
        public void UpdateSingleSocket(SocketBehaviour socket)
        {
            if (CurrentPreview == null || socket == null)
            {
                return;
            }

            CurrentSocket = null;

            if (socket != null)
            {
                if (socket.gameObject.activeSelf && !socket.IsDisabled)
                {
                    if (socket.AllowPiece(CurrentPreview) && !CurrentPreview.IgnoreSocket)
                    {
                        CurrentSocket = socket;
                    }
                }
            }

            if (CurrentSocket != null)
            {
                Offset Offset = CurrentSocket.GetOffset(CurrentPreview);

                if (Offset != null)
                {
                    CurrentPreview.transform.position = CurrentSocket.transform.position + CurrentSocket.transform.TransformVector(Offset.Position);

                    CurrentPreview.transform.rotation = CurrentSocket.transform.rotation *
                        (CurrentPreview.RotateOnSockets ? Quaternion.Euler(Offset.Rotation + CurrentPreviewRotationOffset) : Quaternion.Euler(Offset.Rotation));

                    if (Offset.Scale != Vector3.one)
                    {
                        CurrentPreview.transform.localScale = Offset.Scale * 1.005f;
                    }

                    HasSocket = true;

                    if (!CheckPlacementConditions())
                    {
                        HasSocket = false;
                    }
                    else
                    {
                        LastSocket = CurrentSocket;
                        HasSocket = true;
                        return;
                    }
                }
            }

            UpdateFreeMovement();
        }

        /// <summary>
        /// This method allows to place the current preview.
        /// </summary>
        public virtual void PlacePrefab(GroupBehaviour group = null)
        {       
            AllowPlacement = CheckPlacementConditions();

            if (!AllowPlacement)
            {
                return;
            }

            if (CurrentEditionPreview != null)
            {
                Destroy(CurrentEditionPreview.gameObject);
            }

            BuildManager.Instance.PlacePrefab(SelectedPiece,
                CurrentPreview.transform.position,
                CurrentPreview.transform.eulerAngles,
                CurrentPreview.transform.localScale / 1.005f,
                group,
                CurrentSocket);

            if (Source != null)
            {
                if (PlacementClips.Length != 0)
                {
                    Source.PlayOneShot(PlacementClips[UnityEngine.Random.Range(0, PlacementClips.Length)]);
                }
            }

            CurrentSocket = null;
            LastSocket = null;
            AllowPlacement = false;
            HasSocket = false;

            if (CurrentPreview != null)
            {
                if (!CurrentPreview.KeepLastRotation)
                    CurrentPreviewRotationOffset = Vector3.zero;

                Destroy(CurrentPreview.gameObject);
            }
        }

        /// <summary>
        /// This method allows to create a preview.
        /// </summary>
        public virtual PieceBehaviour CreatePreview(GameObject prefab)
        {
            if (prefab == null)
                return null;

            CurrentPreview = Instantiate(prefab).GetComponent<PieceBehaviour>();

            if (!CurrentPreview.KeepLastRotation)
            {
                CurrentPreview.transform.eulerAngles = Vector3.zero;
                CurrentPreviewRotationOffset = Vector3.zero;
            }

            CurrentPreviewInitialScale = CurrentPreview.transform.localScale;

            if (Physics.Raycast(GetRay, out RaycastHit Hit, Mathf.Infinity, RaycastLayer, QueryTriggerInteraction.Ignore))
            {
                CurrentPreview.transform.position = Hit.point;
            }

            CurrentPreview.ChangeState(StateType.Preview);

            SelectedPiece = prefab.GetComponent<PieceBehaviour>();

            BuildEvent.Instance.OnPieceInstantiated.Invoke(CurrentPreview, null);

            CurrentSocket = null;

            LastSocket = null;

            AllowPlacement = false;

            HasSocket = false;

            return CurrentPreview;
        }

        /// <summary>
        /// This method allows to clear the current preview.
        /// </summary>
        public virtual void ClearPreview()
        {
            if (CurrentPreview == null)
            {
                return;
            }

            BuildEvent.Instance.OnPieceDestroyed.Invoke(CurrentPreview);

            Destroy(CurrentPreview.gameObject);

            AllowPlacement = false;

            CurrentPreview = null;
        }

        #endregion Placement

        #region Destruction

        /// <summary>
        /// This method allows to update the destruction preview.
        /// </summary>
        public void UpdateRemovePreview()
        {
            float Distance = RaycastMaxDistance == 0 ? RaycastActionDistance : RaycastMaxDistance;

            if (CurrentRemovePreview != null)
            {
                if (CurrentRemovePreview.CurrentState != StateType.Remove)
                    CurrentRemovePreview.ChangeState(StateType.Remove);

                AllowPlacement = false;
            }

            if (Physics.Raycast(GetRay, out RaycastHit Hit, Distance, RaycastLayer))
            {
                PieceBehaviour Part = Hit.collider.GetComponentInParent<PieceBehaviour>();

                if (Part != null)
                {
                    if (CurrentRemovePreview != null)
                    {
                        if (CurrentRemovePreview.GetInstanceID() != Part.GetInstanceID())
                        {
                            ClearRemovePreview();

                            CurrentRemovePreview = Part;
                        }
                    }
                    else
                    {
                        CurrentRemovePreview = Part;
                    }
                }
                else
                {
                    ClearRemovePreview();
                }
            }
            else
            {
                ClearRemovePreview();
            }
        }

        /// <summary>
        /// This method allows to check the internal destruction conditions.
        /// </summary>
        public bool CheckDestructionConditions()
        {
            if (CurrentRemovePreview == null)
            {
                return false;
            }

            if (!CurrentRemovePreview.CheckExternalDestructionConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to remove the current preview.
        /// </summary>
        public virtual void DestroyPrefab()
        {
            AllowDestruction = CheckDestructionConditions();

            if (!AllowDestruction)
            {
                return;
            }

            BuildManager.Instance.DestroyPrefab(CurrentRemovePreview);

            if (Source != null)
            {
                if (DestructionClips.Length != 0)
                {
                    Source.PlayOneShot(DestructionClips[UnityEngine.Random.Range(0, DestructionClips.Length)]);
                }
            }

            CurrentSocket = null;
            LastSocket = null;
            AllowDestruction = false;
            HasSocket = false;
        }

        /// <summary>
        /// This method allows to clear the current remove preview.
        /// </summary>
        public virtual void ClearRemovePreview()
        {
            if (CurrentRemovePreview == null)
            {
                return;
            }

            CurrentRemovePreview.ChangeState(CurrentRemovePreview.LastState);

            AllowDestruction = false;

            CurrentRemovePreview = null;
        }

        #endregion Destruction

        #region Edit

        /// <summary>
        /// This method allows to update the edition mode.
        /// </summary>
        public void UpdateEditionPreview()
        {
            AllowEdition = CurrentEditionPreview;

            if (CurrentEditionPreview != null && AllowEdition)
            {
                if (CurrentEditionPreview.CurrentState != StateType.Edit)
                    CurrentEditionPreview.ChangeState(StateType.Edit);
            }

            float Distance = RaycastMaxDistance == 0 ? RaycastActionDistance : RaycastMaxDistance;

            if (Physics.Raycast(GetRay, out RaycastHit Hit, Distance, RaycastLayer))
            {
                PieceBehaviour Piece = Hit.collider.GetComponentInParent<PieceBehaviour>();

                if (Piece != null)
                {
                    if (CurrentEditionPreview != null)
                    {
                        if (CurrentEditionPreview.GetInstanceID() != Piece.GetInstanceID())
                        {
                            ClearEditPreview();

                            CurrentEditionPreview = Piece;
                        }
                    }
                    else
                    {
                        CurrentEditionPreview = Piece;
                    }
                }
                else
                {
                    ClearEditPreview();
                }
            }
            else
            {
                ClearEditPreview();
            }
        }

        /// <summary>
        /// This method allows to check the internal edition conditions.
        /// </summary>
        public bool CheckEditionConditions()
        {
            if (CurrentEditionPreview == null)
            {
                return false;
            }

            if (!CurrentEditionPreview.CheckExternalEditionConditions())
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// This method allows to edit the current preview.
        /// </summary>
        public virtual void EditPrefab()
        {
            AllowEdition = CheckEditionConditions();

            if (!AllowEdition)
                return;

            PieceBehaviour EditingPiece = CurrentEditionPreview;

            EditingPiece.ChangeState(StateType.Edit);

            SelectPrefab(EditingPiece);

            SelectedPiece.SkinIndex = EditingPiece.SkinIndex;

            ChangeMode(BuildMode.Placement);
        }

        /// <summary>
        /// This method allows to clear the current edition preview.
        /// </summary>
        public void ClearEditPreview()
        {
            if (CurrentEditionPreview == null)
            {
                return;
            }

            CurrentEditionPreview.ChangeState(CurrentEditionPreview.LastState);

            AllowEdition = false;

            CurrentEditionPreview = null;
        }

        #endregion

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(BuildMode mode)
        {
            if (CurrentMode == mode)
            {
                return;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                ClearPreview();
            }

            if (CurrentMode == BuildMode.Destruction)
            {
                ClearRemovePreview();
            }

            if (mode == BuildMode.None)
            {
                ClearPreview();
                ClearRemovePreview();
                ClearEditPreview();
            }

            LastMode = CurrentMode;

            CurrentMode = mode;

            BuildEvent.Instance.OnChangedBuildMode.Invoke(CurrentMode);
        }

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(string modeName)
        {
            if (CurrentMode.ToString() == modeName)
            {
                return;
            }

            if (CurrentMode == BuildMode.Placement)
            {
                ClearPreview();
            }

            if (CurrentMode == BuildMode.Destruction)
            {
                ClearRemovePreview();
            }

            if (modeName == BuildMode.None.ToString())
            {
                ClearPreview();
                ClearRemovePreview();
                ClearEditPreview();
            }

            LastMode = CurrentMode;

            CurrentMode = (BuildMode)Enum.Parse(typeof(BuildMode), modeName);

            BuildEvent.Instance.OnChangedBuildMode.Invoke(CurrentMode);
        }

        /// <summary>
        /// This method allows to select a prefab.
        /// </summary>
        public void SelectPrefab(PieceBehaviour prefab)
        {
            if (prefab == null)
            {
                return;
            }

            SelectedPiece = BuildManager.Instance.GetPieceById(prefab.Id);
        }

        #endregion Methods
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BuilderBehaviour), true)]
    public class BuilderBehaviourEditor : Editor
    {
        #region Fields

        private BuilderBehaviour Target { get { return (BuilderBehaviour)target; } }
        private static bool[] FoldoutArray = new bool[4];

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            AddonInspector.LoadAddons(Target, AddonTarget.BuilderBehaviour);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Builder Behaviour - Component");

            GUILayout.Label("Manages the behaviour of all the build modes (placement, destruction, and editing).\n" +
                "Find more information about this component in the documentation.", EditorStyles.miniLabel);

            EditorGUILayout.Space(1);

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastViewType"),
                    new GUIContent("Raycast View Type :", "Raycast view type.\n" +
                    "First Person : Raycast origin come from camera center to forward.\n" +
                    "Third Person : Raycast origin come from custom transform to forward.\n" +
                    "Top Down : Raycast origin come from camera center to mouse position.\n" +
                    "Virtual Reality : Raycast origin come from custom transform to forward."));

                if (((RaycastType)serializedObject.FindProperty("RaycastViewType").enumValueIndex) == RaycastType.ThirdPerson)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastOriginParent"),
                        new GUIContent("Raycast Third Person Origin Parent :", "Custom transform on which the raycast will start."));
                else if (((RaycastType)serializedObject.FindProperty("RaycastViewType").enumValueIndex) == RaycastType.TopDown)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastTopDownSnapThreshold"),
                        new GUIContent("Raycast Top Down Snap Threshold :", "Max threshold to snap a piece on closest socket."));
                else if (((RaycastType)serializedObject.FindProperty("RaycastViewType").enumValueIndex) == RaycastType.VirtualReality)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastOriginParent"),
                        new GUIContent("Raycast Virtual Reality Origin Parent :", "Custom transform on which the raycast will start."));

                if (Target.RaycastLayer.value != 1)
                {
                    EditorGUILayout.HelpBox("If you are set your own layer here, make you sure to add also this layer on all your pieces.\n" +
                        "Otherwise, the destruction and edit mode will ignoring these pieces.", MessageType.Warning);
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastLayer"),
                    new GUIContent("Raycast Layer Mask :", "Raycast layers to place, destroy and edited the pieces."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastActionDistance"), 
                    new GUIContent("Raycast Action Distance :", "Action distance on which the preview can will be moved and placed (if possible)."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastMaxDistance"),
                    new GUIContent("Raycast Max Distance :", "Max distance on which the preview can will be moved but cannot be placed."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RaycastOffsetPosition"),
                    new GUIContent("Raycast Offset Position :", "Raycast offset position, useful to adjusting the position of origin raycast."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SocketDetectionType"),
                    new GUIContent("Socket Detection Type :", "Define the ray of detection.\nIt is recommended to use the following types :\nRaycast: if you've sockets with the type Attachment.\nOverlap Sphere: if you've sockets with the type (Point)."));

                if (serializedObject.FindProperty("SocketDetectionType").enumValueIndex == (int)DetectionType.Overlap)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SocketDetectionMaxAngles"),
                        new GUIContent("Socket Detection Max Angles :", "Define the maximum angles to detect the sockets."));
            }

            #endregion

            #region Preview

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Preview Settings", true);

            if (FoldoutArray[1])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewMovementType"), 
                    new GUIContent("Preview Movement Type :", "Preview movement type."));

                if ((MovementType)serializedObject.FindProperty("PreviewMovementType").enumValueIndex == MovementType.Smooth)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewSmoothTime"), 
                        new GUIContent("Preview Movement Smooth Time :", "Preview movement smooth time."));

                if ((MovementType)serializedObject.FindProperty("PreviewMovementType").enumValueIndex == MovementType.Grid)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewGridSize"), 
                        new GUIContent("Preview Grid Size :", "Grid size on which the preview will be moved."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewGridOffset"),
                        new GUIContent("Preview Grid Offset :", "Grid offset for the preview position."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewMovementOnlyAllowed"),
                    new GUIContent("Preview Movement Only Allowed :", "Move the preview only if the placement is possible."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewLockRotation"), 
                    new GUIContent("Preview Movement Lock Rotation :", "Lock the preview with the camera rotation."));
            }

            #endregion

            #region Audios

            FoldoutArray[2] = EditorGUILayout.Foldout(FoldoutArray[2], "Audios Settings", true);

            if (FoldoutArray[2])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Source"),
                    new GUIContent("Audio Source :", "Audio source on which the audio clips will be played."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PlacementClips"),
                    new GUIContent("Audio Placement Sounds :", "Placement clips at play when a preview is placed (Randomly played)."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DestructionClips"), 
                    new GUIContent("Audio Destruction Sounds :", "Destruction clips at play when a piece is destroyed (Randomly played)."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("EditionClips"),
                    new GUIContent("Audio Edition Sounds :", "Edition clips at play when a piece is destroyed (Randomly played)."), true);
                GUILayout.EndHorizontal();
            }

            #endregion

            #region Add-ons

            FoldoutArray[3] = EditorGUILayout.Foldout(FoldoutArray[3], "Add-ons Settings", true);

            if (FoldoutArray[3])
            {
                AddonInspector.DrawAddons(Target, AddonTarget.BuilderBehaviour);
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
#endif
}