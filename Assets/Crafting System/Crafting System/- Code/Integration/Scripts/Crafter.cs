using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using TInventoryType = Polyperfect.Crafting.Integration.ISlottedInventory<Polyperfect.Crafting.Framework.ISlot<Polyperfect.Crafting.Framework.Quantity,Polyperfect.Crafting.Integration.ItemStack>>; 
namespace Polyperfect.Crafting.Integration
{
    public class Crafter : ItemUserBase
    {
        public override string __Usage => "Exposes methods for crafting to Unity Events, for things like UGUI Buttons";
        [SerializeField] MatchingMode MatchType;

        public UnityEvent 
            OnRecipeChange = new UnityEvent(),
            OnSourceUpdate = new UnityEvent(), 
            OnDestinationUpdate = new UnityEvent();

        public SimpleRecipe Recipe
        {
            get => recipe;
            set
            {
                recipe = value;
                OnRecipeChange.Invoke();
            }
        }


        public TInventoryType Source
        {
            get => source;
            set
            {
                if (source is IChangeable changeableOld) 
                    changeableOld.Changed -= SendSourceUpdated;
                source = value;
                if (source is IChangeable changeableNew) 
                    changeableNew.Changed += SendSourceUpdated;
                OnSourceUpdate.Invoke();
            }
        }

        public TInventoryType Destination
        {
            get => destination;
            set
            {
                if (destination is IChangeable changeableOld) 
                    changeableOld.Changed -= SendDestinationUpdated;
                destination = value;
                if (destination is IChangeable changeableNew) 
                    changeableNew.Changed += SendDestinationUpdated;
                OnDestinationUpdate.Invoke();
            }
        }

        TInventoryType source;
        TInventoryType destination;
        SimpleRecipe recipe;
        void SendDestinationUpdated() => OnDestinationUpdate.Invoke();

        ISatisfier<IEnumerable<IValueAndID<Quantity>>, Quantity> Satisfier { get; set; }


        void Awake()
        {
            switch (MatchType)
            {
                case MatchingMode.AnyPosition:
                    Satisfier = new AnyPositionItemQuantitySatisfier<IValueAndID<Quantity>>();
                    break;
                case MatchingMode.SamePosition:
                    Satisfier = new MatchingPositionSatisfier<IValueAndID<Quantity>>(new QuantityAndIDSatisfier<IValueAndID<Quantity>>(), int.MaxValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void SetSource(GameObject go) => Source = go.GetComponentInChildren<BaseItemStackInventory>();
        public void ClearSource() => Source = null;
        public void SetDestination(GameObject go) => Destination = go.GetComponentInChildren<BaseItemStackInventory>();
        public void ClearDestination() => Destination = null;
        public void SetRecipe(BaseRecipeObject obj) => Recipe = new SimpleRecipe(obj.Ingredients, obj.Outputs);
        public void ClearRecipe() => Recipe = null;

        public void CraftAmount(int craftAmount)
        {
            // if (!CheckIfValid()) 
            //     Debug.LogError("Make sure that Source, Destination, and Recipe are all assigned to the Crafter on " + gameObject.name);
            craftAmount = Mathf.Min(craftAmount, GetMaxCraftAmount(Source.Peek(), Recipe));
            if (craftAmount < 1)
                return;
            var factory = new MultiItemFactory(Recipe.Output);
            var ghostCreation = factory.Create(craftAmount).ToList();

            if (!InventoryOps.CanInsertCollectionCompletely(ghostCreation,Destination.Slots))//(!(Destination.Slots.CanInsertCompletelyIntoCollection(ghostCreation)))//InventoryOps.CanInsertCollectionCompletely(ghostCreation, Destination.GetSlots()))
                return;
            
            if (MatchType == MatchingMode.AnyPosition)
                InventoryOps.ExtractCompletelyFromCollection(Source.Slots, Recipe.Requirements.Multiply(craftAmount));
            else if (MatchType == MatchingMode.SamePosition)
            {
                using (var sourceIterator = Source.Slots.GetEnumerator())
                using (var requirementsIterator = Recipe.Requirements.GetEnumerator())
                    while (requirementsIterator.MoveNext() | sourceIterator.MoveNext())
                    {
                        var currentSource = sourceIterator.Current;
                        var currentRequirement = requirementsIterator.Current;
                        var requiredAmount = currentRequirement.Value*craftAmount;
                        var extracted = currentSource.ExtractAmount(requiredAmount);
                        Assert.AreEqual(extracted.ID,currentRequirement.ID);
                        Assert.AreEqual((int)extracted.Value,requiredAmount);
                    }
            }
            InventoryOps.InsertCompletelyIntoCollection(ghostCreation, Destination.Slots);
        }


        public bool CheckIfValid()
        {
            return !(Source.IsDefault() || Destination.IsDefault() || Satisfier.IsDefault() || Recipe.IsDefault());
        }

        public int GetMaxCraftAmount(IEnumerable<ItemStack> inventory, SimpleRecipe testRecipe)
        {
            if (testRecipe.IsDefault() || inventory==null)
                return 0;
            var satisfier = Satisfier;
            var requirements = testRecipe.Requirements.Cast<IValueAndID<Quantity>>();
            var suppliedItems = inventory.Cast<IValueAndID<Quantity>>();
            var maxCraftAmount = satisfier.SatisfactionWith(requirements, suppliedItems);
            return maxCraftAmount;
        }
        public int GetMaxCraftAmount(SimpleRecipe testRecipe) => GetMaxCraftAmount(Source.Peek(), testRecipe);

        enum MatchingMode
        {
            AnyPosition,
            SamePosition
        }

        void SendSourceUpdated()
        {
            OnSourceUpdate.Invoke();
        }
    }
}