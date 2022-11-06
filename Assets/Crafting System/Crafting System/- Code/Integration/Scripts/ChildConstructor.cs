using System;
using System.Collections.Generic;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration.UGUI
{
    public class ChildConstructor : PolyMono
    {
        public GameObject Prefab;
        public bool ClearOnConstruct = true;
        public bool BringExistingToFront = true;
        public override string __Usage => "Creates children and invokes a bind on all of them. Destroys all children on construct if ClearOnConstruct is enabled. Optionally brings the existing transforms to the front after construction.";
        public IReadOnlyList<GameObject> Constructed => constructed;
        readonly List<GameObject> constructed = new List<GameObject>();
        readonly List<Transform> existing = new List<Transform>();

        void Awake()
        {
            for (var i = 0; i < transform.childCount; i++) 
                existing.Add(transform.GetChild(i));
        }

        
        public void Construct<T>(IEnumerable<T> items, Action<GameObject, T> bind)
        {
            if (ClearOnConstruct)
                ClearConstructed();
            
            foreach (var item in items)
            {
                var go = Instantiate(Prefab, transform);
                constructed.Add(go);
                bind(go, item);
            }

            if (!BringExistingToFront) 
                return;
            
            foreach (var item in existing) 
                item.SetAsLastSibling();
        }

        public void ClearConstructed()
        {
            foreach (var item in constructed)
            {
                Destroy(item);
            }
            constructed.Clear();
        }
    }

    public static class ChildConstructorExtensions
    {
        /// <summary>
        /// Convenience function for a common operation in the pack.
        /// </summary>
        public static void ConstructAndInsertItems(this ChildConstructor that, IEnumerable<ItemStack> items)
        {
            that.Construct(items, (entry, stack) =>
            {
                entry.GetComponent<IInsert<ItemStack>>().InsertPossible(stack);
            });
        }
    }
    
}