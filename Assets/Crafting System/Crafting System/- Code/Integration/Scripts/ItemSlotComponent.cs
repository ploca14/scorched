using System;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Integration
{
    public class ItemSlotComponent : ItemUserBase, IChangeable, ISlot<Quantity, ItemStack>
    {
        [Obsolete("Readonly has been changed to AllowInsert and AllowExtract.")]
        public bool IsReadonly { get => !AllowExtract; set => AllowExtract = value; }

        public bool AllowInsert = true, AllowExtract = true;
        
        //[SerializeField] bool isLocked;
        [SerializeField] bool instancesOnly;
        //public bool Locked { get => isLocked; set => isLocked = value; }
        public Quantity MaximumCapacity = 64;
        [SerializeField] ObjectItemStack InitialObject;
        [SerializeField] BaseCategoryObject MemberRequirement;
        [SerializeField] CategoryWithInt CapacitySource;
        [SerializeField] UnityEvent OnChanged;
        public override string __Usage => "Simple item slot for any use.";

        protected ItemSlotWithProcessor Slot
        {
            get
            {
                if (slot.IsDefault())
                    InitSlot();
                return slot;
            }
        }

        void InitSlot()
        {
            slot = new ItemSlotWithProcessor();
            
            if (instancesOnly)
                slot.InsertionProcessors.Add(new RequireCategoryConstraint(World,StaticCategories.Archetypes));
            if (MemberRequirement)
                slot.InsertionProcessors.Add(new RequireCategoryConstraint(World,MemberRequirement));
            slot.InsertionProcessors.Add(new FuncProcessor(i =>
            {
                if (CapacitySource)
                    if (World.GetReadOnlyAccessor<int>(CapacitySource).TryGetValue(i.ID, out var capacity)) 
                    {
                        capacity = Mathf.Min(capacity, MaximumCapacity);
                        return new ItemStack(i.ID, Mathf.Min(capacity, i.Value));
                    }

                return new ItemStack(i.ID, Mathf.Min(MaximumCapacity, i.Value));
            })); 
        }
        ItemSlotWithProcessor slot;
        

        public ItemStack Contained => Slot.Peek();
        public RuntimeID RequiredCategory { get; set; }

        void Awake()
        {
            RequiredCategory = MemberRequirement ? MemberRequirement.ID : default;
        }

        protected new void Start()
        {
            base.Start();
            if (!InitialObject?.ID.IsDefault() ?? false)
            {
                var allowInsert = AllowInsert;
                AllowInsert = true;
                InsertPossible(new ItemStack(InitialObject.ID, InitialObject.Value));
                AllowInsert = allowInsert;
            }
        }

        public event PolyChangeEvent Changed;

        public ItemStack RemainderIfInserted(ItemStack toInsert)
        {
            if (!AllowInsert)
                return toInsert;
            return Slot.RemainderIfInserted(toInsert);
        }


        public ItemStack InsertPossible(ItemStack toInsert)
        {
            if (!AllowInsert)
                return toInsert;
            
            var ret = Slot.InsertPossible(toInsert);
            if (!ret.Equals(toInsert))
                FireChange();
            return ret;
        }

        public ItemStack Peek()
        {
            return Slot.Peek();
        }

        public ItemStack Peek(Quantity arg)
        {
            return Slot.Peek(arg);
        }


        public ItemStack ExtractAmount(Quantity arg)
        {
            if (!AllowExtract)
                return default;

            var ret = Slot.ExtractAmount(arg);
            if (!ret.IsDefault())
                FireChange();
            return ret;
        }


        public ItemStack ExtractAll()
        {
            if (!AllowExtract)
                return default;

            var ret = Slot.ExtractAll();
            FireChange();
            return ret;
        }

        public bool CanExtract()
        {
            return AllowExtract&& Slot.CanExtract();
        }

        void FireChange()
        {
            Changed?.Invoke();
            OnChanged?.Invoke();
        }

        public void Discard()
        {
            Slot.ExtractAll();
        }

        public void AddProcessor(IProcessor<ItemStack> processor)
        {
            Slot.InsertionProcessors.Add(processor);
        }

        public void RemoveProcessor(IProcessor<ItemStack> processor)
        {
            Slot.InsertionProcessors.Remove(processor);
        }

        #if UNITY_EDITOR
        //the following is a hacky way of getting the initial item to be reflected in the item slot in the UI
        //unfortunately it would probably slow large projects too much, so disabling for now
        //also doesn't update in realtime
        /*void OnDrawGizmosSelected()
        {
            if (Application.isPlaying && gameObject.scene.isLoaded)
            {
                var filter = $"t:{nameof(BaseObjectWithID)}";
                InitialObject = new ObjectItemStack(
                    AssetDatabase.FindAssets(
                            filter)
                        .Select(AssetDatabase.GUIDToAssetPath)
                        .Select(AssetDatabase.LoadAssetAtPath<BaseObjectWithID>)
                        .SingleOrDefault(s => s.ID == Peek().ID), Peek().Value);
            }
        }*/
        #endif
    }
}