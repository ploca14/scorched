using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket.Data;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif
using EasyBuildSystem.Features.Scripts.Extensions;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Socket
{
    [AddComponentMenu("Easy Build System/Components/Socket Behaviour")]
    public class SocketBehaviour : MonoBehaviour
    {
        #region Fields

        public float Radius = 0.25f;

        public bool ShowRadius;

        public List<Offset> PartOffsets = new List<Offset>();

        public PieceBehaviour ParentPiece;

        private Collider _CachedCollider;
        public Collider CachedCollider
        {
            get
            {
                if (_CachedCollider == null)
                    _CachedCollider = GetComponent<Collider>();

                return _CachedCollider;
            }
        }

        public bool IsDisabled;

        public static bool ShowGizmos = true;

        #endregion Fields

        #region Methods

        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_SOCKET);
            ParentPiece = GetComponentInParent<PieceBehaviour>();
            gameObject.AddSphereCollider(Radius);
        }

        private void Start()
        {
            BuildManager.Instance.AddSocket(this);
        }

        private void OnDestroy()
        {
            BuildManager.Instance.RemoveSocket(this);
        }

        public void OnDrawGizmos()
        {
            if (!ShowGizmos)
                return;

            if (IsDisabled)
            {
                return;
            }

            Gizmos.DrawWireCube(transform.position, Vector3.one / 6);
            Gizmos.DrawWireSphere(transform.position, 1f * Radius);
        }

        /// <summary>
        /// This method allows to disable the collider of the socket.
        /// </summary>
        public void DisableSocketCollider()
        {
            IsDisabled = true;

            if (CachedCollider != null)
            {
                CachedCollider.gameObject.layer = Physics.IgnoreRaycastLayer;
            }
        }

        /// <summary>
        /// This method allows to enable the collider of the socket.
        /// </summary>
        public void EnableSocketCollider()
        {
            IsDisabled = false;

            if (CachedCollider != null)
            {
                CachedCollider.gameObject.layer = LayerMask.NameToLayer(Constants.LAYER_SOCKET);
            }
        }

        /// <summary>
        /// This method allows to check if the piece is contains in the offset list of this socket.
        /// </summary>
        public bool AllowPiece(PieceBehaviour piece)
        {
            if (piece == null) return false;

            for (int i = 0; i < PartOffsets.Count; i++)
            {
                if (PartOffsets[i] != null && PartOffsets[i].Piece != null)
                {
                    if (PartOffsets[i].AllowSameCategory)
                    {
                        if (PartOffsets[i].Piece.Category == piece.Category)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (PartOffsets[i].Piece.Id == piece.Id)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// This method allows to get the piece offset wich allowed on this socket.
        /// </summary>
        public Offset GetOffset(PieceBehaviour piece)
        {
            for (int i = 0; i < PartOffsets.Count; i++)
            {
                if (PartOffsets[i].AllowSameCategory)
                {
                    if (PartOffsets[i].Piece.Category == piece.Category)
                    {
                        return PartOffsets[i];
                    }
                }
                else
                {
                    if (PartOffsets[i].Piece != null && PartOffsets[i].Piece.Id == piece.Id)
                    {
                        return PartOffsets[i];
                    }
                }
            }

            return null;
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(SocketBehaviour))]
    [CanEditMultipleObjects]
    public class SocketBehaviourInspector : Editor
    {
        #region Fields

        private SocketBehaviour Target { get { return (SocketBehaviour)target; } }
        private static bool[] FoldoutArray = new bool[5];

        private Offset CurrentOffset;
        private GameObject PreviewPiece;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            AddonInspector.LoadAddons(Target, AddonTarget.SocketBehaviour);
            ConditionInspector.LoadConditions(Target, ConditionTarget.SocketBehaviour);
        }

        private void OnDisable()
        {
            ClearPreview();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Socket Behaviour - Component");

            GUILayout.Label("Create a socket to snapping of pieces at an world point (position, rotation, scale).\n" +
                "Find more information about this component on the documentation.", EditorStyles.miniLabel);

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Radius"),
                    new GUIContent("Socket Radius :", "Socket radius point.\nYou can decrease the socket radius to improve the precision during the detection."));
            }

            #endregion

            #region Offsets

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Offsets Settings", true);

            if (FoldoutArray[1])
            {
                if (serializedObject.FindProperty("PartOffsets").arraySize == 0)
                {
                    GUI.color = Color.black / 4;
                    GUILayout.BeginVertical("helpBox");
                    GUI.color = Color.white;
                    GUILayout.Label("Offset list does not contains any transform child(s).");
                    GUILayout.EndVertical();
                }
                else
                {
                    int Index = 0;

                    foreach (Offset Offset in Target.PartOffsets.ToList())
                    {
                        if (Offset == null)
                        {
                            return;
                        }

                        if (CurrentOffset == Offset)
                            GUI.color = Color.yellow;
                        else
                            GUI.color = Color.white;

                        GUI.color = Color.black / 4;
                        GUILayout.BeginVertical("helpBox");
                        GUI.color = Color.white;

                        GUI.color = Color.white;

                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Piece"),
                            new GUIContent("Offset Piece :", "Piece which can be snap on this socket."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Position"),
                            new GUIContent("Offset Piece Position :", "Position of the preview when snapped on this socket."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Rotation"),
                            new GUIContent("Offset Piece Rotation :", "Rotation of the preview when snapped on this socket."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("Scale"),
                            new GUIContent("Offset Piece Scale :", "Scale of the preview when snapped on this socket."));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("PartOffsets").GetArrayElementAtIndex(Index).FindPropertyRelative("AllowSameCategory"), 
                            new GUIContent("Offset Piece Same Category :", "If checked, all the pieces of the same category can will be snapped."));

                        if (PreviewPiece != null && PreviewPiece.name == Offset.Piece.Id)
                        {
                            GUI.color = Color.yellow;
                            if (GUILayout.Button("Cancel Preview"))
                            {
                                ClearPreview();
                            }
                            GUI.color = Color.white;
                        }
                        else
                        {
                            if (GUILayout.Button("Show Preview"))
                            {
                                ClearPreview();
                                CurrentOffset = Offset;
                                CreatePreview(Offset);
                            }
                        }

                        if (GUILayout.Button("Remove Offset"))
                        {
                            Undo.RecordObject(target, "Cancel remove offset");

                            Target.PartOffsets.Remove(Offset);

                            ClearPreview();

                            EditorUtility.SetDirty(target);

                            return;
                        }

                        GUILayout.EndVertical();

                        Index++;
                    }
                }

                GUI.color = Color.black / 4;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                Rect DropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));

                GUI.Box(DropRect, "Drag & Drop your pieces here to add them in the list.", EditorStyles.centeredGreyMiniLabel);

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

                            if (!PrefabUtility.IsPartOfPrefabAsset(DraggedObject))
                            {
                                DraggedObject = PrefabUtility.GetCorrespondingObjectFromSource(DraggedObject);

                                if (DraggedObject == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> : Object have not PieceBehaviour component or the prefab is not the original.");
                                    return;
                                }
                            }

                            PieceBehaviour DraggedPiece = DraggedObject.GetComponent<PieceBehaviour>();

                            if (DraggedPiece == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : You object have not PieceBehaviour component!");
                                return;
                            }

                            if (Target.PartOffsets.Find(entry => entry.Piece.Id == DraggedPiece.Id) == null)
                            {
                                ClearPreview();
                                Offset Offset = new Offset(DraggedPiece);
                                Target.PartOffsets.Insert(Target.PartOffsets.Count, Offset);
                                Target.PartOffsets = Target.PartOffsets.OrderBy(x => i).ToList();
                                CurrentOffset = Offset;
                                CreatePreview(Offset);
                                Repaint();
                                EditorUtility.SetDirty(target);
                            }
                            else
                            {
                                Debug.LogError("<b>Easy Build System</b> : This piece is already exists in the list.");
                            }
                        }

                        UnityEngine.Event.current.Use();
                    }
                }

                GUILayout.EndVertical();
            }

            #endregion

            #region Conditions

            FoldoutArray[2] = EditorGUILayout.Foldout(FoldoutArray[2], "Conditions Settings", true);

            if (FoldoutArray[2])
            {
                ConditionInspector.DrawConditions(Target, ConditionTarget.SocketBehaviour);
            }

            #endregion

            #region Add-Ons

            FoldoutArray[3] = EditorGUILayout.Foldout(FoldoutArray[3], "Add-ons Settings", true);

            if (FoldoutArray[3])
            {
                AddonInspector.DrawAddons(Target, AddonTarget.SocketBehaviour);
            }

            #endregion

            #region Debugging

            FoldoutArray[4] = EditorGUILayout.Foldout(FoldoutArray[4], "Debugging Settings", true);

            if (FoldoutArray[4])
            {
                SocketBehaviour.ShowGizmos = EditorGUILayout.Toggle("Socket Show Gizmos", SocketBehaviour.ShowGizmos);
                GUI.enabled = false;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsDisabled"), new GUIContent("Socket Is Disabled :", "Socket is disabled?, when disable all the raycast will ignore this."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ParentPiece"), new GUIContent("Socket Parent Piece :", "Parent piece if the socket has a parent with the Piece Behaviour component."));
                GUI.enabled = true;
            }

            GUI.color = Color.white;

            #endregion

            serializedObject.ApplyModifiedProperties();

            if (CurrentOffset != null)
            {
                if (PreviewPiece != null)
                {
                    PreviewPiece.transform.position = Target.transform.TransformPoint(CurrentOffset.Position);
                    PreviewPiece.transform.rotation = Target.transform.rotation * Quaternion.Euler(CurrentOffset.Rotation);

                    if (CurrentOffset.Scale != Vector3.one)
                    {
                        PreviewPiece.transform.localScale = CurrentOffset.Scale;
                    }
                    else
                        PreviewPiece.transform.localScale = Target.transform.parent != null ? Target.transform.parent.localScale : Target.transform.localScale;
                }
            }
        }

        private void CreatePreview(Offset offsetPiece)
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (PreviewPiece == null)
            {
                PieceBehaviour PieceInstance = offsetPiece.Piece;

                if (PieceInstance == null) return;

                PreviewPiece = Instantiate(PieceInstance.gameObject, Target.transform);

                PreviewPiece.transform.position = Target.transform.TransformPoint(offsetPiece.Position);
                PreviewPiece.transform.rotation = Target.transform.rotation * Quaternion.Euler(offsetPiece.Rotation);

                if (offsetPiece.Scale != Vector3.one)
                {
                    PreviewPiece.transform.localScale = offsetPiece.Scale;
                }
                else
                    PreviewPiece.transform.localScale = Target.transform.parent != null ? Target.transform.parent.localScale : Target.transform.localScale;

                PreviewPiece.name = PieceInstance.Id.ToString();

                DestroyImmediate(PreviewPiece.GetComponent<PieceBehaviour>());

                foreach (SocketBehaviour Socket in PreviewPiece.GetComponentsInChildren<SocketBehaviour>())
                {
                    DestroyImmediate(Socket);
                }

                Material PreviewMaterial = new Material(Resources.Load<Material>("Materials/Default Transparent"));

                if (GraphicsSettings.currentRenderPipeline)
                {
                    if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                        PreviewMaterial = Resources.Load<Material>("Materials/HDRP Default Transparent");
                    else
                        PreviewMaterial = Resources.Load<Material>("Materials/URP Default Transparent");
                }

                PreviewMaterial.SetColor("_BaseColor", new Color(0, 1f, 1f, 0.5f));

                PreviewPiece.ChangeAllMaterialsInChildren(PreviewPiece.GetComponentsInChildren<MeshRenderer>(), PreviewMaterial);

                SceneView.FrameLastActiveSceneView();
            }
        }

        private void ClearPreview()
        {
            if (PreviewPiece != null)
            {
                DestroyImmediate(PreviewPiece);
                PreviewPiece = null;
                CurrentOffset = null;
            }
        }

        #endregion Methods
    }

#endif
}