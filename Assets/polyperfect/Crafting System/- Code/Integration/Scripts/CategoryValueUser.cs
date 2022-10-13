using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public abstract class CategoryValueUser<T> : ItemUserBase
    {
        [SerializeField] protected BaseCategoryObject Category;
        public bool SendsOnEmpty = true;
        ItemSlotComponent _slot;

        void Awake()
        {
            _slot = GetComponent<ItemSlotComponent>();
            _slot.Changed += HandleSlotChanged;
        }

        void HandleSlotChanged()
        {
            var poke = _slot.Peek();
            if (poke.IsDefault())
            {
                if (SendsOnEmpty)
                    SendChange(default);
                return;
            }

            SendChange(World.GetReadOnlyAccessor<T>(Category.ID)[poke.ID]);
        }

        protected abstract void SendChange(T value);
    }
}