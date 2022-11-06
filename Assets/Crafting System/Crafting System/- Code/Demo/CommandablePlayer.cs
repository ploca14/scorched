using System;
using System.Text;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using Polyperfect.Crafting.Placement;
using UnityEngine;
using UnityEngine.Events;


namespace Polyperfect.Crafting.Demo
{
    public interface ICommandablePlayer : ICommandable
    {
    }

    [RequireComponent(typeof(EquippedSlot))]
    public abstract class CommandablePlayer : ItemUserBase, ICommandablePlayer
    {
        [Serializable]
        public class InteractEvent : UnityEvent<BaseInteractable>
        {
        }

        public InteractEvent OnStartInteract;
        public UnityEvent OnStopInteract;
        [HighlightNull] public CategoryWithPrefab PlaceableData;
        bool interactingLastFrame;
        EquippedSlot equippedSlot;
        Action<GameObject> onPlaceComplete;
        protected BaseInteractable interactTarget;
        public abstract void IssueCommand(ICommand command);

        protected override void Start()
        {
            base.Start();
            equippedSlot = GetComponent<EquippedSlot>();
        }

        protected void Update()
        {
            if (interactingLastFrame && !interactTarget)
            {
                StopInteracting();
            }
        }
        protected void InteractWith(BaseInteractable target)
        {
            var earlyOut = target == interactTarget;
            StopInteracting();
            if (earlyOut)
                return;
            
            interactTarget = target;
            interactTarget.BeginInteract(gameObject);
            interactingLastFrame = true;
            OnStartInteract?.Invoke(target);
        }
        
        protected void StopInteracting()
        {
            if (interactTarget)
                interactTarget.EndInteract(gameObject);
            interactTarget = null;
            interactingLastFrame = false;
            OnStopInteract?.Invoke();
        }
        protected GameObject ExecutePlace(PlacementInfo info)
        {
            var inSlot = equippedSlot.Slot?.Peek(1)??default;
            info.TryGetGenericData<ItemStack>(ItemPlacer.PlacedItemStackKey, out var inPlacementData);
            if (inSlot.ID != inPlacementData.ID)
                return null;
            var pulled = equippedSlot.Slot.ExtractAmount(1);
            if (pulled.IsDefault())
                return null;
            
            var prefabAccessor = World.GetReadOnlyAccessor<GameObject>(PlaceableData.ID);
            if (!prefabAccessor.TryGetValue(pulled.ID, out var toPlace))
                return null;
            
            var instantiated = Instantiate(toPlace, info.Position, info.Rotation);
            info.SetGenericData(ItemPlacer.PlacedGameObjectKey,instantiated);
            foreach (var item in instantiated.GetComponentsInChildren<IPlacementProcessor>(true))
                item.ProcessPlacement(ref info);
            foreach (var item in instantiated.GetComponentsInChildren<IPlacementPostprocessor>(true))
                item.PostprocessPlacement(in info);
            onPlaceComplete?.Invoke(instantiated);
            return instantiated;
        }
    }
}