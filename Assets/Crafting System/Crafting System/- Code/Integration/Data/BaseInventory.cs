using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public abstract class BaseInventory : PolyMono
    { 
    }

    public abstract class BaseItemStackInventory : BaseInventory,ISlottedInventory<ISlot<Quantity,ItemStack>>//IInsert<ItemStack>,IInsert<IEnumerable<ItemStack>>,IExtract<IEnumerable<ItemStack>>
    {
        protected void Start()
        {
            TryInit();
        }

        public abstract void TryInit();
        protected abstract ISlottedInventory<ISlot<Quantity, ItemStack>> _slottedInventoryImplementation { get; }
        public IEnumerable<ItemStack> RemainderIfInserted(IEnumerable<ItemStack> toInsert)
        {
            return _slottedInventoryImplementation.RemainderIfInserted(toInsert);
        }

        public IEnumerable<ItemStack> InsertPossible(IEnumerable<ItemStack> toInsert)
        {
            return _slottedInventoryImplementation.InsertPossible(toInsert);
        }

        public ItemStack RemainderIfInserted(ItemStack toInsert)
        {
            return _slottedInventoryImplementation.RemainderIfInserted(toInsert);
        }

        public ItemStack InsertPossible(ItemStack toInsert)
        {
            if (_slottedInventoryImplementation == null)
                Debug.LogError($"Was null on {gameObject}");
            return _slottedInventoryImplementation.InsertPossible(toInsert);
        }

        public IEnumerable<ItemStack> Peek()
        {
            return _slottedInventoryImplementation.Peek();
        }

        public IEnumerable<ItemStack> ExtractAll()
        {
            return _slottedInventoryImplementation.ExtractAll();
        }

        public bool CanExtract()
        {
            return _slottedInventoryImplementation.CanExtract();
        }

        //public ICollection<ISlot<Quantity, ItemStack>> Slots => _slottedInventoryImplementation.Slots;
        public IEnumerable<ISlot<Quantity, ItemStack>> Slots => _slottedInventoryImplementation.Slots;
    }
}