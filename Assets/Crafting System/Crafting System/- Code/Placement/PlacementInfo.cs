using System.Collections.Generic;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public class PlacementInfo
    {
        public Vector3 InitialPosition { get; }
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsValid;
        public bool Confirmed { get; private set; }
        readonly Dictionary<object, object> genericData = new Dictionary<object, object>();

        public void SetGenericData<T>(object id, T value) => genericData[id] = value;
        public bool TryGetGenericData<T>(object id,out T result)
        {
            var ret =  genericData.TryGetValue(id, out var obj);
            result = (T)obj;
            return ret;
        }
        public void ConfirmPlacement() => Confirmed = true;

        public PlacementInfo(Vector3 position,Quaternion rotation)
        {
            InitialPosition = position;
            Position = position;
            Rotation = rotation;
            IsValid = true;
        }
    }
}