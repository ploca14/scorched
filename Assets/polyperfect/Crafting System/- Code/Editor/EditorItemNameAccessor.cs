using System.Collections;
using System.Collections.Generic;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Common.Edit
{
    public class EditorItemNameAccessor : IReadOnlyDictionary<RuntimeID, string>
    {
        readonly Dictionary<RuntimeID, string> names = new Dictionary<RuntimeID, string>();

        public EditorItemNameAccessor()
        {
            foreach (var item in AssetUtility.FindAssetsOfType<BaseObjectWithID>())
            {
                if (names.ContainsKey(item.ID))
                {
                    Debug.LogError($"The ID already exists for item {item.name}. Randomizing the ID. If this happens again after doing something specific, please report a bug.");
                    item.RandomizeID();
                }
                names.Add(item.ID, item.name);
            }
        }

        public bool TryGetValue(RuntimeID id, out string data)
        {
            if (ContainsKey(id))
            {
                data = this[id];
                return true;
            }

            data = default;
            return false;
        }

        public bool ContainsKey(RuntimeID id)
        {
            return names.ContainsKey(id);
        }

        public string this[RuntimeID key] => names[key];
        public IEnumerable<RuntimeID> Keys => names.Keys;
        public IEnumerable<string> Values => names.Values;

        public IEnumerator<KeyValuePair<RuntimeID, string>> GetEnumerator()
        {
            return names.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return names.GetEnumerator();
        }

        public int Count => names.Count;
    }
}