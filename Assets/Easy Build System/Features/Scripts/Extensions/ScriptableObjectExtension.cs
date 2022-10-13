#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Extensions
{
    public static class ScriptableObjectExtension
    {
        #region Methods 

        private class EndNameEdit : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
            }
        }

        public static T CreateAsset<T>(string name, bool select = true) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = "Assets/" + name + ".asset";

            AssetDatabase.CreateAsset(asset, path);

            AssetDatabase.SaveAssets();

            if (select)
            {
                EditorUtility.FocusProjectWindow();

                Selection.activeObject = asset;

                EditorGUIUtility.PingObject(asset);
            }

            return asset;
        }

        public static T[] GetAllInstances<T>() where T : ScriptableObject
        {
            string[] Guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
            T[] Result = new T[Guids.Length];

            for (int i = 0; i < Guids.Length; i++)
            {
                string Path = AssetDatabase.GUIDToAssetPath(Guids[i]);
                Result[i] = AssetDatabase.LoadAssetAtPath<T>(Path);
            }

            return Result;
        }

        #endregion
    }
}
#endif