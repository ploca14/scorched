using EasyBuildSystem.Features.Scripts.Core.Base.Area;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage;
using EasyBuildSystem.Features.Scripts.Core.Scriptables.Blueprint;
using EasyBuildSystem.Features.Scripts.Core.Scriptables.Collection;
using EasyBuildSystem.Features.Scripts.Extensions;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Editor.Menu
{
    public class MenuItems : ScriptableObject
    {
        #region Methods

        [MenuItem("GameObject/Easy Build System/Components/Scriptable Object(s)/Create New Piece Collection...", false, priority = -200)]
        [MenuItem(@"Tools/Easy Build System/Components/Scriptable Object(s)/Create New Piece Collection...", priority = -200)]
        public static void EditorCreatePieceCollection()
        {
            ScriptableObjectExtension.CreateAsset<PieceCollection>("New Piece Collection...");
        }

        [MenuItem("GameObject/Easy Build System/Components/Scriptable Object(s)/Create New Blueprint Template...", false, priority = -200)]
        [MenuItem(@"Tools/Easy Build System/Components/Scriptable Object(s)/Create New Blueprint Template...", priority = -200)]
        public static void EditorCreateBlueprintData()
        {
            ScriptableObjectExtension.CreateAsset<BlueprintTemplate>("New Blueprint Template...");
        }

        [MenuItem("GameObject/Easy Build System/Components/Code Template(s)/Create New Addon Template...", false, priority = -200)]
        [MenuItem(@"Tools/Easy Build System/Components/Code Template(s)/Create New Addon Template...", isValidateFunction: false, priority = -200)]
        public static void EditorCreateAddonScript()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/Easy Build System/Features/Scripts/Core/Templates/AddonTemplate.txt", "NewAddonTemplate.cs");
        }

        [MenuItem("GameObject/Easy Build System/Components/Code Template(s)/Create New Condition Template...", false, priority = -200)]
        [MenuItem(@"Tools/Easy Build System/Components/Code Template(s)/Create New Condition Template...", isValidateFunction: false, priority = -200)]
        public static void EditorCreateConditionScript()
        {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile("Assets/Easy Build System/Features/Scripts/Core/Templates/ConditionTemplate.txt", "NewConditionTemplate.cs");
        }

        [MenuItem("GameObject/Easy Build System/Components/Create New Build Storage...", false, priority = -499)]
        [MenuItem(@"Tools/Easy Build System/Components/Create New Build Storage...", priority = -499)]
        public static void EditorCreateBuildStorage()
        {
            GameObject Parent = new GameObject("New Build Storage");
            Parent.AddComponent<BuildStorage>();
            SceneHelper.Focus(Parent);
        }


        [MenuItem("GameObject/Easy Build System/Components/Create New Build Manager...", false, priority = -498)]
        [MenuItem(@"Tools/Easy Build System/Components/Create New Build Manager...", priority = -498)]
        public static void EditorCreateBuildManager()
        {
            GameObject Parent = new GameObject("New Build Manager");
            Parent.AddComponent<BuildManager>();
            SceneHelper.Focus(Parent);
        }

        [MenuItem("GameObject/Easy Build System/Components/Create New Area Behaviour...", false, priority = -500)]
        [MenuItem(@"Tools/Easy Build System/Components/Create New Area Behaviour...", priority = -500)]
        public static void EditorCreateArea()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject Child = new GameObject("New Area Behaviour");
                Child.transform.SetParent(Selection.activeGameObject.transform, false);
                Child.AddComponent<AreaBehaviour>();
                SceneHelper.Focus(Child);
            }
            else
            {
                GameObject Parent = new GameObject("New Area Behaviour");
                Parent.AddComponent<AreaBehaviour>();
                SceneHelper.Focus(Parent);
            }
        }

        [MenuItem("GameObject/Easy Build System/Components/Create New Piece Behaviour...", false, priority = -500)]
        [MenuItem(@"Tools/Easy Build System/Components/Create New Piece Behaviour...", priority = -500)]
        public static void EditorCreatePiece()
        {
            if (Selection.gameObjects.Length == 0)
            {
                Debug.LogWarning("<b>Easy Build System</b> : Please select a gameObject to create a new Piece Behaviour.");
                return;
            }

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                if (Selection.gameObjects[i].GetComponentInParent<PieceBehaviour>() != null)
                {
                    Debug.LogError("<b>Easy Build System</b> : Piece Behaviour already exists on this gameObject: " + Selection.gameObjects[i].name);
                    return;
                }
            }

            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                if (Selection.gameObjects[i].GetComponentInParent<PieceBehaviour>() == null)
                {
                    string LocalPath = EditorUtility.SaveFilePanel("Define a saving path for the piece: " + Selection.gameObjects[i].name, "", Selection.gameObjects[i].name + ".prefab", "prefab");

                    if (LocalPath == string.Empty)
                    {
                        return;
                    }

                    try
                    {
                        LocalPath = LocalPath.Substring(LocalPath.LastIndexOf("Assets"));
                    }
                    catch { return; }

                    if (LocalPath != string.Empty)
                    {
                        GameObject Parent = new GameObject(LocalPath);

                        Selection.gameObjects[i].transform.position = new Vector3(0, Selection.gameObjects[i].transform.position.y, 0);

                        Parent.transform.position = Vector3.zero;
                        Parent.transform.rotation = Quaternion.identity;

                        Selection.gameObjects[i].transform.SetParent(Parent.transform, false);

                        PieceBehaviour Temp = Parent.AddComponent<PieceBehaviour>();

                        Temp.Id = (i + 1).ToString();
                        Temp.Name = Selection.gameObjects[i].name;
                        Temp.gameObject.name = Temp.Name;

#if UNITY_2018_3 || UNITY_2019
                        Object AssetPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(Temp.gameObject, LocalPath, InteractionMode.UserAction);
                        EditorGUIUtility.PingObject(AssetPrefab);
                        Parent.name = AssetPrefab.name;
#else

#if UNITY_2019_4_OR_NEWER
                        LocalPath = AssetDatabase.GenerateUniqueAssetPath(LocalPath);
                        GameObject AssetPrefab = PrefabUtility.SaveAsPrefabAssetAndConnect(Temp.gameObject, LocalPath, InteractionMode.UserAction);
                        EditorGUIUtility.PingObject(AssetPrefab);
#else
                        UnityEngine.Object AssetPrefab = PrefabUtility.CreateEmptyPrefab(LocalPath);
                        GameObject Asset = PrefabUtility.ReplacePrefab(Parent, AssetPrefab, ReplacePrefabOptions.ConnectToPrefab);
                        EditorGUIUtility.PingObject(Asset);
#endif
                        AssetDatabase.Refresh();
#endif
                    }
                }
            }

            SceneHelper.Focus(Selection.gameObjects[0]);
        }

        [MenuItem("GameObject/Easy Build System/Components/Create New Socket Behaviour...", false, priority = -500)]
        [MenuItem(@"Tools/Easy Build System/Components/Create New Socket Behaviour...", priority = -500)]
        public static void EditorCreateSocket()
        {
            if (Selection.activeGameObject != null)
            {
                GameObject Child = new GameObject("New Socket Behaviour");
                Child.transform.SetParent(Selection.activeGameObject.transform, false);
                Child.transform.position = Selection.activeGameObject.transform.position;
                Child.AddComponent<SocketBehaviour>();
                SceneHelper.Focus(Child);
            }
            else
            {
                GameObject Parent = new GameObject("New Socket Behaviour");
                Parent.AddComponent<SocketBehaviour>();
                SceneHelper.Focus(Parent);
            }
        }

        [MenuItem(@"Tools/Easy Build System/Documentation...")]
        public static void EditorLinkWeb()
        {
            Application.OpenURL("https://adsstudio12.gitbook.io/easybuildsystem/");
        }

        #endregion Methods
    }
}