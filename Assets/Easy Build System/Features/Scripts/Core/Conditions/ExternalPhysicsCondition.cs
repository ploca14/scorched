using EasyBuildSystem.Features.Scripts.Core.Base.Condition;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Conditions
{
    [System.Serializable]
    public class Detection
    {
        #region Fields

        public Bounds DetectionBounds;
        public LayerMask RequireLayer = 1 << 0;
        public bool RequireSupport;
        public string[] RequiredCategory;

        #endregion Fields

        #region Methods

        public bool CheckCategory(string type)
        {
            return RequiredCategory.Contains(type);
        }

        #endregion Methods
    }

    [Condition("External Physics Condition", "Allow placement if the piece is stable, otherwise physics is applied to make it fall.\n" +
        "You can find more information about this component in the documentation.", ConditionTarget.PieceBehaviour)]
    public class ExternalPhysicsCondition : ConditionBehaviour
    {
        #region Fields

        public static bool ShowGizmos = true;

        public bool PhysicsSleeping = false;

        public bool RequireStableSupport = false;
        public bool ConvexMeshColliders = true;
        public Detection[] Detections;

        public bool AffectedByPhysics { get; set; }

        private Rigidbody Rigidbody;

        #endregion Fields

        #region Methods

        private void Start()
        {
            if (Piece.CurrentState != StateType.Placed)
                return;

            if (!CheckStability())
            {
                ApplyPhysics();
            }

            BuildEvent.Instance.OnPieceDestroyed.AddListener((PieceBehaviour piece) =>
            {
                if (piece.CurrentState != StateType.Remove) return;

                if (!CheckStability())
                {
                    ApplyPhysics();
                }
            });
        }

        public override bool CheckForPlacement()
        {
            if (RequireStableSupport)
                return CheckStability();
            else
                return true;
        }

        public void ApplyPhysics()
        {
            if (AffectedByPhysics)
            {
                return;
            }

            if (Piece.CurrentState == StateType.Queue)
            {
                return;
            }

            if (Rigidbody == null)
            {
                Rigidbody = Piece.gameObject.AddRigibody(false, false);

                if (ConvexMeshColliders)
                {
                    for (int i = 0; i < Piece.Colliders.Count; i++)
                    {
                        if (Piece.Colliders[i].GetComponent<MeshCollider>() != null)
                        {
                            Piece.Colliders[i].GetComponent<MeshCollider>().convex = true;
                        }
                    }
                }

                Rigidbody.useGravity = true;
                Rigidbody.isKinematic = false;
                Rigidbody.AddForce(Random.insideUnitSphere, ForceMode.Impulse);
            }

            AffectedByPhysics = true;
            //PhysicExtension.SetLayerRecursively(Piece.gameObject, Physics.IgnoreRaycastLayer);
            Destroy(Piece);
            Destroy(Piece.gameObject, 5f);
        }

        public bool CheckStability()
        {
            if (PhysicsSleeping) return true;

            if (Detections == null) return false;

            if (Detections.Length != 0)
            {
                bool[] Results = new bool[Detections.Length];

                for (int i = 0; i < Detections.Length; i++)
                {
                    if (Detections[i] != null)
                    {
                        if (Piece == null) continue;

                        PieceBehaviour[] Pieces = PhysicExtension.GetNeighborsTypeByBox<PieceBehaviour>(Piece.transform.TransformPoint(Detections[i].DetectionBounds.center),
                            Detections[i].DetectionBounds.extents, Piece.transform.rotation, Detections[i].RequireLayer);

                        for (int p = 0; p < Pieces.Length; p++)
                        {
                            PieceBehaviour CollapsePiece = Pieces[p];

                            if (CollapsePiece != null)
                            {
                                if (CollapsePiece != Piece)
                                {
                                    if (CollapsePiece.CurrentState != StateType.Queue && Detections[i].CheckCategory(CollapsePiece.Category))
                                    {
                                        Results[i] = true;
                                    }
                                }
                            }
                        }

                        Collider[] Colliders = PhysicExtension.GetNeighborsTypeByBox<Collider>(Piece.transform.TransformPoint(Detections[i].DetectionBounds.center),
                            Detections[i].DetectionBounds.extents, Piece.transform.rotation, Detections[i].RequireLayer);

                        for (int x = 0; x < Colliders.Length; x++)
                        {
                            if (Detections[i].RequireSupport)
                            {
                                if (BuildManager.Instance.IsBuildableSurface(Colliders[x]))
                                {
                                    Results[i] = true;
                                }
                            }
                        }
                    }
                }

                return Results.All(result => result);
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            if (Detections == null || Detections.Length == 0) return;

            for (int i = 0; i < Detections.Length; i++)
            {
                Gizmos.DrawWireCube(transform.TransformPoint(Detections[i].DetectionBounds.center), Detections[i].DetectionBounds.extents * 2);
            }
        }

        #endregion
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(ExternalPhysicsCondition), true)]
    public class ExternalPhysicsConditionInspector : Editor
    {
        #region Fields

        private ExternalPhysicsCondition Target { get { return (ExternalPhysicsCondition)target; } }
        private static bool[] FoldoutArray = new bool[1];

        #endregion

        #region Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUILayout.BeginHorizontal();
            GUILayout.Space(13);
            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "Conditions Settings", true);
            GUILayout.EndHorizontal();

            if (FoldoutArray[0])
            {
                ExternalPhysicsCondition.ShowGizmos = EditorGUILayout.Toggle("Physics Show Gizmos :", ExternalPhysicsCondition.ShowGizmos);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("PhysicsSleeping"), new GUIContent("Physics Sleeping :"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Detections.Array.size"), new GUIContent("Physics Detection Array Size :"));
                for (int i = 0; i < serializedObject.FindProperty("Detections").arraySize; i++)
                {
                    GUI.color = Color.black / 4f;
                    GUILayout.BeginVertical("helpBox");
                    GUI.color = Color.white;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Detections").GetArrayElementAtIndex(i).FindPropertyRelative("DetectionBounds"), new GUIContent("Physics Detection Bounds :"));
                    SceneView.RepaintAll();
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Detections").GetArrayElementAtIndex(i).FindPropertyRelative("RequireLayer"), new GUIContent("Physics Require Layer(s) :"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Detections").GetArrayElementAtIndex(i).FindPropertyRelative("RequireSupport"), new GUIContent("Physics Require Support :"));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Detections").GetArrayElementAtIndex(i).FindPropertyRelative("RequiredCategory.Array.size"), new GUIContent("Physics Required Category Array Size :"));

                    for (int x = 0; x < serializedObject.FindProperty("Detections").GetArrayElementAtIndex(i).FindPropertyRelative("RequiredCategory").arraySize; x++)
                    {
                        GUI.color = Color.black / 4f;
                        GUILayout.BeginVertical("helpBox");
                        GUI.color = Color.white;
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("Detections").GetArrayElementAtIndex(i).FindPropertyRelative("RequiredCategory").GetArrayElementAtIndex(x));
                        GUILayout.EndVertical();
                    }

                    GUILayout.EndVertical();
                }

                if (GUILayout.Button("Check Stability"))
                {
                    Debug.Log("<b>Easy Build System</b> : The piece is " + (Target.CheckStability() ? "stable" : "unstable"));
                }

                GUILayout.Space(3f);
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }

#endif
}