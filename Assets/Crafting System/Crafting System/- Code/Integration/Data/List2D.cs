using System;
using System.Collections;
using System.Collections.Generic;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    [Serializable]
    public class Serialized2DItemList : List2D<ObjectItemStack>
    {
        public Serialized2DItemList(Vector2Int dims) : base(dims)
        {
        }
    }
/// <summary>
///     A list that has dimensions and can be accessed via 2D index
/// </summary>
/// <typeparam name="T"></typeparam>
    //have to do the long route with IList due to serialization shenanigans in Unity 2019
    [Serializable]
    public class List2D<T> : IList<T>, IIndexed<Vector2Int, T>, IDimensions<Vector2Int>
    {
        [SerializeField] protected List<T> list;
        [SerializeField] protected Vector2Int dimensions; //no Vector2Int again due to serialization shenanigans

        public List2D()
        {
            list = new List<T>();
        }

        public List2D(Vector2Int dims)
        {
            list = new List<T>();
            var count = dims.x * dims.y;
            for (var i = 0; i < count; i++)
                list.Add(default);

            dimensions = new Vector2Int(dims.x, dims.y);
        }

        public Vector2Int Dimensions => new Vector2Int(dimensions.x, dimensions.y);

        public T this[Vector2Int ind]
        {
            get => list[ind.x + ind.y * Dimensions.y];
            set => list[ind.x + ind.y * Dimensions.y] = value;
        }

        public IEnumerable<Vector2Int> Indices => this.AllCoordinates();

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void Add(T item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        public int Count => list.Count;
        public bool IsReadOnly => false;

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get => list[index];
            set => list[index] = value;
        }
    }
}