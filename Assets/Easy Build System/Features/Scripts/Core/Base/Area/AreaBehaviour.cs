using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Area.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;

#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Area
{
    [AddComponentMenu("Easy Build System/Components/Area Behaviour")]
    public class AreaBehaviour : MonoBehaviour
    {
        #region Fields

        public AreaShape Shape;
        public float Radius = 5f;
        public Bounds Bounds = new Bounds(Vector3.zero, Vector3.one);

        public bool AllowAllPiecesPlacement = true;
        public List<PieceBehaviour> AllowSpecificPiecesPlacement;
        public bool AllowAllPiecesDestruction = true;
        public List<PieceBehaviour> AllowDestructionSpecificPieces;
        public bool AllowAllPiecesEdition = true;
        public List<PieceBehaviour> AllowEditionSpecificPieces;

        #endregion Fields

        #region Methods

        private void Start()
        {
            BuildManager.Instance.AddArea(this);
        }

        private void OnDestroy()
        {
            BuildManager.Instance.RemoveArea(this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan / 2;

            if (Shape == AreaShape.Bounds)
            {
                Gizmos.DrawCube(transform.TransformPoint(Bounds.center), Bounds.size);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(transform.TransformPoint(Bounds.center), Bounds.size);
            }
            else if (Shape == AreaShape.Sphere)
            {
                Gizmos.DrawSphere(transform.position, Radius);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, Radius);
            }
        }

        /// <summary>
        /// Allows to check if the piece is allowed for placement in this area.
        /// </summary>
        public bool CheckAllowedPlacement(PieceBehaviour piece)
        {
            if (AllowSpecificPiecesPlacement.Count == 0) return false;

            return AllowSpecificPiecesPlacement.Find(entry => entry.Id == piece.Id);
        }

        /// <summary>
        /// Allows to check if the piece is allowed for destruction in this area.
        /// </summary>
        public bool CheckAllowedDestruction(PieceBehaviour piece)
        {
            if (AllowDestructionSpecificPieces.Count == 0) return false;

            return AllowDestructionSpecificPieces.Find(entry => entry.Id == piece.Id);
        }

        /// <summary>
        /// Allows to check if the piece is allowed for edition in this area.
        /// </summary>
        public bool CheckAllowedEdition(PieceBehaviour piece)
        {
            if (AllowEditionSpecificPieces.Count == 0) return false;

            return AllowEditionSpecificPieces.Find(entry => entry.Id == piece.Id);
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(AreaBehaviour))]
    public class AreaBehaviourInspector : Editor
    {
        #region Fields

        private AreaBehaviour Target { get { return (AreaBehaviour)target; } }
        private static bool[] FoldoutArray = new bool[3];

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            AddonInspector.LoadAddons(Target, AddonTarget.AreaBehaviour);
            ConditionInspector.LoadConditions(Target, ConditionTarget.AreaBehaviour);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Area Behaviour - Component");

            GUILayout.Label("Create an area which allows to limit the placement, destruction and editing of pieces.\n" +
                "Find more information about this component in the documentation.", EditorStyles.miniLabel);

            EditorGUILayout.Space(1);

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Shape"),
                    new GUIContent("Shape Type :", "Area shape type."));

                if (Target.Shape == AreaShape.Bounds)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Bounds"),
                        new GUIContent("Shape Size :", "Area shape size."));
                else
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Radius"),
                        new GUIContent("Shape Radius :", "Area shape radius."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowAllPiecesPlacement"),
                    new GUIContent("Allow All Pieces Placement :", "Allow the placement of all the pieces in area."));

                if (!serializedObject.FindProperty("AllowAllPiecesPlacement").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowSpecificPiecesPlacement"),
                        new GUIContent("Allow Only Specific Pieces Placement :", "Allow only the placement of predefined pieces."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowAllPiecesDestruction"),
                    new GUIContent("Allow All Pieces Destruction :", "Allow the destruction of all the pieces in area."));

                if (!serializedObject.FindProperty("AllowAllPiecesDestruction").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowDestructionSpecificPieces"),
                        new GUIContent("Allow Only Specific Pieces Destruction :", "Allow only the destruction of predefined pieces."), true);

                EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowAllPiecesEdition"),
                    new GUIContent("Allow All Pieces Edition :", "Allow the edition of all the pieces in area."));

                if (!serializedObject.FindProperty("AllowAllPiecesEdition").boolValue)
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("AllowEditionSpecificPieces"), 
                        new GUIContent("Allow Only Specific Pieces Edition :", "Allow only the edition of predefined pieces."), true);
            }

            #endregion

            #region Conditions

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Conditions Settings", true);

            if (FoldoutArray[1])
                ConditionInspector.DrawConditions(Target, ConditionTarget.AreaBehaviour);

            #endregion

            #region Add-ons

            FoldoutArray[2] = EditorGUILayout.Foldout(FoldoutArray[2], "Add-ons Settings", true);

            if (FoldoutArray[2])
                AddonInspector.DrawAddons(Target, AddonTarget.AreaBehaviour);

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }

#endif
}