using System;
using System.Linq;
using NUnit.Framework;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration.Tests
{
    public class CollectionTests
    {
        [Test]
        public void SlottedInventoryTests()
        {
            var slottedInventory = new SlottedInventory<ItemStackSlot>();
            for (var i = 0; i < 4; i++)
                slottedInventory.SlotList.Add(new ItemStackSlot());
            var i1 = new ItemStack(RuntimeID.Random(), 5);
            var i2 = new ItemStack(RuntimeID.Random(), 7);
            var i3 = new ItemStack(RuntimeID.Random(), 3);
            var i4 = new ItemStack(RuntimeID.Random(), 1);
            var i5 = new ItemStack(RuntimeID.Random(), 1);
            slottedInventory.SlotList[2].InsertPossible(i1);
            Assert.AreEqual(slottedInventory[2], i1);
            slottedInventory.Insert(2, i2);
            Assert.AreEqual(slottedInventory[2], i2);
            Assert.AreEqual(slottedInventory[3], i1);
            slottedInventory.Add(i3);
            slottedInventory.Add(i4);
            Assert.AreEqual(4, slottedInventory.SlotList.Count);
            Assert.AreEqual(1, slottedInventory.SlotList.FindIndex(s=>s.Peek().Equals(i4)));
            Assert.Throws<InvalidOperationException>(() => slottedInventory.Add(i5));
            slottedInventory.SlotList[1].ExtractAll();
            Assert.AreEqual(slottedInventory[1], default(ItemStack));
        }

        [Test]
        public void InventoryOpsTesting()
        {
            var i1 = new ItemStack(RuntimeID.Random(), 10);
            var i2 = new ItemStack(RuntimeID.Random(), 7);
            var i3 = new ItemStack(RuntimeID.Random(), 3);
            var i4 = new ItemStack(RuntimeID.Random(), 1);
            var i5 = new ItemStack(RuntimeID.Random(), 1);
            var smallInventory = new SlottedInventory<ItemStackSlotWithCapacity>();
            for (var i= 0; i < 3; i++)
                smallInventory.SlotList.Add(new ItemStackSlotWithCapacity(3));
            var largeInventory = new SlottedInventory<ItemStackSlotWithCapacity>();
            for (var i= 0; i < 3; i++)
                largeInventory.SlotList.Add(new ItemStackSlotWithCapacity(999));


            var firstInsertRemainder = InventoryOps.InsertPossible(i1, smallInventory.Slots);
            Assert.AreEqual(new ItemStack(i1.ID,1),firstInsertRemainder);
            Assert.AreEqual(i2,InventoryOps.InsertPossible(i2,smallInventory.Slots));
            Assert.AreEqual(0,InventoryOps.RemainderAfterInsertCollection(smallInventory.Peek(),largeInventory.SlotList).Count);
            Assert.IsTrue(InventoryOps.InsertPossible(firstInsertRemainder,largeInventory.Slots).IsDefault());
            
            var extracted = InventoryOps.ExtractPossibleFromCollection(smallInventory.Slots, new ItemStack(i1.ID, 999));
            InventoryOps.InsertCompletelyIntoCollection(extracted.MakeEnumerable(),largeInventory.Slots);
            Assert.AreEqual(0,InventoryOps.ExtractPossibleFromCollection(smallInventory.SlotList,i1).Value.Value);
            Assert.IsFalse(InventoryOps.CanCompletelyExtractFromCollection(smallInventory.Peek(),i1));
            Assert.IsTrue(InventoryOps.CanCompletelyExtractFromCollection(largeInventory.Peek(),i1));
            Assert.IsFalse(InventoryOps.CanCompletelyExtractFromCollection(largeInventory.Peek(),i2));
            Assert.IsTrue(largeInventory.SlotList[1].Peek().IsEmpty());
        }
    }
}