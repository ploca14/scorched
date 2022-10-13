using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Conditions;
using EasyBuildSystem.Features.Scripts.Extensions;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Piece
{
    [AddComponentMenu("Easy Build System/Components/Piece Behaviour")]
    public class PieceBehaviour : MonoBehaviour
    {
        #region Fields

        public string Id;
        public Sprite Icon;
        public string Name = "New Piece";
        public string Description = "";
        public string Category = "Default";

        public bool IgnoreSocket;
        public bool IgnoreGrid;

        public bool UseGroundUpper;
        public float GroundUpperHeight = 1f;

        public bool KeepLastRotation = false;
        public bool RotateOnSockets = true;
        public bool RotateAccodingSurface = false;
        public bool RotateAccordingSlope;
        public float RotateAccordingSlopeAngleLimition;
        public Vector3 RotationAxis = Vector3.up * 90;

        public bool PreviewClampPosition = false;
        public Vector3 PreviewClampMinPosition;
        public Vector3 PreviewClampMaxPosition;

        public Vector3 PreviewOffset = new Vector3(0, 0, 0);
        public GameObject[] PreviewDisableObjects;
        public MonoBehaviour[] PreviewDisableBehaviours;
        public Collider[] PreviewDisableColliders;

        public float PreviewColorLerpTime = 0f;

        public Material CustomPreviewMaterial;

        public Material PreviewMaterial { get; set; }

        public Color PreviewAllowedColor = new Color(0.0f, 1.0f, 0, 0.5f);
        public Color PreviewDeniedColor = new Color(1.0f, 0, 0, 0.5f);

        public List<ConditionBehaviour> Conditions = new List<ConditionBehaviour>();

        public List<GameObject> Skins;
        public int SkinIndex = 0;

        public Bounds MeshBounds;
        public Bounds MeshBoundsToWorld { get { return transform.ConvertBoundsToWorld(MeshBounds); } }

        public StateType CurrentState;
        public StateType LastState;

        public GroupBehaviour Group => GetComponentInParent<GroupBehaviour>();

        public SocketBehaviour[] Sockets;

        public Dictionary<Renderer, Material[]> InitialRenderers = new Dictionary<Renderer, Material[]>();
        public List<Collider> Colliders = new List<Collider>();
        public List<Renderer> Renderers = new List<Renderer>();

        public List<string> ExtraProperties;

        public int EntityInstanceId { get; set; }

        private ExternalGeneralCondition generalCondition;
        public ExternalGeneralCondition GeneralCondition 
        {
            get
            {
                if (generalCondition == null) 
                    generalCondition = GetComponent<ExternalGeneralCondition>(); 
                
                return generalCondition; 
            }

            set { generalCondition = value; }
        }

        private ExternalCollisionCondition collisionCondition;
        public ExternalCollisionCondition CollisionCondition
        {
            get
            {
                if (collisionCondition == null)
                    collisionCondition = GetComponent<ExternalCollisionCondition>();

                return collisionCondition;
            }

            set { collisionCondition = value; }
        }

        private ExternalPhysicsCondition physicsCondition;
        public ExternalPhysicsCondition PhysicsCondition
        {
            get
            {
                if (physicsCondition == null)
                    physicsCondition = GetComponent<ExternalPhysicsCondition>();

                return physicsCondition;
            }

            set { physicsCondition = value; }
        }

        public bool IsSnapped { get; set; }

        public Material DefaultPreviewMaterial { get; set; }

        public static bool ShowGizmos = true;

        #endregion Fields

        #region Methods

        private void Awake()
        {
            Conditions.AddRange(GetComponents<ConditionBehaviour>());

            Sockets = GetComponentsInChildren<SocketBehaviour>();

            Renderers = GetComponentsInChildren<Renderer>(true).ToList();

            for (int i = 0; i < Renderers.Count; i++)
                InitialRenderers.Add(Renderers[i], Renderers[i].sharedMaterials);

            Colliders = GetComponentsInChildren<Collider>(true).ToList();

            for (int i = 0; i < Colliders.Count; i++)
                if (Colliders[i] != Colliders[i])
                    Physics.IgnoreCollision(Colliders[i], Colliders[i]);

            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                    DefaultPreviewMaterial = Resources.Load<Material>("Materials/HDRP Default Transparent");
                else
                    DefaultPreviewMaterial = Resources.Load<Material>("Materials/URP Default Transparent");
            }
            else
            {
                DefaultPreviewMaterial = new Material(Resources.Load<Material>("Materials/Default Transparent"));
            }

            DefaultPreviewMaterial.SetColor("_BaseColor", Color.clear);

            if (CustomPreviewMaterial == null)
            {
                PreviewMaterial = new Material(DefaultPreviewMaterial);
            }
            else
            {
                PreviewMaterial = new Material(CustomPreviewMaterial);
            }
        }

        private void Start()
        {
            if (CurrentState != StateType.Preview)
            {
                BuildManager.Instance.AddPiece(this);
            }
        }

        private void Reset()
        {
            if (gameObject.GetComponent<ExternalGeneralCondition>() == null)
                gameObject.AddComponent<ExternalGeneralCondition>();

            if (MeshBounds.size == Vector3.zero)
                MeshBounds = gameObject.GetChildsBounds();
        }

        private void Update()
        {
            bool isPlaced = CurrentState == StateType.Placed;

            if (!isPlaced)
            {
                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(isPlaced);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = isPlaced;
                    }
                }

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = isPlaced;
                    }
                }

                return;
            }

            for (int i = 0; i < Skins.Count; i++)
            {
                if (Skins[i] == null)
                {
                    return;
                }

                if (i == SkinIndex)
                {
                    Skins[i].SetActive(true);
                }
                else
                {
                    Skins[i].SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (CurrentState == StateType.Preview)
            {
                return;
            }

            if (Group != null)
                Group.RemovePiece(this);

            BuildEvent.Instance.OnPieceDestroyed.Invoke(this);

            BuildManager.Instance.RemovePiece(this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position, Vector3.one * 0.1f);
            Gizmos.color = Color.cyan / 2;
            Gizmos.DrawCube(transform.position, Vector3.one * 0.1f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(MeshBounds.center, MeshBounds.size * 1.001f);
        }

        /// <summary>
        /// This method allows to change the piece state (Queue, Preview, Edit, Remove, Placed).
        /// </summary>
        public void ChangeState(StateType state)
        {
            if (BuilderBehaviour.Instance == null)
            {
                return;
            }

            if (state == StateType.Queue)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(), PreviewAllowedColor);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].EnableSocketCollider();
                    Sockets[i].gameObject.SetActive(true);
                }
            }
            else if (state == StateType.Preview)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(),
                    BuilderBehaviour.Instance.AllowPlacement ? PreviewAllowedColor : PreviewDeniedColor);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                DisableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].DisableSocketCollider();
                    Sockets[i].gameObject.SetActive(false);
                }
            }
            else if (state == StateType.Edit)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(),
                    BuilderBehaviour.Instance.AllowEdition ? PreviewAllowedColor : PreviewDeniedColor);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].EnableSocketCollider();
                    Sockets[i].gameObject.SetActive(true);
                }
            }
            else if (state == StateType.Remove)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), PreviewMaterial);
                gameObject.ChangeAllMaterialsColorInChildren(Renderers.ToArray(), PreviewDeniedColor);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(false);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = false;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = false;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].DisableSocketCollider();
                    Sockets[i].gameObject.SetActive(false);
                }
            }
            else if (state == StateType.Placed)
            {
                gameObject.ChangeAllMaterialsInChildren(Renderers.ToArray(), InitialRenderers);

                for (int i = 0; i < PreviewDisableObjects.Length; i++)
                {
                    if (PreviewDisableObjects[i])
                    {
                        PreviewDisableObjects[i].SetActive(true);
                    }
                }

                for (int i = 0; i < PreviewDisableBehaviours.Length; i++)
                {
                    if (PreviewDisableBehaviours[i])
                    {
                        PreviewDisableBehaviours[i].enabled = true;
                    }
                }

                EnableAllColliders();

                for (int i = 0; i < PreviewDisableColliders.Length; i++)
                {
                    if (PreviewDisableColliders[i])
                    {
                        PreviewDisableColliders[i].enabled = true;
                    }
                }

                for (int i = 0; i < Sockets.Length; i++)
                {
                    Sockets[i].EnableSocketCollider();
                    Sockets[i].gameObject.SetActive(true);
                }
            }

            LastState = CurrentState;
            CurrentState = state;

            BuildEvent.Instance.OnPieceChangedState.Invoke(this, state);
        }

        /// <summary>
        /// This method allows to enable all the colliders of this piece.
        /// </summary>
        public void EnableAllColliders()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].enabled = true;
            }
        }

        /// <summary>
        /// This method allows to disable all the colliders of this piece.
        /// </summary>
        public void DisableAllColliders()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].enabled = false;
            }
        }

        /// <summary>
        /// This method allows to enable all the colliders of this piece.
        /// </summary>
        public void EnableAllCollidersTrigger()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].isTrigger = true;
            }
        }

        /// <summary>
        /// This method allows to disable all the colliders of this piece.
        /// </summary>
        public void DisableAllCollidersTrigger()
        {
            for (int i = 0; i < Colliders.Count; i++)
            {
                Colliders[i].isTrigger = false;
            }
        }

        /// <summary>
        /// This method allows check all the external condition(s) before placement.
        /// </summary>
        public bool CheckExternalPlacementConditions()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckForPlacement())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This method allows check all the external condition(s) before destruction.
        /// </summary>
        public bool CheckExternalDestructionConditions()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckForDestruction())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This method allow check all the external condition(s) before edition.
        /// </summary>
        public bool CheckExternalEditionConditions()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (!Conditions[i].CheckForEdit())
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// This method allows to change the piece appearance.
        /// </summary>
        public void ChangeSkin(int skinIndex)
        {
            if (SkinIndex == skinIndex)
            {
                return;
            }

            if (Skins.Count < skinIndex)
            {
                return;
            }

            SkinIndex = skinIndex;

            if (BuildEvent.Instance != null)
                BuildEvent.Instance.OnPieceChangedSkin.Invoke(this, skinIndex);
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PieceBehaviour), true)]
    [CanEditMultipleObjects]
    public class PieceBehaviourInspector : Editor
    {
        #region Fields

        private PieceBehaviour Target { get { return (PieceBehaviour)target; } }
        private static bool[] FoldoutArray = new bool[7];

        private static bool[] SkinsFoldout = new bool[999];
        private readonly List<Editor> SkinsPreview = new List<Editor>();
        private readonly List<Editor> CachedEditors = new List<Editor>();

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            AddonInspector.LoadAddons(Target, AddonTarget.PieceBehaviour);
            ConditionInspector.LoadConditions(Target, ConditionTarget.PieceBehaviour);
        }

        private void OnSceneGUI()
        {
            if (SceneView.lastActiveSceneView.camera == null)
            {
                return;
            }

            if (Target.UseGroundUpper)
            {
                Handles.color = Color.green;
                Handles.DrawLine(Target.transform.position, Target.transform.position + Vector3.down * Target.GroundUpperHeight);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Piece Behaviour - Component");

            GUILayout.Label("Contains all the settings such as preview, skins, bounds, conditions settings.\n" +
                "Find more information about this component on the documentation.", EditorStyles.miniLabel);

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Id"),
                    new GUIContent("Piece Identifier :", "Unique identifier of the piece."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"),
                    new GUIContent("Piece Icon :", "Icon of the piece."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Name"), 
                    new GUIContent("Piece Name :", "Name of the piece."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"),
                    new GUIContent("Piece Description :", "Description of the piece."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Category"),
                    new GUIContent("Piece Category :", "Category of the piece."));
            }

            #endregion

            #region Preview

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Preview Settings", true);

            if (FoldoutArray[1])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IgnoreSocket"), 
                    new GUIContent("Preview Ignore Sockets :", "If the preview ignore the sockets."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("IgnoreGrid"),
                    new GUIContent("Preview Ignore Grid :", "If the preview ignore the grid position."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseGroundUpper"),
                    new GUIContent("Preview Ground Upper :", "Allows to raise the preview from ground on the Y axis."));

                if (serializedObject.FindProperty("UseGroundUpper").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("GroundUpperHeight"),
                        new GUIContent("Preview Ground Upper Height :", "Height limit not to exceed on the Y axis."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("KeepLastRotation"),
                    new GUIContent("Preview Keep Last Rotation :", "Keep the last rotation of the preview."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateOnSockets"),
                    new GUIContent("Preview Can Rotate On Socket :", "If the preview can be rotated on socket."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateAccodingSurface"),
                    new GUIContent("Preview Rotate According Surface :", "Allows to rotate the preview according to raycast hitted surface."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateAccordingSlope"), 
                    new GUIContent("Preview Rotate According Slope :", "Allows to rotate the preview according to collider slope."));

                if (serializedObject.FindProperty("RotateAccordingSlope").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RotateAccordingSlopeAngleLimition"),
                        new GUIContent("Preview Slope Max Angle :", "Limit the max rotation angle on slopes."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RotationAxis"),
                    new GUIContent("Preview Rotate Axis :", "Rotation axis on which the preview will be rotated."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewOffset"), 
                    new GUIContent("Preview Position Offset :", "Preview offset position."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewClampPosition"),
                    new GUIContent("Preview Clamp Position :", "Allows to clamp the preview position."));

                if (serializedObject.FindProperty("PreviewClampPosition").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewClampMinPosition"),
                        new GUIContent("Preview Clamp Min Position :", "Preview clamp min position."));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewClampMaxPosition"),
                        new GUIContent("Preview Clamp Max Position :", "Preview clamp max position."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewColorLerpTime"),
                    new GUIContent("Preview Color Lerp Time :", "Preview color lerp time, 0 for instant."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CustomPreviewMaterial"), 
                    new GUIContent("Preview Custom Material :", "If the preview use a custom material, can be null."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewAllowedColor"), 
                    new GUIContent("Preview Allowed Color :", "Show the allowed color when the preview can be placed."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDeniedColor"), 
                    new GUIContent("Preview Denied Color :", "Show the denied color when the preview can't be placed."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableObjects"),
                    new GUIContent("Disable Objects In Preview State :", "Disable some objects when the piece is in preview state."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableBehaviours"),
                    new GUIContent("Disable Mono Behaviours In Preview State :", "Disable some monobehaviours when the piece is in preview/queue/remove/edit state."), true);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PreviewDisableColliders"),
                    new GUIContent("Disable Colliders In Preview State :", "Disable some colliders when the piece is in preview state."), true);
                GUILayout.EndHorizontal();
            }

            #endregion

            #region Skins

            FoldoutArray[2] = EditorGUILayout.Foldout(FoldoutArray[2], "Skins Settings", true);

            if (FoldoutArray[2])
            {
                bool Flag = false;

                if (SkinsFoldout == null)
                {
                    SkinsFoldout = new bool[serializedObject.FindProperty("Skins").arraySize];
                }

                for (int i = 0; i < serializedObject.FindProperty("Skins").arraySize; i++)
                {
                    if (Target.Skins[i] == null)
                    {
                        Flag = true;
                    }
                }

                if (Flag)
                {
                    Target.Skins.Clear();
                }

                int Index = 0;

                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                if (serializedObject.FindProperty("Skins").arraySize == 0)
                {
                    GUILayout.Label("Skins list does not contains any transform child(s).");
                }
                else
                {
                    foreach (GameObject Skin in Target.Skins)
                    {
                        if (Skin == null)
                        {
                            return;
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Space(13);

                        EditorGUI.BeginChangeCheck();

                        string Format = string.Format("[{0}] ", Index) + Skin.name;

                        GUILayout.BeginHorizontal();

                        SkinsFoldout[Index] = EditorGUILayout.Foldout(SkinsFoldout[Index], Format, true);

                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Remove", GUILayout.Width(80)))
                        {
                            Undo.RecordObject(target, "Remove Appearance");
                            Target.Skins.Remove(Skin);
                            Repaint();
                            EditorUtility.SetDirty(target);
                            SkinsPreview.Clear();
                            break;
                        }

                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (SkinsFoldout[Index] == true)
                            {
                                for (int i = 0; i < SkinsFoldout.Length; i++)
                                {
                                    if (i != Index)
                                    {
                                        SkinsFoldout[i] = false;
                                    }
                                }

                                for (int x = 0; x < Target.Skins.Count; x++)
                                {
                                    if (x == Index)
                                    {
                                        Target.Skins[x].SetActive(true);
                                    }
                                    else
                                    {
                                        Target.Skins[x].SetActive(false);
                                    }
                                }
                            }
                            else
                            {
                                for (int x = 0; x < Target.Skins.Count; x++)
                                {
                                    if (x == Target.SkinIndex)
                                    {
                                        Target.Skins[x].SetActive(true);
                                    }
                                    else
                                    {
                                        Target.Skins[x].SetActive(false);
                                    }
                                }
                            }

                            SceneHelper.Focus(Skin, false);

                            SkinsPreview.Clear();
                        }

                        if (Target.SkinIndex == Index)
                        {
                            GUI.enabled = false;
                        }

                        if (GUILayout.Button("Define As Default"))
                        {
                            for (int i = 0; i < SkinsFoldout.Length; i++)
                            {
                                SkinsFoldout[i] = false;
                            }

                            Target.ChangeSkin(Index);

                            for (int x = 0; x < Target.Skins.Count; x++)
                            {
                                if (x == Target.SkinIndex)
                                {
                                    Target.Skins[x].SetActive(true);
                                }
                                else
                                {
                                    Target.Skins[x].SetActive(false);
                                }
                            }

                            Repaint();

                            EditorUtility.SetDirty(target);
                        }

                        GUI.enabled = true;

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();

                        if (SkinsFoldout[Index])
                        {
                            GUILayout.BeginHorizontal();

                            GUI.color = Color.black / 4f;
                            GUILayout.BeginVertical("helpBox");
                            GUI.color = Color.white;

                            if (Skin != null)
                            {
                                GUILayout.BeginHorizontal();

                                UnityEditor.Editor PreviewEditor = null;

                                if (SkinsPreview.Count > Index)
                                {
                                    PreviewEditor = SkinsPreview[Index];
                                }

                                if (PreviewEditor == null)
                                {
                                    PreviewEditor = CreateEditor(Skin);

                                    SkinsPreview.Add(PreviewEditor);

                                    PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.textArea);

                                    CachedEditors.Add(PreviewEditor);
                                }
                                else
                                {
                                    PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128), EditorStyles.textArea);
                                }

                                GUILayout.EndHorizontal();

                                EditorGUILayout.ObjectField(serializedObject.FindProperty("Skins").GetArrayElementAtIndex(Index), new GUIContent("Child Transform :"));
                            }

                            GUILayout.FlexibleSpace();

                            GUILayout.EndVertical();

                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndHorizontal();

                        Index++;
                    }
                }

                EditorGUILayout.EndVertical();

                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                Rect DropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));

                GUI.Box(DropRect, "Drag & Drop your transform children(s) here to add them in the list.", EditorStyles.centeredGreyMiniLabel);

                if (DropRect.Contains(UnityEngine.Event.current.mousePosition))
                {
                    if (UnityEngine.Event.current.type == EventType.DragUpdated)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        UnityEngine.Event.current.Use();
                    }
                    else if (UnityEngine.Event.current.type == EventType.DragPerform)
                    {
                        for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                        {
                            GameObject DraggedObject = DragAndDrop.objectReferences[i] as GameObject;

                            if (DraggedObject == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : Cannot add empty child!");
                                return;
                            }

                            if (!DraggedObject.transform.IsChildOf(Target.transform))
                            {
                                Debug.LogError("<b>Easy Build System</b> : This child does not exist in this transform!");
                                return;
                            }

                            if (Target.Skins.Contains(DraggedObject) == false)
                            {
                                Target.Skins.Add(DraggedObject);

                                for (int x = 0; x < Target.Skins.Count; x++)
                                {
                                    if (x == Target.SkinIndex)
                                    {
                                        Target.Skins[x].SetActive(true);
                                    }
                                    else
                                    {
                                        Target.Skins[x].SetActive(false);
                                    }
                                }

                                EditorUtility.SetDirty(target);

                                Repaint();
                            }
                            else
                            {
                                Debug.LogError("<b>Easy Build System</b> : This child already exists in the list!");
                            }
                        }
                        UnityEngine.Event.current.Use();
                    }
                }

                GUILayout.EndVertical();
            }

            #endregion

            #region Bounds

            FoldoutArray[3] = EditorGUILayout.Foldout(FoldoutArray[3], "Bounds Settings", true);

            if (FoldoutArray[3])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("MeshBounds"),
                    new GUIContent("Piece Mesh Bounds :", "Mesh bounds of the piece used to detect the collisions."));

                if (GUILayout.Button("Generate Bounds"))
                {
                    Undo.RecordObject(target, "Cancel new bounds generation");
                    Target.MeshBounds = Target.gameObject.GetChildsBounds();
                    EditorUtility.SetDirty(target);
                }
            }

            #endregion

            #region Conditions

            FoldoutArray[4] = EditorGUILayout.Foldout(FoldoutArray[4], "Conditions Settings", true);

            if (FoldoutArray[4])
            {
                ConditionInspector.DrawConditions(Target, ConditionTarget.PieceBehaviour);
            }

            #endregion

            #region Addons

            FoldoutArray[5] = EditorGUILayout.Foldout(FoldoutArray[5], "Add-ons Settings", true);

            if (FoldoutArray[5])
            {
                AddonInspector.DrawAddons(Target, AddonTarget.PieceBehaviour);
            }

            #endregion

            #region Debugging

            FoldoutArray[6] = EditorGUILayout.Foldout(FoldoutArray[6], "Debugging Settings", true);

            if (FoldoutArray[6])
            {
                PieceBehaviour.ShowGizmos = EditorGUILayout.Toggle("Piece Show Gizmos", PieceBehaviour.ShowGizmos);
                GUI.enabled = false;
                EditorGUILayout.Toggle("Piece Has Group :", Target.Group != null);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentState"),
                    new GUIContent("Piece Current State", "Current state of the piece."));
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Sockets"),
                    new GUIContent("Piece Sockets :", "All the sockets that the piece contains"), true);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Colliders"), 
                    new GUIContent("Piece Colliders :", "All colliders of the piece."), true);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Renderers"),
                    new GUIContent("Piece Renderers :", "All renderers of the piece."), true);
                GUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            #endregion

            if (GUILayout.Button("Create New Socket..."))
            {
                GameObject Child = new GameObject("New Socket Behaviour");
                Child.transform.SetParent(Selection.activeGameObject.transform, false);
                Child.transform.position = Selection.activeGameObject.transform.position;
                Child.AddComponent<SocketBehaviour>();
                SceneHelper.Focus(Child);
            }

            if (GUILayout.Button("Add To Build Manager..."))
            {
                BuildManager.Instance.Pieces.Add(Target);
                Debug.Log("<b>Easy Build System</b> : " + Target.Name + " has been added to Build Manager.");
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }

#endif
}