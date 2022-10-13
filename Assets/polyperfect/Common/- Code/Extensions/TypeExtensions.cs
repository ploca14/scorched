using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyperfect.Common
{
    public static class TypeExtensions
    {
        //https://answers.unity.com/questions/1347203/a-smarter-way-to-get-the-type-of-serializedpropert.html
        public static FieldInfo GetFieldViaPath([NotNull] this Type type, string path)
        {
            var parent = type;
            var fi = parent.GetField(path,
                GetBindingFlags());
            var paths = path.Split('.');
            for (var i = 0; i < paths.Length; i++)
            {
                while (true)
                {
                    fi = parent.GetField(paths[i], GetBindingFlags());
                    if (fi == null)
                        if (parent.BaseType != null)
                        {
                            parent = parent.BaseType;
                            continue;
                        }

                    break;
                }

                if (fi == null) return null;

                // there are only two container field type that can be serialized:
                // Array and List<T>
                if (fi.FieldType.IsArray)
                {
                    parent = fi.FieldType.GetElementType();
                    i += 2;
                    continue;
                }

                if (fi.FieldType.IsGenericType)
                {
                    parent = fi.FieldType.GetGenericArguments()[0];
                    i += 2;
                    continue;
                }

                parent = fi.FieldType;
            }

            return fi;
        }

        static BindingFlags GetBindingFlags()
        {
            return BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        }

        public static bool IsHighPriority(this FieldInfo that)
        {
            return that?.GetCustomAttribute<HighPriorityAttribute>() != null;
        }

        public static bool IsLowPriority(this FieldInfo that)
        {
            return that?.GetCustomAttribute<LowPriorityAttribute>() != null;
        }

        public static bool IsHighlightNull(this FieldInfo that)
        {
            return that?.GetCustomAttribute<HighlightNullAttribute>() != null;
        }

        public static bool IsDisableInInspector(this FieldInfo that)
        {
            return that?.GetCustomAttribute<DisableInInspectorAttribute>() != null;
        }

        public static IEnumerable<RequireComponent> GetRequiredTypes(this Type that)
        {
            return that.GetCustomAttributes<RequireComponent>();
        }

#if UNITY_EDITOR
        public static FieldInfo GetFieldInfo(this SerializedProperty that)
        {
            return that.GetTypeOfPropertySO()?.GetFieldViaPath(that.propertyPath);
        }

        public static Type GetTypeOfPropertySO(this SerializedProperty property)
        {
            return property.serializedObject.GetTypeFromSerializedObject();
        }

        public static Type GetTypeFromSerializedObject(this SerializedObject that)
        {
            return that.targetObject.GetType();
        }
#endif
    }
}