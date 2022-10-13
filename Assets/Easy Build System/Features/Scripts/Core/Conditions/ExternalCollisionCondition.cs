using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Conditions
{
    [Condition("External Collision Condition", "Allow the placement if the piece does not collide with another colliders.\n" +
        "You can find more information about this component in the documentation.", ConditionTarget.PieceBehaviour)]
    public class ExternalCollisionCondition : ConditionBehaviour
    {
        #region Fields

        public LayerMask CollisionLayer = 1 << 0;
        [Range(0f, 10f)]
        public float CollisionClippingToleranceWithBuildSurface = 1f;
        [Range(0f, 10f)]
        public float CollisionClippingTolerance = 1f;
        [Range(0f, 10f)]
        public float CollisionClippingSnappingTolerance = 0.99f;
        public string[] CollisionIgnoreCategory;
        public bool RequireBuildableSurface;
        public bool CollisionIgnoreWhenSnap;

        public static bool ShowGizmos = true;

        #endregion Fields

        #region Methods

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            if (Piece == null) return;

            Gizmos.matrix = Piece.transform.localToWorldMatrix;

            Gizmos.color = Color.cyan / 2f;
            Gizmos.DrawCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingToleranceWithBuildSurface * 1.001f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingToleranceWithBuildSurface * 1.001f);

            Gizmos.color = Color.yellow / 2f;
            Gizmos.DrawCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingTolerance * 1.001f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingTolerance * 1.001f);

            Gizmos.color = Color.white / 2f;
            Gizmos.DrawCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingSnappingTolerance * 1.001f);

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(Piece.MeshBounds.center, Piece.MeshBounds.size * CollisionClippingSnappingTolerance * 1.001f);
        }

        public override bool CheckForPlacement()
        {
            bool hasBuildableSurface = false;
            bool canBePlaced = true;

            Collider[] colliders = PhysicExtension.GetNeighborsTypeByBox<Collider>(Piece.MeshBoundsToWorld.center,
                Piece.MeshBoundsToWorld.extents * CollisionClippingToleranceWithBuildSurface, Piece.transform.rotation, CollisionLayer).Where(x => !x.isTrigger).ToArray();

            if (RequireBuildableSurface)
            {
                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                        if (BuildManager.Instance.IsBuildableSurface(colliders[i]))
                            hasBuildableSurface = true;
                }

                if (!hasBuildableSurface)
                    return false;
            }
            else
                hasBuildableSurface = true;

            colliders = PhysicExtension.GetNeighborsTypeByBox<Collider>(Piece.MeshBoundsToWorld.center,
                Piece.MeshBoundsToWorld.extents * CollisionClippingTolerance, Piece.transform.rotation, CollisionLayer).Where(x => !x.isTrigger).ToArray();

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    if (RequireBuildableSurface)
                    {
                        if (colliders[i].GetComponentInParent<PieceBehaviour>() == null && !BuildManager.Instance.IsBuildableSurface(colliders[i]))
                            canBePlaced = false;
                    }
                    else
                    {
                        canBePlaced = true;
                    }
                }
            }

            if (canBePlaced && !CollisionIgnoreWhenSnap)
            {
                colliders = PhysicExtension.GetNeighborsTypeByBox<Collider>(Piece.MeshBoundsToWorld.center,
                   Piece.MeshBoundsToWorld.extents * CollisionClippingSnappingTolerance, Piece.transform.rotation, CollisionLayer).ToArray();

                for (int i = 0; i < colliders.Length; i++)
                {
                    if (colliders[i] != null)
                    {
                        if (colliders[i].GetComponentInParent<PieceBehaviour>() != null)
                        {
                            if (!CollisionIgnoreCategory.Contains(colliders[i].GetComponentInParent<PieceBehaviour>().Category))
                            {
                                canBePlaced = false;
                            }
                        }
                    }
                }
            }

            return hasBuildableSurface && canBePlaced;
        }

        #endregion
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ExternalCollisionCondition), true)]
    public class ExternalColliderConditionInspector : Editor
    {
        #region Fields

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
                ExternalCollisionCondition.ShowGizmos = EditorGUILayout.Toggle("Collision Show Gizmos :", ExternalCollisionCondition.ShowGizmos);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionLayer"), new GUIContent("Collision LayerMask :"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionClippingTolerance"), new GUIContent("Collision Tolerance :", "Allowed collision tolerance with the collider (except Buildable Surface)."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionClippingSnappingTolerance"), new GUIContent("Collision Snapping Tolerance :", "Allowed collision snapping tolerance when the piece is on a socket."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireBuildableSurface"), new GUIContent("Collision Require Buildable Surface :"));

                if (serializedObject.FindProperty("RequireBuildableSurface").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionClippingToleranceWithBuildSurface"), new GUIContent("Collision Buildable Surface Tolerance :", "Allowed collision tolerance with the Buildable Surface."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionIgnoreWhenSnap"), new GUIContent("Collision Ignore When Snapped :"));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionIgnoreCategory.Array.size"), new GUIContent("Collision Ignore Categories :"));
                for (int i = 0; i < serializedObject.FindProperty("CollisionIgnoreCategory").arraySize; i++)
                {
                    GUI.color = Color.black / 4f;
                    GUILayout.BeginVertical("helpBox");
                    GUI.color = Color.white;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CollisionIgnoreCategory").GetArrayElementAtIndex(i), new GUIContent("Element " + i));
                    GUILayout.EndVertical();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }

#endif
}