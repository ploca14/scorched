using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Area;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager.Surface;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Scriptables.Blueprint;
using EasyBuildSystem.Features.Scripts.Core.Scriptables.Collection;
using EasyBuildSystem.Features.Scripts.Extensions;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Manager
{
    [ExecuteInEditMode, DefaultExecutionOrder(-998)]
    [AddComponentMenu("Easy Build System/Build Manager")]
    [RequireComponent(typeof(BuildEvent))]
    public class BuildManager : MonoBehaviour
    {
        #region Fields

        public static BuildManager Instance;

        public SupportType[] BuildableSurfaces = new SupportType[1] { SupportType.AnyCollider };

        public bool DynamicBatching;

        public StateType DefaultState = StateType.Placed;

        public BlueprintTemplate[] Blueprints;

        public List<PieceBehaviour> Pieces;

        public List<PieceBehaviour> CachedParts;
        public List<SocketBehaviour> CachedSockets;
        public List<AreaBehaviour> CachedAreas;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Instance = this;
        }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// This method allows to add a piece from the manager cache.
        /// </summary>
        public void AddPiece(PieceBehaviour piece)
        {
            if (piece == null)
            {
                return;
            }

            CachedParts.Add(piece);
        }

        /// <summary>
        /// This method allows to remove a piece from the manager cache.
        /// </summary>
        public void RemovePiece(PieceBehaviour piece)
        {
            if (piece == null)
            {
                return;
            }

            CachedParts.Remove(piece);
        }

        /// <summary>
        /// This method allows to add a socket from the manager cache.
        /// </summary>
        public void AddSocket(SocketBehaviour socket)
        {
            if (socket == null)
            {
                return;
            }

            CachedSockets.Add(socket);
        }

        /// <summary>
        /// This method allows to remove a socket from the manager cache.
        /// </summary>
        public void RemoveSocket(SocketBehaviour socket)
        {
            if (socket == null)
            {
                return;
            }

            CachedSockets.Remove(socket);
        }

        /// <summary>
        /// This method allows to add a area from the manager cache.
        /// </summary>
        public void AddArea(AreaBehaviour area)
        {
            if (area == null)
            {
                return;
            }

            CachedAreas.Add(area);
        }

        /// <summary>
        /// This method allows to remove a area from the manager cache.
        /// </summary>
        public void RemoveArea(AreaBehaviour area)
        {
            if (area == null)
            {
                return;
            }

            CachedAreas.Remove(area);
        }

        /// <summary>
        /// This method allows to get a prefab by id.
        /// </summary>
        public PieceBehaviour GetPieceById(string id)
        {
            return Pieces.Find(entry => entry.Id == id);
        }

        /// <summary>
        /// This method allows to get a prefab by name.
        /// </summary>
        public PieceBehaviour GetPieceByName(string name)
        {
            return Pieces.Find(entry => entry.Name == name);
        }

        /// <summary>
        /// This method allows to get a prefab by category.
        /// </summary>
        public PieceBehaviour GetPieceByCategory(string category)
        {
            return Pieces.Find(entry => entry.Category == category);
        }

        /// <summary>
        /// This method allows to get all the nearest sockets from point according radius.
        /// </summary>
        public SocketBehaviour[] GetAllNearestSockets(Vector3 point, float radius)
        {
            List<SocketBehaviour> Result = new List<SocketBehaviour>();

            for (int i = 0; i < CachedSockets.Count; i++)
            {
                if (CachedSockets[i] != null)
                {
                    if (Vector3.Distance(CachedSockets[i].transform.position, point) < radius)
                    {
                        Result.Add(CachedSockets[i]);
                    }
                }
            }

            return Result.ToArray();
        }

        /// <summary>
        /// This method allows to place a piece.
        /// </summary>
        public PieceBehaviour PlacePrefab(PieceBehaviour piece, Vector3 position, Vector3 rotation, Vector3 scale, GroupBehaviour group = null, SocketBehaviour socket = null, bool createGroup = true)
        {
            GameObject PlacedObject = Instantiate(piece.gameObject, position, Quaternion.Euler(rotation));

            PlacedObject.transform.localScale = scale;

            PieceBehaviour InstantiatedPiece = PlacedObject.GetComponent<PieceBehaviour>();

            if (group == null)
            {
                if (socket != null)
                {
                    if (socket.ParentPiece != null && socket.ParentPiece.Group != null)
                    {
                        socket.ParentPiece.Group.AddPiece(InstantiatedPiece);
                        PlacedObject.transform.SetParent(socket.ParentPiece.transform.parent, true);
                    }
                }
                else
                {
                    if (createGroup)
                    {
                        GroupBehaviour InstancedGroup = new GameObject("Group (" + InstantiatedPiece.GetInstanceID() + ")").AddComponent<GroupBehaviour>();
                        
                        InstancedGroup.AddPiece(InstantiatedPiece);

                        if (BuildEvent.Instance != null)
                            BuildEvent.Instance.OnGroupInstantiated.Invoke(InstancedGroup);
                    }
                }
            }
            else
            {
                group.AddPiece(InstantiatedPiece);

                if (BuildEvent.Instance != null)
                    BuildEvent.Instance.OnGroupUpdated.Invoke(group);
            }

            InstantiatedPiece.ChangeState(DefaultState);

            if (BuildEvent.Instance != null)
                BuildEvent.Instance.OnPieceInstantiated.Invoke(InstantiatedPiece, socket);

            return InstantiatedPiece;
        }

        /// <summary>
        /// This method allows to destroy a piece.
        /// </summary>
        public void DestroyPrefab(PieceBehaviour piece)
        {
            Destroy(piece.gameObject);
        }

        /// <summary>
        /// This method allows to check if the collider is a buildable surface.
        /// </summary>
        public bool IsBuildableSurface(Collider collider)
        {
            if (BuildableSurfaces.Contains(SupportType.AnyCollider))
                return true;

            for (int i = 0; i < BuildableSurfaces.Length; i++)
            {
                if (collider.GetComponent<SurfaceCollider>())
                {
                    return BuildableSurfaces.Contains(SupportType.SurfaceCollider);
                }

                if (collider.GetComponent<TerrainCollider>())
                {
                    return BuildableSurfaces.Contains(SupportType.TerrainCollider);
                }
            }

            return false;
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(BuildManager))]
    public class BuildManagerInspector : Editor
    {
        #region Fields

        private BuildManager Target { get { return (BuildManager)target; } }
        private static bool[] FoldoutArray = new bool[3];

        private readonly List<Editor> PiecePreviews = new List<Editor>();
        private bool[] PieceFoldout = new bool[999];

        private PieceCollection[] Pieces;
        private string[] Options;
        private int Selection;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Pieces = ListExtension.FindAssetsByType<PieceCollection>().ToArray();

            Options = new string[Pieces.Length + 1];
            Options[0] = "Select Collection...";
            for (int i = 0; i < Pieces.Length; i++)
            {
                Options[i + 1] = Pieces[i].name;
            }

            PieceFoldout = new bool[999];

            AddonInspector.LoadAddons(Target, AddonTarget.BuildManager);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Build Manager - Component");

            GUILayout.Label("Manages all the components relative to the system and also contains the list of pieces.\n" +
                "Find more information about this component in the documentation.", EditorStyles.miniLabel);

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DynamicBatching"),
                    new GUIContent("(Experimental) Dynamic Batching :", "(Experimental) Optimizes more +80% the Group Behaviours which containing of large structure."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("BuildableSurfaces"),
                    new GUIContent("Buildable Surfaces :", "Define a type of surface on which the conditions with the field (Require Buildable Surface) will can be placed."));
                GUILayout.EndHorizontal();
            }

            #endregion

            #region Pieces

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Pieces Settings", true);

            if (FoldoutArray[1])
            {
                bool Flag = false;

                if (PieceFoldout == null)
                {
                    PieceFoldout = new bool[Target.Pieces.Count];
                }

                if (Target.Pieces != null)
                {
                    for (int i = 0; i < Target.Pieces.Count; i++)
                    {
                        if (Target.Pieces[i] == null)
                        {
                            Flag = true;
                        }
                    }
                }

                if (Flag)
                {
                    Target.Pieces = Target.Pieces.Where(s => s != null).Distinct().ToList();
                }

                int Index = 0;

                GUI.color = Color.black / 4;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                if (Target.Pieces == null || Target.Pieces.Count == 0)
                {
                    GUILayout.Label("Pieces list does not contains any piece(s).");
                }
                else
                {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Sort By Name"))
                    {
                        Target.Pieces = Target.Pieces.OrderBy(e => e.Name).ToList();
                    }

                    if (GUILayout.Button("Sort By Id"))
                    {
                        Target.Pieces = Target.Pieces.OrderBy(e => e.Id).ToList();
                    }

                    GUILayout.EndHorizontal();

                    foreach (PieceBehaviour Piece in Target.Pieces)
                    {
                        if (Piece == null)
                        {
                            return;
                        }

                        GUILayout.BeginHorizontal();

                        GUILayout.Space(13);

                        EditorGUI.BeginChangeCheck();

                        string Format = string.Format("[{0}] ", Index) + Piece.Name;

                        GUILayout.BeginHorizontal();

                        PieceFoldout[Index] = EditorGUILayout.Foldout(PieceFoldout[Index],
                            Format, true);

                        GUILayout.FlexibleSpace();

                        GUILayout.EndHorizontal();

                        if (EditorGUI.EndChangeCheck())
                        {
                            if (PieceFoldout[Index] == true)
                            {
                                for (int i = 0; i < PieceFoldout.Length; i++)
                                {
                                    if (i != Index)
                                    {
                                        PieceFoldout[i] = false;
                                    }
                                }
                            }

                            PiecePreviews.Clear();
                        }

                        GUI.color = Color.white;

                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();

                        if (PieceFoldout[Index])
                        {
                            GUI.color = Color.black / 4;
                            GUILayout.BeginHorizontal("helpBox");
                            GUI.color = Color.white;

                            GUILayout.BeginVertical();

                            if (Piece != null)
                            {
                                UnityEditor.Editor PreviewEditor = null;

                                if (PiecePreviews.Count > Index)
                                {
                                    PreviewEditor = PiecePreviews[Index];
                                }

                                if (PreviewEditor == null)
                                {
                                    PreviewEditor = CreateEditor(Piece.gameObject);

                                    PiecePreviews.Add(PreviewEditor);

                                    PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128, 128, 128), EditorStyles.textArea);
                                }
                                else
                                {
                                    PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128, 128, 128), EditorStyles.textArea);
                                }

                                EditorGUILayout.ObjectField(serializedObject.FindProperty("Pieces").GetArrayElementAtIndex(Index), new GUIContent("Piece Behaviour :"));

                                if (GUILayout.Button("Remove Piece"))
                                {
                                    Undo.RecordObject(target, "Remove Piece");
                                    Target.Pieces.Remove(Piece);
                                    Repaint();
                                    EditorUtility.SetDirty(target);
                                    PiecePreviews.Clear();
                                    break;
                                }
                            }

                            GUILayout.EndVertical();

                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndHorizontal();

                        Index++;
                    }
                }

                EditorGUILayout.BeginHorizontal();

                GUI.enabled = Target.Pieces.Count > 0;
                if (GUILayout.Button("Clear All Piece(s) List", GUILayout.MinWidth(200)))
                {
                    Undo.RecordObject(target, "Cancel Clear List");
                    Target.Pieces.Clear();
                    Repaint();
                    EditorUtility.SetDirty(target);
                }
                if (GUILayout.Button("All Piece(s) List To Piece Collection", GUILayout.MinWidth(200)))
                {
                    PieceCollection Collection = ScriptableObjectExtension.CreateAsset<PieceCollection>("New Piece Collection...");
                    Collection.Pieces.AddRange(Target.Pieces);
                    Repaint();
                    EditorUtility.SetDirty(target);
                }
                GUI.enabled = true;

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();

                GUI.color = Color.black / 4;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                Rect DropRect = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));

                GUI.Box(DropRect, "Drag & Drop your pieces or pieces collection here to add them in the list.", EditorStyles.centeredGreyMiniLabel);

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
                            if (DragAndDrop.objectReferences[i] is PieceCollection)
                            {
                                Target.Pieces.AddRange(((PieceCollection)DragAndDrop.objectReferences[i]).Pieces);
                                EditorUtility.SetDirty(target);
                                Repaint();
                            }
                            else
                            {
                                GameObject DraggedObject = DragAndDrop.objectReferences[i] as GameObject;

                                if (DraggedObject == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> : Cannot add empty object!");
                                    return;
                                }

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
                                    Debug.LogError("<b>Easy Build System</b> : Only piece can be added to list!");
                                    return;
                                }

                                if (Target.Pieces.Find(entry => entry.Id == DraggedPiece.Id) == null)
                                {
                                    Undo.RecordObject(target, "Cancel Add Piece");
                                    Target.Pieces.Add(DraggedPiece);
                                    EditorUtility.SetDirty(target);
                                    Repaint();
                                }
                                else
                                {
                                    Debug.LogError("<b>Easy Build System</b> : The piece already exists in the list.");
                                }
                            }
                        }

                        UnityEngine.Event.current.Use();
                    }
                }

                EditorGUI.BeginChangeCheck();
                Selection = EditorGUILayout.Popup("Load Piece Collection", Selection, Options);

                if (EditorGUI.EndChangeCheck())
                {
                    if (Selection - 1 != -1)
                    {
                        Undo.RecordObject(target, "Cancel Add Piece Collection");
                        Target.Pieces.AddRange(Pieces[Selection - 1].Pieces);
                        EditorUtility.SetDirty(target);
                        Repaint();
                        Selection = 0;
                    }
                }

                GUILayout.EndVertical();
            }

            #endregion

            #region Addons

            FoldoutArray[2] = EditorGUILayout.Foldout(FoldoutArray[2], "Add-ons Settings", true);

            if (FoldoutArray[2])
            {
                AddonInspector.DrawAddons(Target, AddonTarget.BuildManager);
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }

#endif
}