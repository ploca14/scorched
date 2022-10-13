using System;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Polyperfect.Crafting.Integration.UGUI
{
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(ItemSlotComponent))]
    public class UGUIItemTransfer : PolyMono
    {
        public override string __Usage => "Allows transferring of items between slots and such by simply clicking.";
        ItemSlotComponent _slot;
        public ItemSlotComponent Slot => _slot;
        public ItemStackEvent OnExtracted, OnInserted;
        public UnityEvent OnBeginHold, OnEndHold;


        public static void AssertInstanceExists()
        {
            if (!Instance)
                throw new Exception($"There must be a {nameof(UGUIItemTransfer)} in the scene to leverage its capabilities");
        }
        
        public static UGUIItemTransfer Instance { get; private set; }
        bool isHolding;
        void Awake()
        {
            Instance = this;
            _slot = GetComponent<ItemSlotComponent>();
        }

        void Update()
        {
            var currentlyHolding = !_slot.Peek().IsDefault();
            if (currentlyHolding != isHolding)
            {
                if (currentlyHolding)
                    OnBeginHold.Invoke();
                else
                    OnEndHold.Invoke();
            }

            isHolding = currentlyHolding;
        }

        public bool HandleSlotClick(GameObject caller, PointerEventData data)
        {
            var slot = _slot;
            var peek = slot.Peek();
            if (peek.IsEmpty())
            {
                var extractable = caller.GetComponent<IExtract<Quantity, ItemStack>>();
                if (extractable != null)
                {
                    var transferer = new SimpleStackTransfer<ItemStack>(extractable, slot);
                    Quantity transferAmount;
                    switch (data.button)
                    {
                        case PointerEventData.InputButton.Left:
                            transferAmount = extractable.Peek().Value;
                            break;
                        case PointerEventData.InputButton.Right:
                            transferAmount = Mathf.CeilToInt(extractable.Peek().Value / 2f);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    if (transferer.CanTransfer(transferAmount))
                    {
                        OnExtracted.Invoke(transferer.TransferPossible(transferAmount));
                        return true;
                    }
                }
            }
            else
            {
                var insertable = caller.GetComponent<IInsert<ItemStack>>();
                if (insertable != null)
                {
                    
                    var transferer = new SimpleStackTransfer<ItemStack>(slot, insertable);
                    switch (data.button)
                    {
                        case PointerEventData.InputButton.Left:
                            if (transferer.CanTransfer(slot.Peek().Value))
                            {
                                var inserted = transferer.TransferPossible(slot.Peek().Value);
                                if (inserted.Value>0)
                                    OnInserted.Invoke(inserted);
                            }
                            else if (insertable is ISlot<Quantity, ItemStack> targetSlot && targetSlot.CanExtract())
                            {
                                var inTarget = targetSlot.ExtractAll();
                                var inMouse = slot.ExtractAll();
                                if (targetSlot.CanInsertCompletely(inMouse) && slot.CanInsertCompletely(inTarget))
                                {
                                    targetSlot.InsertCompletely(inMouse);
                                    slot.InsertCompletely(inTarget);
                                    OnExtracted.Invoke(inTarget);
                                    OnInserted.Invoke(inMouse);
                                }
                            }
                            break;
                        case PointerEventData.InputButton.Right:
                        {
                            var inserted = transferer.TransferPossible(1);
                            if (inserted.Value > 0)
                                OnInserted.Invoke(inserted);
                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            return false;
        }
    }
}