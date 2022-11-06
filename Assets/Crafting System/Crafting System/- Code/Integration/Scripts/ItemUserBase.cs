using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public abstract class ItemUserBase : PolyMono
    {
        protected IItemWorld World => ItemWorldReference.Instance.World;

        protected virtual void Start()
        {
            if (World == null)
                Debug.LogError("No Item World Found. Please ensure there is a ObjectWorldReference component with an attached World in the scene.");
        }
    }
}