using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    public class GatherInteractable : BaseInteractable
    {
        public override string __Usage => $"Transfers resources to the inventories of interacting GameObjects. If {nameof(DestroyOnEmpty)} is enabled and the object is empty for a full frame, it will be deleted.";
        readonly Dictionary<GameObject, TransferOverTime> transferers = new Dictionary<GameObject, TransferOverTime>();
        public ItemStackEvent OnTransferred = new ItemStackEvent();
        public int RemoveAmount = 1;
        [SerializeField] bool TransferOverTime = true;
        public float TimeDelay = 1;
        [SerializeField] bool DestroyOnEmpty = true;
        IExtract<Quantity, ItemStack> _source;

        void Start()
        {
            _source = GetComponent<IExtract<Quantity, ItemStack>>();
        }

        bool emptyLastFrame = false;
        void Update()
        {
            if (!DestroyOnEmpty)
                return;

            var isEmpty = _source.Peek().IsDefault();
            if (isEmpty && emptyLastFrame)
                Destroy(gameObject);
            emptyLastFrame = isEmpty;
        }

        public override void BeginInteract(GameObject interactor)
        {
            if (!transferers.ContainsKey(interactor))
            {
                var destination = interactor.GetComponentInChildren<IInsert<ItemStack>>();
                if (TransferOverTime)
                {
                    var transferer = gameObject.AddComponent<TransferOverTime>();
                    transferer.OnTransfer.AddListener(OnTransferred.Invoke);
                    transferer.RemoveAmount = RemoveAmount;
                    transferer.TickDuration = TimeDelay;
                    transferer.Source = _source;
                    transferer.Destination = destination;
                    transferers.Add(interactor, transferer);
                }
            }
            if(!TransferOverTime)
            {
                var destination = interactor.GetComponentInChildren<IInsert<ItemStack>>();
                var peek = _source.Peek(RemoveAmount);
                var remainder = destination.RemainderIfInserted(peek);
                var extracted = _source.ExtractAmount(peek.Value-remainder.Value);
                destination.InsertCompletely(extracted);
                OnTransferred.Invoke(extracted);
            }
            base.BeginInteract(interactor);
            if (!TransferOverTime)
                EndInteract(interactor);
        }

        void OnDestroy()
        {
            foreach (var item in transferers.Keys.ToList())
            {
                EndInteract(item);
            }
        }

        public override void EndInteract(GameObject interactor)
        {
            transferers.TryGetValue(interactor, out var transferer);
            if (!transferer)
            {
                base.EndInteract(interactor);
                return;
            }

            Destroy(transferers[interactor]);
            transferers.Remove(interactor);
            base.EndInteract(interactor);
        }
    }
}