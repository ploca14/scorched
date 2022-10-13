#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Addons.Helper;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Inspectors
{
    public class AddonInspector : MonoBehaviour
    {
        #region Fields

        private static List<AddonAttribute> Addons = new List<AddonAttribute>();
        private static UnityEditor.Editor AddonEditor;

        #endregion

        #region Methods

        public static void LoadAddons(MonoBehaviour target, AddonTarget addonTarget)
        {
            Addons = AddonHelper.GetAddonsByTarget(addonTarget);

            foreach (AddonBehaviour Addon in target.GetComponentsInChildren<AddonBehaviour>())
                if (Addon != null)
                    Addon.hideFlags = HideFlags.HideInInspector;
        }

        public static void DrawAddons(MonoBehaviour target, AddonTarget addonTarget)
        {
            if (Addons.Count == 0)
            {
                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;
                GUILayout.Label("No add-ons was found for this component.");
                GUILayout.EndVertical();
            }
            else
            {
                foreach (AddonAttribute Addon in Addons)
                {
                    if (Addon.Target == addonTarget)
                    {
                        GUI.color = Color.black / 4f;
                        GUILayout.BeginVertical("helpBox");
                        GUI.color = Color.white;

                        GUILayout.BeginHorizontal();

                        GUILayout.Label(Addon.Name, EditorStyles.largeLabel);

                        GUILayout.FlexibleSpace();

                        if (target.gameObject.GetComponent(Addon.Behaviour) == null)
                        {
                            GUILayout.BeginVertical();

                            GUILayout.Space(5f);

                            GUILayout.BeginHorizontal();

                            if (GUILayout.Button("Enable Add-On", GUILayout.Width(120)))
                            {
                                if (target.gameObject.GetComponent(Addon.Behaviour) != null)
                                {
                                    return;
                                }

                                Component Target = target.gameObject.AddComponent(Addon.Behaviour);
                                Target.hideFlags = HideFlags.HideInInspector;
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

                            if (GUILayout.Button("Copy Settings", GUILayout.Width(120)))
                            {
                                UnityEditorInternal.ComponentUtility.CopyComponent(target.GetComponent(Addon.Behaviour));
                            }

                            if (GUILayout.Button("Paste Settings", GUILayout.Width(120)))
                            {
                                UnityEditorInternal.ComponentUtility.PasteComponentValues(target.GetComponent(Addon.Behaviour));
                                EditorUtility.SetDirty(target);
                            }

                            if (GUILayout.Button("Disable Add-On", GUILayout.Width(120)))
                            {
                                try
                                {
                                    DestroyImmediate(target.gameObject.GetComponent(Addon.Behaviour), true);
                                    break;
                                }
                                catch { }
                            }

                            GUILayout.Space(2f);

                            GUILayout.EndHorizontal();

                            GUILayout.EndVertical();
                        }

                        GUILayout.EndHorizontal();

                        GUILayout.Label(Addon.Description, EditorStyles.wordWrappedMiniLabel);

                        GUILayout.BeginHorizontal();
                        if (target.gameObject.GetComponent(Addon.Behaviour) != null)
                        {
                            GUILayout.BeginVertical();

                            AddonEditor = UnityEditor.Editor.CreateEditor(target.gameObject.GetComponent(Addon.Behaviour));
                            AddonEditor.OnInspectorGUI();

                            GUILayout.EndVertical();
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }
                }
            }
        }

        #endregion
    }
}
#endif