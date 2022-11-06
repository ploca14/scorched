using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEditor;

namespace Polyperfect.Common.Edit
{
    public class EditorItemIconAccessor : IDictionary<RuntimeID, IconData>, IReadOnlyDictionary<RuntimeID, IconData>
    {
        readonly IDictionary<RuntimeID, IconData> lookup = new Dictionary<RuntimeID, IconData>();

        public EditorItemIconAccessor()
        {
            foreach (var item in AssetUtility.FindAssetsOfType<IconsCategory>().SelectMany(i => i.Pairs)) lookup.Add(item.Key, item.Value);
        }

        public bool Remove(RuntimeID key)
        {
            return lookup.Remove(key);
        }

        public bool TryGetValue(RuntimeID id, out IconData data)
        {
            return lookup.TryGetValue(id, out data);
        }

        public void Add(RuntimeID key, IconData value)
        {
            lookup.Add(key, value);
        }

        public bool ContainsKey(RuntimeID id)
        {
            return lookup.ContainsKey(id);
        }

        public IconData this[RuntimeID key]
        {
            get => lookup[key];
            set
            {
                foreach (var iconsRuntimeID in AssetUtility.FindAssetsOfType<IconsCategory>())
                foreach (var item in iconsRuntimeID.ValidMembers)
                    if (item == key)
                    {
                        iconsRuntimeID.SetData(item, value, new SerializedObject(iconsRuntimeID));
                        return;
                    }
            }
        }

        public ICollection<RuntimeID> Keys => lookup.Keys;
        public ICollection<IconData> Values => lookup.Values;

        public IEnumerator<KeyValuePair<RuntimeID, IconData>> GetEnumerator()
        {
            return lookup.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return lookup.GetEnumerator();
        }

        public void Add(KeyValuePair<RuntimeID, IconData> item)
        {
            lookup.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            lookup.Clear();
        }

        public bool Contains(KeyValuePair<RuntimeID, IconData> item)
        {
            return lookup.ContainsKey(item.Key) && lookup[item.Key].Equals(item.Value);
        }

        public void CopyTo(KeyValuePair<RuntimeID, IconData>[] array, int arrayIndex)
        {
            lookup.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<RuntimeID, IconData> item)
        {
            return lookup.Remove(item);
        }

        public int Count => lookup.Count;
        public bool IsReadOnly => false;

        IEnumerable<RuntimeID> IReadOnlyDictionary<RuntimeID, IconData>.Keys => Keys;

        IEnumerable<IconData> IReadOnlyDictionary<RuntimeID, IconData>.Values => Values;
    }
}