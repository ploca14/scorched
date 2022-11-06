using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Polyperfect.Common.Edit
{
    public static class AssetUtility
    {
        public static IEnumerable<T> FindAssetsOfType<T>() where T : Object
        {
            return AssetDatabase
                .FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>);
        }

        public static IEnumerable<object> FindAssetsOfType(Type type)
        {
            return AssetDatabase
                .FindAssets($"t:{type.Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select((path, i) => AssetDatabase.LoadAssetAtPath(path, type));
        }

        public static T CreateAsset<T>(string namePrefix, string directory) where T : ScriptableObject
        {
            return (T) CreateAsset(typeof(T), namePrefix, directory);
        }

        public static Object CreateAsset(Type t, string namePrefix, string directory)
        {
            var obj = ScriptableObject.CreateInstance(t);
            AssetDatabase.CreateAsset(obj, Path.Combine(directory, $"{namePrefix}{GetRandomString()}.asset"));
            AssetDatabase.SaveAssets();
            return obj;
        }

        static string GetRandomString()
        {
            return GUID.Generate().ToString().Substring(0, 8);
        }
        /// <returns>The sequence of indices removed. They are returned in the order removed, which is in reverse.</returns>
        public static List<int> DeleteFromArray(this SerializedProperty prop, Object o)
        {
            var ret = new List<int>();
            for (var i = prop.arraySize-1; i >=0 ; i--)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == o)
                {
                    prop.DeleteArrayElementAtIndex(i);
                    ret.Add(i);
                }
            }

            return ret;
        }
        /// <param name="removeSequence">This should take into account the shifting of indices as elements are deleted.</param>
        public static void DeleteElementSequence(this SerializedProperty prop, IEnumerable<int> removeSequence)
        {
            foreach (var t in removeSequence) 
                prop.DeleteArrayElementAtIndex(t);
        }
    }
}