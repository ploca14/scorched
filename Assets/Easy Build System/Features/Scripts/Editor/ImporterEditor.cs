using EasyBuildSystem.Features.Scripts.Core.Inspectors;
using System.Collections;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class InitializeOnLoad
{
    private const string k_ProjectOpened = "_initializedProject";

    static InitializeOnLoad()
    {
        if (!SessionState.GetBool(k_ProjectOpened, false))
        {
            SessionState.SetBool(k_ProjectOpened, true);

            ImporterEditor.AddMissingLayers(new string[1] { "Socket" });

            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                {
                    AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/Replacements/Demos & Add-Ons - HDRP Replacement.unitypackage", false);
                    AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/Replacements/Features - HDRP Replacement.unitypackage", false);
                }
                else
                {
                    AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/Replacements/Demos & Add-Ons - URP Replacement.unitypackage", false);
                    AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/Replacements/Features - URP Replacement.unitypackage", false);
                }
            }
        }
    }
}

public class ImporterEditor : EditorWindow
{
    public const string VERSION = "Release 5.6";

    private AddRequest Request;

    private static bool ImportedDemosAddonsPackage;
    private static bool ImportedXRInteractionToolkitPackage;
    private static bool ImportedXRManagementPackage;
    private static bool ImportedInputSystemPackage;
    private static bool ImportedInputSystemSupport;

    private static bool Installing;
    private static float Progression;

    private static string Logs;

    private void OnGUI()
    {
        if (Installing)
        {
            GUILayout.Label(Logs);

            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            EditorGUI.ProgressBar(rect, Progression, "Progression " + Progression * 100f + "%");

            return;
        }

        GUI.color = Color.black / 4;
        GUILayout.BeginVertical("helpBox");
        GUI.color = Color.white;

        InspectorStyles.DrawSectionLabel("Easy Build System - Package Importer");

        GUILayout.Label("Check the system requirements or import supports for XR Interaction Toolkit and the new Input System.\n" +
            "Find more information about Package Importer on the Getting Started page of the documentation.", EditorStyles.wordWrappedMiniLabel);

        GUILayout.EndVertical();

        GUI.color = Color.black / 4;
        GUILayout.BeginVertical("helpBox");
        GUI.color = Color.white;

        InspectorStyles.DrawSectionLabel("Requirements");

        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Unity Version :"), GUILayout.Width(90));
        GUI.color = Color.green;
        GUILayout.BeginVertical();
        GUILayout.Label(Application.unityVersion, GUILayout.Width(150));
        GUILayout.EndVertical();
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Render Pipeline :"), GUILayout.Width(100));
        if (GraphicsSettings.currentRenderPipeline)
        {
            if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
            {
                GUI.color = Color.green;
                GUILayout.BeginVertical();
                GUILayout.Label("High Definition Render Pipeline", GUILayout.Width(350));
                GUILayout.EndVertical();
                GUI.color = Color.white;
            }
            else
            {
                GUI.color = Color.green;
                GUILayout.BeginVertical();
                GUILayout.Label("Universal Render Pipeline", GUILayout.Width(350));
                GUILayout.EndVertical();
                GUI.color = Color.white;
            }
        }
        else
        {
            GUI.color = Color.green;
            GUILayout.BeginVertical();
            GUILayout.Label("Built-in Render Pipeline", GUILayout.Width(350));
            GUILayout.EndVertical();
            GUI.color = Color.white;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Package Version :"), GUILayout.Width(110));
        GUI.color = Color.green;
        GUILayout.BeginVertical();
        GUILayout.Label(VERSION, GUILayout.Width(150));
        GUILayout.EndVertical();
        GUI.color = Color.white;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Internet Reachability :"), GUILayout.Width(130));

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GUI.color = Color.yellow;
            GUILayout.BeginVertical();
            GUILayout.Label("UnReachable", GUILayout.Width(150));
            GUILayout.EndVertical();
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.green;
            GUILayout.BeginVertical();
            GUILayout.Label("Reachable", GUILayout.Width(150));
            GUILayout.EndVertical();
            GUI.color = Color.white;
        }

        GUILayout.EndHorizontal();

        GUILayout.EndHorizontal();

        if (GUILayout.Button("Check Render Pipeline Integrity..."))
        {
            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                {
                    AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/Replacements/Demos & Add-Ons - HDRP Replacement.unitypackage", false);
                }
                else
                {
                    AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/Replacements/Demos & Add-Ons - URP Replacement.unitypackage", false);
                }
            }
        }

        if (GUILayout.Button("Import Cross-Platform Support..."))
        {
            if (EditorUtility.DisplayDialog("Easy Build System - Import Package",
                "This will import the New Input System from the Packager Manager.", "Import", "Cancel"))
            {
                EditorApplication.update += ProcessDownload;
                CurrentDownload = Install(false, true, false);
            }
        }

        if (GUILayout.Button("Import XR Interaction Toolkit Support..."))
        {
            if (EditorUtility.DisplayDialog("Easy Build System - Import Package",
                    "If you have already imported this package and have you made changes, these changes will be liable to be erased if you re-imported the package.", "Import", "Cancel"))
            {
                if (EditorUtility.DisplayDialog("Easy Build System - Import Package",
                    "This will import the XR Interaction Toolkit & XR Management from the Packager Manager.", "Import", "Cancel"))
                {
                    EditorApplication.update += ProcessDownload;
                    CurrentDownload = Install(true, false, false);
                }
            }
        }
    }

    private static void ProcessDownload()
    {
        if (CurrentDownload != null)
            CurrentDownload.MoveNext();
    }

    private static IEnumerator CurrentDownload;
    private IEnumerator Install(bool importXRInteractionToolkit, bool importNewInputSystem, bool importDemosAddons)
    {
        Logs = null;

        ImportedDemosAddonsPackage = false;
        ImportedXRInteractionToolkitPackage = false;
        ImportedInputSystemPackage = false;
        ImportedInputSystemSupport = false;

        EditorApplication.LockReloadAssemblies();

        Installing = true;

        Logs += "Importing packages...\n";

        if (importDemosAddons)
        {
            Logs += "Importing Easy Build System - Demos & Add-Ons...\n";

            AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/Demos & Add-Ons.unitypackage", true);
            AssetDatabase.importPackageCompleted += (string packageName) => { ImportedDemosAddonsPackage = true; };
            while (!ImportedDemosAddonsPackage)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        Progression = 0.25f;

        if (importNewInputSystem)
        {
            Logs += "Importing com.unity.inputsystem@1.1.0-preview.3...\n";

            Logs += "Importing New Input System Support...\n";
            AssetDatabase.ImportPackage("Assets/Easy Build System/Features/Resources/Editor/Packages/New Input System.unitypackage", false);
            AssetDatabase.importPackageCompleted += (string packageName) => { ImportedInputSystemSupport = true; };
            while (!ImportedInputSystemSupport)
            {
                yield return new WaitForEndOfFrame();
            }

            Request = Client.Add("com.unity.inputsystem@1.1.0-preview.3");
            EditorApplication.update += DownloadInputSystem;
            while (!ImportedInputSystemPackage)
            {
                yield return new WaitForEndOfFrame();
            }

            IntegrationEditor.EnableIntegration("EBS_NEW_INPUT_SYSTEM", () => { });
        }

        if (importXRInteractionToolkit && Application.internetReachability != NetworkReachability.NotReachable)
        {
            Logs += "Importing com.unity.xr.management@4.0.5...\n";
            Request = Client.Add("com.unity.xr.management@4.0.5");
            EditorApplication.update += DownloadXRManagement;
            while (!ImportedXRManagementPackage)
            {
                yield return new WaitForEndOfFrame();
            }

            Progression = 0.7f;

            Logs += "Importing com.unity.xr.interaction.toolkit@1.0.0-pre.3...\n";
            Request = Client.Add("com.unity.xr.interaction.toolkit@1.0.0-pre.3");
            EditorApplication.update += DownloadXRInteractionToolkit;
            while (!ImportedXRInteractionToolkitPackage)
            {
                yield return new WaitForEndOfFrame();
            }

            Progression = 0.9f;

            IntegrationEditor.EnableIntegration("EBS_XR", () => { });
        }

        Progression = 1f;

        EditorApplication.update -= ProcessDownload;

        Installing = false;
        EditorPrefs.SetBool("isInstalled_" + Application.productName, true);
        EditorApplication.UnlockReloadAssemblies();
        Close();
    }

    private void OnImportedDemosAndAddons(string packageName)
    {
        AssetDatabase.importPackageCompleted -= OnImportedDemosAndAddons;
    }

    private void DownloadXRInteractionToolkit()
    {
        if (Request.IsCompleted)
        {
            EditorApplication.update -= DownloadXRInteractionToolkit;
            ImportedXRInteractionToolkitPackage = true;
        }
    }

    private void DownloadXRManagement()
    {
        if (Request.IsCompleted)
        {
            EditorApplication.update -= DownloadXRManagement;
            ImportedXRManagementPackage = true;
        }
    }

    private void DownloadInputSystem()
    {
        if (Request.IsCompleted)
        {
            EditorApplication.update -= DownloadInputSystem;
            ImportedInputSystemPackage = true;
        }
    }

    public static void AddMissingLayers(string[] layerNames)
    {
        SerializedObject manager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layersProp = manager.FindProperty("layers");

        foreach (string name in layerNames)
        {
            bool found = false;
            for (int i = 0; i <= 31; i++)
            {
                SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);
                if (sp != null && name.Equals(sp.stringValue))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                SerializedProperty slot = null;
                for (int i = 8; i <= 31; i++)
                {
                    SerializedProperty sp = layersProp.GetArrayElementAtIndex(i);

                    if (sp != null && string.IsNullOrEmpty(sp.stringValue))
                    {
                        slot = sp;
                        break;
                    }
                }

                if (slot != null)
                {
                    slot.stringValue = name;
                }
            }
        }

        manager.ApplyModifiedProperties();
    }

    [MenuItem(@"Tools/Easy Build System/Package Importer...", priority = -799)]
    public static void Init()
    {
        Logs = null;

        ImportedDemosAddonsPackage = false;
        ImportedXRInteractionToolkitPackage = false;
        ImportedInputSystemPackage = false;
        ImportedInputSystemSupport = false;

        EditorApplication.UnlockReloadAssemblies();

        Installing = false;

        EditorWindow Window = GetWindow(typeof(ImporterEditor), false, "Package Importer", true);

        Window.titleContent.image = EditorGUIUtility.IconContent("d__Popup").image;

        int WindowWidth = 695;
        int WindowHeight = 225;

        Window.position = new Rect((Screen.currentResolution.width - WindowWidth) / 2,
            (Screen.currentResolution.height - WindowHeight - 200) / 2, WindowWidth, WindowHeight);
        Window.minSize = new Vector2(WindowWidth, WindowHeight);
        Window.maxSize = new Vector2(WindowWidth, 900);

        Window.ShowAuxWindow();
    }
}