using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    /// <summary>
    /// A list of ObjectWithQuantities. Concrete for easy editor access.
    /// </summary>
    [Serializable]
    public class QuantityContainer : IList<ObjectItemStack>
    {
        [SerializeField] List<ObjectItemStack> items = new List<ObjectItemStack>();

        public QuantityContainer()
        {
            
        }
        public QuantityContainer(IEnumerable<ObjectItemStack> startingItems)
        {
            items.AddRange(startingItems);
        }

        public IEnumerable<ObjectItemStack> Items => items;

        public IEnumerator<ObjectItemStack> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        public void Add(ObjectItemStack item)
        {
            items.Add(item);
        }

        public void Clear()
        {
            items.Clear();
        }

        public bool Contains(ObjectItemStack item)
        {
            return items.Contains(item);
        }

        public void CopyTo(ObjectItemStack[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(ObjectItemStack item)
        {
            return items.Remove(item);
        }

        public int Count => items.Count;
        public bool IsReadOnly => true;

        public int IndexOf(ObjectItemStack item)
        {
            return items.IndexOf(item);
        }

        public void Insert(int index, ObjectItemStack item)
        {
            items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
        }

        public ObjectItemStack this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }

        public bool Contains(RuntimeID item)
        {
            return Items.Select(i => i.ID).Contains(item);
        }
    }
}