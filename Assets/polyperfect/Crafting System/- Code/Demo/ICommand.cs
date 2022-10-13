using System;
using Polyperfect.Crafting.Integration;
using Polyperfect.Crafting.Placement;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    public interface ICommand
    {
        event Action OnComplete;
        event Action OnCancel;
        event Action OnDone;

        void Complete();
        void Cancel();
    }

    public abstract class BaseCommand : ICommand
    {
        public event Action OnComplete
        {
            add
            {
                if (finished)
                {
                    Debug.LogError("Trying to subscribe to an already finished command.");
                    value();
                }
                else
                    onComplete += value;
            }
            remove => onComplete -= value;
        }

        event Action onComplete;
        
        
        public event Action OnCancel
        {
            add
            {
                if (finished)
                {
                    Debug.LogError("Trying to subscribe to an already finished command.");
                    value();
                }
                else
                    onCancel += value;
            }
            remove => onCancel -= value;
        }

        event Action onCancel;
        public event Action OnDone
        {
            add
            {
                OnComplete += value;
                OnCancel += value;
            }
            remove
            {
                OnComplete -= value;
                OnCancel -= value;
            }
        }
        protected bool finished { get; private set; } = false;

        public virtual void Complete()
        {
            if (finished)
            {
                Debug.LogError("Trying to complete an already finished task.");
                return;
            }
            onComplete?.Invoke();
            finished = true;
        }

        public virtual void Cancel()
        {
            if (finished)
            {
                
                Debug.LogError("Trying to cancel an already finished task.");
                return;
            }
            onCancel?.Invoke();
            finished = true;
        }

        ~BaseCommand()
        {
            if (!finished)
            {
                Debug.LogError($"{ToString()} was finalized without being cancelled or completed. Make sure to invoke {nameof(Complete)} or {nameof(Cancel)} on all created commands.");
            }
        }
        
    }
    public class InteractCommand : BaseCommand
    {
        public readonly BaseInteractable Interactable;

        public InteractCommand(BaseInteractable interactable)
        {
            Interactable = interactable;
        }
    }
    public class MoveCommand: BaseCommand
    {
        public readonly Vector3 Position;

        public MoveCommand(Vector3 position)
        {
            Position = position;
        }
    }
    public class StopCommand : BaseCommand
    {
        
    }

    public class PlaceCommand : BaseCommand
    {
        public readonly PlacementInfo Info;
        public event Action<GameObject,ItemStack> OnPlaced;
        public PlaceCommand(PlacementInfo info)
        {
            Info = info;
            
            OnComplete +=()=>
            {
                info.TryGetGenericData<ItemStack>(ItemPlacer.PlacedItemStackKey, out var placedStack);
                info.TryGetGenericData<GameObject>(ItemPlacer.PlacedGameObjectKey, out var placedGameObject);
                if (!placedGameObject)
                    Debug.LogError("The placed GameObject was not set before completion of the command.");
                OnPlaced?.Invoke(placedGameObject, placedStack);
            };
        }
    }
}