using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyperfect.Crafting.Integration
{
    [CreateAssetMenu(menuName = "Polyperfect/Item World Fragment")]
    /// <summary>
    ///     A database for storing and accessing item data.
    /// </summary>
    public class ItemWorldFragment : PolyObject
    {
        public List<BaseObjectWithID> Objects = new List<BaseObjectWithID>();
        public override string __Usage => "Holds objects and all their data for use by other things.";
        public IEnumerable<BaseItemObject> ItemObjects => Objects.OfType<BaseItemObject>().Where(o=>o);
        public IEnumerable<BaseRecipeObject> RecipeObjects => Objects.OfType<BaseRecipeObject>().Where(o=>o);
        public IEnumerable<BaseCategoryObject> CategoryObjects => Objects.OfType<BaseCategoryObject>().Where(o=>o);


        protected void OnValidate()
        {
            #if UNITY_EDITOR
            var so = new SerializedObject(this);
            var arr = so.FindProperty(nameof(Objects));
            for (var i = Objects.Count-1; i >= 0; i--)
            {
                var prop = arr.GetArrayElementAtIndex(i);
                if (!prop.objectReferenceValue) 
                    arr.DeleteArrayElementAtIndex(i);
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(this);
            #endif
        }
    }
}