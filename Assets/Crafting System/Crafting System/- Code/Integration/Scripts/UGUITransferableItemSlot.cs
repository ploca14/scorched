using System;
using Polyperfect.Crafting.Integration.UGUI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Polyperfect.Crafting.Integration
{
    public class UGUITransferableItemSlot : ItemSlotComponent,IPointerDownHandler
    {
        public override string __Usage => $"An item slot that can be transferred to and from using the mouse if a {nameof(UGUIItemTransfer)} is present.";

        [SerializeField] CategoryWithInt Consumable_Water;
        [SerializeField] CategoryWithInt Consumable_Food;
        [SerializeField] CategoryWithInt Consumable_Oxygen;
        public event Action OnPreClick;
        public event Action OnPostClick;
        public void OnPointerDown(PointerEventData eventData)
        {
            OnPreClick?.Invoke();
            UGUIItemTransfer.AssertInstanceExists();
            UGUIItemTransfer.Instance.HandleSlotClick(gameObject,eventData, Consumable_Water, Consumable_Food, Consumable_Oxygen);
            OnPostClick?.Invoke();
        }
        
        
    }
}