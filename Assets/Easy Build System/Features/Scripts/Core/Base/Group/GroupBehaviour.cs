using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage.Data;
using EasyBuildSystem.Features.Scripts.Core.Conditions;
using EasyBuildSystem.Features.Scripts.Extensions;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif
using UnityEngine.SceneManagement;

namespace EasyBuildSystem.Features.Scripts.Core.Base.Group
{
    [ExecuteInEditMode]
    public class GroupBehaviour : MonoBehaviour
    {
        #region Fields

        public List<PieceBehaviour> Pieces = new List<PieceBehaviour>();

        private Transform OriginalParent;
        private Transform BatchedParent;

        public bool Batched { get; set; }

        #endregion

        #region Methods

        private void Awake()
        {
            if (transform.Find("OriginalParent") == null)
            {
                OriginalParent = new GameObject("OriginalParent").transform;
                OriginalParent.SetParent(transform);
            }
            else
                OriginalParent = transform.Find("OriginalParent");

            if (transform.Find("BatchedParent") == null)
            {
                BatchedParent = new GameObject("BatchedParent").transform;
                BatchedParent.SetParent(transform);
            }
            else
                BatchedParent = transform.Find("BatchedParent");

            if (BuildManager.Instance.DynamicBatching)
            {
                if (BuildEvent.Instance != null)
                {
                    BuildEvent.Instance.OnChangedBuildMode.AddListener((BuildMode mode) =>
                    {
                        if (mode == BuildMode.None)
                            Invoke("ShowBatchedGroup", 0.1f);
                        else
                            Invoke("ShowOriginalGroup", 0.1f);
                    });
                }
            }

            Pieces.AddRange(GetComponentsInChildren<PieceBehaviour>());
        }

        private void Start()
        {
            if (BuildManager.Instance.DynamicBatching && Application.isPlaying)
                ShowBatchedGroup();
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            if (Pieces.Count == 0)
                Destroy(gameObject);
        }

        /// <summary>
        /// This method allows to show the original group.
        /// </summary>
        public void ShowOriginalGroup()
        {
            Batched = false;
            BatchedParent.gameObject.SetActive(false);

            for (int i = 0; i < BatchedParent.transform.childCount; i++)
            {
                if (Application.isPlaying)
                    Destroy(BatchedParent.transform.GetChild(i).gameObject);
                else
                    DestroyImmediate(BatchedParent.transform.GetChild(i).gameObject);
            }

            foreach (MeshFilter Piece in OriginalParent.GetComponentsInChildren<MeshFilter>())
            {
                if (Piece.GetComponent<MeshRenderer>() != null)
                    Piece.GetComponent<MeshRenderer>().enabled = true;
            }
        }

        /// <summary>
        /// This method allows to show the batched group (if using dynamic batching).
        /// </summary>
        public void ShowBatchedGroup()
        {
            Batched = true;
            BatchedParent.gameObject.SetActive(true);

            foreach (MeshFilter Piece in OriginalParent.GetComponentsInChildren<MeshFilter>())
            {
                ExternalPhysicsCondition PhysicsCondition = Piece.GetComponentInParent<ExternalPhysicsCondition>();

                if (PhysicsCondition != null)
                {
                    if (!PhysicsCondition.AffectedByPhysics)
                    {
                        Instantiate(Piece, BatchedParent.transform, true);
                        Piece.GetComponent<MeshRenderer>().enabled = false;
                    }
                }
            }

            StaticBatchingUtility.Combine(BatchedParent.gameObject);
        }

        /// <summary>
        /// This method allows to add a new piece in this group.
        /// </summary>
        public void AddPiece(PieceBehaviour piece)
        {
            if (piece == null) return;

            Pieces.Add(piece);

            piece.transform.SetParent(OriginalParent, true);
        }

        public void RemovePiece(PieceBehaviour piece)
        {
            if (piece == null) return;

            Pieces.Remove(piece);
        }

        /// <summary>
        /// This method allows to get a model which contains all the base piece data.
        /// </summary>
        public PieceData GetModel()
        {
            PieceData Model = new PieceData();

            PieceBehaviour[] Pieces = GetComponentsInChildren<PieceBehaviour>();

            Model.Pieces = new List<PieceData.SerializedPiece>();

            for (int i = 0; i < Pieces.Length; i++)
            {
                Model.Pieces.Add(new PieceData.SerializedPiece()
                {
                    Id = Pieces[i].Id,
                    Name = Pieces[i].Name,
                    SkinIndex = Pieces[i].SkinIndex,
                    Position = PieceData.ParseToSerializedVector3(Pieces[i].transform.position),
                    Rotation = PieceData.ParseToSerializedVector3(Pieces[i].transform.eulerAngles),
                    Scale = PieceData.ParseToSerializedVector3(Pieces[i].transform.localScale),
                    Properties = Pieces[i].ExtraProperties
                });
            }

            return Model;
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(GroupBehaviour))]
    public class GroupBehaviourEditor : Editor
    {
        #region Fields

        private GroupBehaviour Target { get { return (GroupBehaviour)target; } }
        private static bool[] FoldoutArray = new bool[2];

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            AddonInspector.LoadAddons(Target, AddonTarget.GroupBehaviour);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region General

            InspectorStyles.DrawSectionLabel("Group Behaviour - Component");

            GUILayout.Label("Automatically instantiated to each new piece to regroup the closest pieces together.\n" +
                "Find more information about this component on the documentation.", EditorStyles.miniLabel);

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                GUILayout.BeginHorizontal();
                GUI.enabled = Target.Batched;
                if (GUILayout.Button("Show Original Group"))
                {
                    Target.ShowOriginalGroup();
                }
                GUI.enabled = false;

                GUI.enabled = !Target.Batched;
                if (GUILayout.Button("Show Batched Group"))
                {
                    Target.ShowBatchedGroup();
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUI.enabled = Target.transform.GetComponentsInChildren<PieceBehaviour>().Length > 0;
                if (GUILayout.Button("Export As Blueprint Template..."))
                {
                    Scriptables.Blueprint.BlueprintTemplate Data = ScriptableObjectExtension.CreateAsset<Scriptables.Blueprint.BlueprintTemplate>("New Blueprint " + Target.name);
                    Data.name = Target.name;
                    Data.Model = Target.GetModel();
                    Data.Data = Target.GetModel().ToJson();
                    Data.SourceSceneName = SceneManager.GetActiveScene().name;
                }
                GUI.enabled = true;
            }

            #endregion

            #region Addons

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Add-ons Settings", true);

            if (FoldoutArray[1])
            {
                AddonInspector.DrawAddons(Target, AddonTarget.GroupBehaviour);
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }

#endif
}