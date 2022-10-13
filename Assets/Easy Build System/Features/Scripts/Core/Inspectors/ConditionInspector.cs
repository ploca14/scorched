#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Base.Condition;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Condition.Helper;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EasyBuildSystem.Features.Scripts.Core.Inspectors
{
    public class ConditionInspector : Editor
    {
        #region Fields

        private static List<ConditionAttribute> Conditions = new List<ConditionAttribute>();
        private static Editor ConditionEditor;

        #endregion

        #region Methods

        public static void LoadConditions(MonoBehaviour target, ConditionTarget conditionTarget)
        {
            Conditions = ConditionHelper.GetConditionsByTarget(conditionTarget);

            foreach (ConditionBehaviour Condition in target.GetComponentsInChildren<ConditionBehaviour>())
                if (Condition != null)
                    Condition.hideFlags = HideFlags.HideInInspector;
        }

        public static void DrawConditions(MonoBehaviour target, ConditionTarget addOnTarget)
        {
            EditorGUILayout.BeginVertical();

            if (Conditions.Count == 0)
            {
                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;
                GUILayout.Label("No conditions was found for this component.");
                GUILayout.EndVertical();
            }

            foreach (ConditionAttribute Condition in Conditions)
            {
                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                GUILayout.BeginHorizontal();

                GUILayout.Label(Condition.Name, EditorStyles.largeLabel);

                GUILayout.FlexibleSpace();

                if (target.gameObject.GetComponent(Condition.Behaviour) != null)
                {
                    GUILayout.BeginVertical();

                    GUILayout.Space(5f);

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Copy Settings", GUILayout.Width(120)))
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(target.GetComponent(Condition.Behaviour));
                    }

                    if (GUILayout.Button("Paste Settings", GUILayout.Width(120)))
                    {
                        UnityEditorInternal.ComponentUtility.PasteComponentValues(target.GetComponent(Condition.Behaviour));
                        EditorUtility.SetDirty(target);
                    }

                    if (GUILayout.Button("Disable Condition", GUILayout.Width(120)))
                    {
                        try
                        {
                            DestroyImmediate(target.gameObject.GetComponent(Condition.Behaviour), true);
                            break;
                        }
                        catch { }
                    }

                    GUILayout.Space(2f);

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.BeginVertical();

                    GUILayout.Space(5f);

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Enable Condition", GUILayout.Width(120)))
                    {
                        if (target.gameObject.GetComponent(Condition.Behaviour) != null)
                        {
                            return;
                        }

                        Component Com = target.gameObject.AddComponent(Condition.Behaviour);
                        Com.hideFlags = HideFlags.HideInInspector;
                    }

                    GUILayout.Space(2f);

                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                GUILayout.EndHorizontal();

                GUILayout.Label(Condition.Description, EditorStyles.wordWrappedMiniLabel);

                GUILayout.BeginHorizontal();
                if (target.gameObject.GetComponent(Condition.Behaviour) != null)
                {
                    GUILayout.BeginVertical();

                    if (Selection.gameObjects.Length > 1)
                    {
                        EditorGUILayout.HelpBox("Conditions not support yet the multiple-editing.", MessageType.Warning);
                    }
                    else
                    {
                        ConditionEditor = CreateEditor(target.gameObject.GetComponent(Condition.Behaviour));
                        ConditionEditor.OnInspectorGUI();
                    }

                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        #endregion
    }
}
#endif