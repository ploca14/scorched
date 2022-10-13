using EasyBuildSystem.Features.Scripts.Core.Inspectors;
using EasyBuildSystem.Features.Scripts.Editor.Inspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class IntegrationEditor : EditorWindow
{
    #region Fields

    private static List<BuildTargetGroup> Targets;
    private Vector2 IntegrationScrollPosition;
    private static readonly List<string> Integrations = new List<string>();

    #endregion

    #region Methods

    private void OnGUI()
    {
        InspectorStyles.DrawSectionLabel("Easy Build System - Integrations");

        GUILayout.Label("All the integrations are available in the folder: Easy Build System/Integrations\n" +
            "Some integrations may not work with the new versions, let us know so that we update them.", EditorStyles.wordWrappedMiniLabel);

        GUI.color = Color.black / 4;
        GUILayout.BeginVertical();
        GUI.color = Color.white;

        IntegrationScrollPosition = GUILayout.BeginScrollView(IntegrationScrollPosition);

        AddIntegration("Game Creator", "/content/89443", "GAMECREATOR",
            "Catsoft Studios", "Allows integrating Easy Build System with Game Creator in a few clicks.\n" +
            "You can find installation guide for this integration on the documentation.", "Game Creator Integration", null, null);

        AddIntegration("Photon Network V2", "/content/119922", "EXITGAMESV2",
            "Exit Games", "Allows integrating Easy Build System with Photon Network V2 in a few clicks.\n" +
            "You can find installation guide for this integration on the documentation.", "Photon Network V2 Integration", null, null);

        AddIntegration("Mirror Network", "/content/129321", "MIRRORNETWORK",
            "Vis2k", "Allows integrating Easy Build System with Mirror Network in a few clicks.\n" +
            "You can find installation guide for this integration on the documentation.", "Mirror Integration", null, null);

        AddIntegration("uSurvival", "/content/95015", "USURVIVALVIS2K",
            "Vis2k", "Allows integrating Easy Build System with uSurvival in a few clicks.\n" +
            "You can find installation guide for this integration on the documentation.", "uSurvival Integration", null, null);

        AddIntegration("uRPG", "/content/95016", "URPGVIS2K",
            "Vis2k", "Allows integrating Easy Build System with uRPG in a few clicks.\n" +
            "You can find installation guide for this integration on the documentation.", "uRPG Integration", null, null);

        AddIntegration("Rewired", "/content/21676", "REWIRED",
            "Guavaman Enterprises", "Allows integrating Easy Build System with Rewired in a few clicks.\n" +
            "You can find installation guide for this integration on the documentation.", "Rewired Integration", null, null);

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
    }

    private void AddIntegration(string name, string link, string defName, string publisher, string description, string packageName, Action onEnable, Action onDisable)
    {
        GUI.color = Color.black / 4;
        GUILayout.BeginVertical("helpBox");
        GUI.color = Color.white;

        GUILayout.BeginHorizontal();
        InspectorStyles.DrawSectionLabel(name + " by ");
        GUILayout.Space(-8f);
        if (InspectorStyles.LinkLabel(new GUIContent(publisher)))
        {
            if (link.Contains("http"))
                Application.OpenURL(link);
            else
                AssetStore.Open(link);
        }

        GUILayout.FlexibleSpace();

        GUILayout.BeginVertical();
        GUILayout.Space(5f);
        GUILayout.BeginHorizontal();
        if (!IsIntegrationEnabled(defName))
        {
            if (GUILayout.Button("Enable Integration", GUILayout.Width(130)))
            {
                if (packageName == string.Empty)
                {
                    EnableIntegration(defName, onEnable);
                }
                else
                {
                    if (GetRelativePath(packageName) != string.Empty)
                    {
                        EnableIntegration(defName, onEnable);
                        AssetDatabase.ImportPackage(GetRelativePath(packageName), true);
                    }
                    else
                    {
                        Debug.LogWarning("<b>Easy Build System</b> : You must before import the integrations via the Package Importer.");
                    }
                }
            }
        }
        else
        {
            if (GUILayout.Button("Disable Integration", GUILayout.Width(130)))
            {
                DisableIntegration(defName, onDisable);
            }
        }
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        GUILayout.Label(description, EditorStyles.wordWrappedMiniLabel);

        GUILayout.Space(7f);

        GUILayout.EndVertical();
    }

    private string GetRelativePath(string packageName)
    {
        string[] allPaths = Directory.GetFiles(Application.dataPath, "*", SearchOption.AllDirectories);

        for (int i = 0; i < allPaths.Length; i++)
            if (allPaths[i].Contains(packageName))
                return allPaths[i];

        return string.Empty;
    }

    private static bool IsIntegrationEnabled(string name)
    {
        return PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone).Contains(name);
    }

    public static void DisableIntegration(string name, Action onDisable)
    {
        if (IsIntegrationEnabled(name) == false)
        {
            return;
        }

        if (onDisable != null)
        {
            onDisable.Invoke();
        }

        Targets = new List<BuildTargetGroup>
            {
                BuildTargetGroup.iOS,

                BuildTargetGroup.WebGL,

                BuildTargetGroup.Standalone,

                BuildTargetGroup.Android
            };

        foreach (BuildTargetGroup Target in Targets)
        {
            string Symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(Target);

            string[] SplitArray = Symbols.Split(';');

            List<string> Array = new List<string>(SplitArray);

            Array.Remove(name);

            if (Target != BuildTargetGroup.Unknown)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(Target, string.Join(";", Array.ToArray()));
            }
        }

        Debug.Log("<b>Easy Build System</b> : Integration <b>(" + name + ")</b> has been disabled !");
    }

    public static void EnableIntegration(string name, Action onEnable)
    {
        if (IsIntegrationEnabled(name))
        {
            return;
        }

        Targets = new List<BuildTargetGroup>
            {
                BuildTargetGroup.iOS,

                BuildTargetGroup.WebGL,

                BuildTargetGroup.Standalone,

                BuildTargetGroup.Android,
            };

        foreach (BuildTargetGroup Target in Targets)
        {
            string Symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(Target);

            string[] SplitArray = Symbols.Split(';');

            List<string> Array = new List<string>(SplitArray)
                {
                    name
                };

            if (Target != BuildTargetGroup.Unknown)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(Target, string.Join(";", Array.ToArray()));
            }
        }

        if (onEnable != null)
        {
            Integrations.Add(onEnable.Method.Name);

            onEnable.Invoke();
        }

        Debug.Log("<b>Easy Build System</b> : Integration <b>(" + name + ")</b> has been enabled !");
    }

    [MenuItem(@"Tools/Easy Build System/Integration Manager...", priority = -798)]
    public static void Init()
    {
        EditorWindow Window = GetWindow(typeof(IntegrationEditor), false, "Integrations", true);

        Window.titleContent.image = EditorGUIUtility.IconContent("d_SceneViewFx").image;
        Window.autoRepaintOnSceneChange = true;

        Window.ShowAuxWindow();

        Window.position = new Rect(Screen.width / 2, Screen.height / 2, 550, 300);
        Window.maxSize = new Vector2(550, 900);

        Window.Show();
    }

    #endregion
}
