using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public abstract class BaseObjectWithID : PolyObject, IHasID
    {
        /// <summary>
        ///     Object from which all Crafting System Assets extend by default.
        /// </summary>
        public abstract override string __Usage { get; }

        public RuntimeID ID => id;
        [SerializeField] RuntimeID id;

        public static implicit operator RuntimeID(BaseObjectWithID obj)
        {
            return obj ? obj.ID : default;
        }

        protected virtual void Awake()
        {
            TryInitID();
        }

        protected virtual void OnValidate() 
        {
            TryInitID();
        }

        void TryInitID()
        {
            if (!id.IsDefault()) 
                return;
            
            id = RuntimeID.Random();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        #if UNITY_EDITOR
        [ContextMenu("Randomize ID")]
        public void RandomizeID()
        {
            id = RuntimeID.Random();
            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif
        
    }
}