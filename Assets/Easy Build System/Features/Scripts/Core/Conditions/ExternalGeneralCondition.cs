using UnityEditor;
using UnityEngine;

using EasyBuildSystem.Features.Scripts.Core.Base.Area;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Extensions;

namespace EasyBuildSystem.Features.Scripts.Core.Conditions
{
    [Condition("External General Condition", "Allow placement if the piece is stable, otherwise physics is applied to make it fall.\n" +
    "You can find more information about this component in the documentation.", ConditionTarget.PieceBehaviour, -999)]
    public class ExternalGeneralCondition : ConditionBehaviour
    {
        public bool IsPlaceable = true;
        public bool IsEditable = true;
        public bool IsDestructible = true;

        public bool RequireAreaForPlacement;
        public bool RequireAreaForDestruction;
        public bool RequireAreaForEdit;

        public bool RequireSocket;

        public override bool CheckForPlacement()
        {
            if (!IsPlaceable)
            {
                return false;
            }

            AreaBehaviour NearestArea = GetNearestArea(Piece.transform.position);

            if (NearestArea != null)
            {
                if (!NearestArea.AllowAllPiecesPlacement)
                {
                    if (!NearestArea.CheckAllowedPlacement(Piece))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (RequireAreaForPlacement)
                {
                    return false;
                }
            }

            if (RequireSocket && !BuilderBehaviour.Instance.HasSocket)
            {
                return false;
            }

            return true;
        }

        public override bool CheckForDestruction()
        {
            if (!IsDestructible)
            {
                return false;
            }

            AreaBehaviour NearestArea = GetNearestArea(Piece.transform.position);

            if (NearestArea != null)
            {
                if (!NearestArea.AllowAllPiecesDestruction)
                {
                    if (!NearestArea.CheckAllowedDestruction(Piece))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (RequireAreaForDestruction)
                {
                    return false;
                }
            }

            return true;
        }

        public override bool CheckForEdit()
        {
            if (!IsEditable)
            {
                return false;
            }

            AreaBehaviour NearestArea = GetNearestArea(Piece.transform.position);

            if (NearestArea != null)
            {
                if (!NearestArea.AllowAllPiecesEdition)
                {
                    if (!NearestArea.CheckAllowedEdition(Piece))
                    {
                        return false;
                    }
                }
            }
            else
            {
                if (RequireAreaForEdit)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the nearest area via an world position.
        /// </summary>
        public AreaBehaviour GetNearestArea(Vector3 position)
        {
            foreach (AreaBehaviour Area in BuildManager.Instance.CachedAreas)
            {
                if (Area != null)
                {
                    if (Area.gameObject.activeSelf == true)
                    {
                        if (Area.Shape == Base.Area.Enums.AreaShape.Bounds)
                        {
                            if (Area.transform.ConvertBoundsToWorld(Area.Bounds).Contains(position))
                            {
                                return Area;
                            }
                        }
                        else
                        {
                            if (Vector3.Distance(position, Area.transform.position) <= Area.Radius)
                            {
                                return Area;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ExternalGeneralCondition), true)]
    public class ExternalGeneralConditionEditor : Editor
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
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsPlaceable"),
                    new GUIContent("Piece Placeable :", "If the piece can be placed during the placement mode."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsDestructible"),
                    new GUIContent("Piece Destructible :", "If the piece can be destroyed during the destruction mode."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("IsEditable"), 
                    new GUIContent("Piece Editable :", "If the piece can be edited during the edit mode."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireAreaForPlacement"), 
                    new GUIContent("Require Area For Placement :", "If the piece requires an area for being placed."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireAreaForDestruction"),
                    new GUIContent("Require Area For Destruction :", "If the piece requires an area for being destroyed."));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireAreaForEdit"),
                    new GUIContent("Require Area For Edit :", "If the piece requires an area for being edited."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("RequireSocket")
                    , new GUIContent("Require Socket :", "If the piece require a socket for being placed"));
            }

            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }

#endif
}