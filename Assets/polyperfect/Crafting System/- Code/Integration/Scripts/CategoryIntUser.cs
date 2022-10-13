using System;
using UnityEngine;
using UnityEngine.Events;
using T = System.Int32;

namespace Polyperfect.Crafting.Integration
{
    public class CategoryIntUser : CategoryValueUser<T>
    {
        [SerializeField] TEvent Event;
        public override string __Usage => "Easy usage of integer data from items put in the attached ItemSlotComponent.";

        void OnValidate()
        {
            if (!(Category is BaseCategoryWithData<T>))
                Category = null;
        }

        protected override void SendChange(T value)
        {
            Event.Invoke(value);
        }

        [Serializable]
        class TEvent : UnityEvent<T>
        {
        } //necessary for Unity 2019 compatibility
    }
}