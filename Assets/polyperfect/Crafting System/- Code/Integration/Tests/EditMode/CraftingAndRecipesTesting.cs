using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration.Tests
{
    public class CraftingAndRecipesTesting
    {
        [Test]
        public void NonConsumingProducerMakesRightQuantity()
        {
            var i1 = new SimpleItem(RuntimeID.Random());
            var i2 = new SimpleItem(RuntimeID.Random());
            var i3 = new SimpleItem(RuntimeID.Random());

            var inventory = new List<ItemStack>();
            inventory.Add(new ItemStack(i1.ID, 64));
            inventory.Add(new ItemStack(i2.ID, 13));

            var requirements = new List<ItemStack>();
            requirements.Add(new ItemStack(i1.ID, 3));
            requirements.Add(new ItemStack(i2.ID, 4));

            var result = new ItemStack(i3.ID, 2);
            var recipe = new SimpleRecipe(requirements, result.MakeEnumerable());
            var satisfier = new AnyPositionItemQuantitySatisfier<ItemStack>();
            var producer = new MultiItemFactory(recipe.Output);
            var output = producer.Create(satisfier.SatisfactionWith(requirements, inventory));
            Assert.AreEqual(output.First().Value.Value, 6);
        }

        [Test]
        public void DefaultItemsAreEqual()
        {
            var i1 = default(ItemStack);
            var i2 = default(ItemStack);

            Assert.IsTrue(i1.Equals(default));
            Assert.IsTrue(i1.Equals(i2));
        }

        [Test]
        public void IsEmptyOnItems()
        {
            var slot = new ItemStackSlot();
            Assert.IsTrue(slot.Peek().IsEmpty());
            slot.InsertCompletely(new ItemStack(RuntimeID.Random(), 0));
            Assert.IsTrue(slot.Peek().IsEmpty());
            slot.InsertCompletely(new ItemStack(RuntimeID.Random(), 1));
            Assert.IsFalse(slot.Peek().IsEmpty());
        }

        [Test]
        public void SlotInsertion()
        {
            var i1 = new SimpleItem(RuntimeID.Random());
            var i2 = new SimpleItem(RuntimeID.Random());

            var s1 = new SimpleItemSlot<SimpleItem>();

            Assert.IsTrue(s1.CanInsertCompletely(i1));
            s1.InsertCompletely(i1);
            Assert.AreEqual(s1.Peek(), i1);
            Assert.IsFalse(s1.CanInsertCompletely(i2));
            Assert.AreEqual(s1.ExtractAll(), i1);
            Assert.IsTrue(s1.CanInsertCompletely(i2));
        }


        [Test]
        public void SimpleItemTransfer()
        {
            var item = new ItemStack(RuntimeID.Random(), 10);

            var slot1 = new ItemStackSlot();
            var slot2 = new ItemStackSlot();

            slot1.InsertCompletely(item);

            var transferA = new SimpleTransferer<ItemStack>(slot1, slot2);
            var transferB = new SimpleTransferer<ItemStack>(slot2, slot1);

            Assert.IsTrue(transferA.CanTransfer());
            Assert.IsFalse(transferB.CanTransfer());
            transferA.Transfer();
            Assert.IsFalse(transferA.CanTransfer());
            Assert.IsTrue(transferB.CanTransfer());
        }

        [Test]
        public void AmountSatisfaction()
        {
            var i1 = new ItemStack(RuntimeID.Random(), 5);
            var i2 = new ItemStack(RuntimeID.Random(), 7);
            var i3 = new ItemStack(i1.ID, 8);
            var i4 = new ItemStack(i1.ID, 3);

            var amountSatisfier = new ValueAndIDSatisfier<Quantity>();
            Assert.IsFalse(amountSatisfier.SatisfactionWith(i1, i2));
            Assert.IsTrue(amountSatisfier.SatisfactionWith(i1, i3));
            Assert.IsFalse(amountSatisfier.SatisfactionWith(i1, i4));
        }

        [Test]
        public void SimpleSatisfaction()
        {
            var i1 = new ItemStack(RuntimeID.Random(), 5);
            var i2 = new ItemStack(RuntimeID.Random(), 7);
            var i3 = new ItemStack(i1.ID, 8);
            var i4 = new ItemStack(i1.ID, 3);

            var satisfier = new MatchesIDSatisfier();
            Assert.IsFalse(satisfier.SatisfactionWith(i1.ID, i2.ID));
            Assert.IsTrue(satisfier.SatisfactionWith(i1.ID, i3.ID));
            Assert.IsTrue(satisfier.SatisfactionWith(i1.ID, i4.ID));
        }

        [Test]
        public void TableComparison()
        {
            var i1 = new SimpleItem(RuntimeID.Random());
            var i2 = new SimpleItem(RuntimeID.Random());
            var i3 = new SimpleItem(RuntimeID.Random());

            var recipeTable = new List2D<IValueAndID<Quantity>>(new Vector2Int(2, 2));
            var inventory1 = new List2D<IValueAndID<Quantity>>(new Vector2Int(3, 3));
            var inventory2 = new List2D<IValueAndID<Quantity>>(new Vector2Int(3, 3));
            var inventory3 = new List2D<IValueAndID<Quantity>>(new Vector2Int(3, 3));
            var inventory4 = new List2D<IValueAndID<Quantity>>(new Vector2Int(3, 3));

            recipeTable[new Vector2Int(1, 1)] = i1.WithAmount(3);
            recipeTable[new Vector2Int(0, 1)] = i2.WithAmount(2);
            recipeTable[new Vector2Int(1, 0)] = i3.WithAmount(4);

            inventory1[new Vector2Int(1, 1)] = i1.WithAmount(4);
            inventory1[new Vector2Int(0, 1)] = i2.WithAmount(5);
            inventory1[new Vector2Int(1, 0)] = i3.WithAmount(6);

            inventory2[new Vector2Int(1, 1)] = i2.WithAmount(4);
            inventory2[new Vector2Int(0, 1)] = i1.WithAmount(5);
            inventory2[new Vector2Int(1, 0)] = i3.WithAmount(6);


            inventory4[new Vector2Int(1, 1)] = i1.WithAmount(9);
            inventory4[new Vector2Int(0, 1)] = i2.WithAmount(9);
            inventory4[new Vector2Int(1, 0)] = i3.WithAmount(16);

            var recipe = new TableRecipe<List2D<IValueAndID<Quantity>>, IValueAndID<Quantity>>(recipeTable, null);

            var satisfier = new ValueAndIDSatisfier<Quantity>();
            Assert.IsTrue(satisfier.SatisfactionWith(recipeTable[new Vector2Int(1, 1)], inventory1[new Vector2Int(1, 1)]));

            var tableSatisfier = new TableSatisfier<List2D<IValueAndID<Quantity>>>();
            Assert.AreEqual(tableSatisfier.SatisfactionWith(recipe.Requirements, inventory1), 1);
            Assert.AreEqual(tableSatisfier.SatisfactionWith(recipe.Requirements, inventory2), 0);
            Assert.AreEqual(tableSatisfier.SatisfactionWith(recipe.Requirements, inventory3), 0);
            Assert.AreEqual(tableSatisfier.SatisfactionWith(recipe.Requirements, inventory4), 3);
        }
    }
}