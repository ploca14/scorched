using System;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using Polyperfect.Crafting.Placement;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ConnectorPort))]
    public class TransferPort : PolyMono
    {
        public override string __Usage => $"Transfers items to the {nameof(IInsert<ItemStack>)} attached to ports connected to this one.";

        [HighlightNull] [SerializeField] ItemSlotComponent ExtractFrom;
        public int TransferAmount = 1;

        [Range(.1f,3f)]
        public float TransferInterval = .5f;

        public bool ErrorOnNonTransferableConnected;
        public ItemStackEvent TransferEvent;
        public UnityEvent OnSourceEmpty, OnDestinationFull;
        ConnectorPort port;
        IInsert<ItemStack> toInsertInto;
        IExtract<Quantity,ItemStack> toExtractFrom;
        SimpleStackTransfer<ItemStack> transferer;
        float time;
        void Awake()
        {
            port = GetComponent<ConnectorPort>();
            port.OnConnected.AddListener(HandleConnected);
            port.OnDisconnected.AddListener(HandleDisconnected);
            toExtractFrom = ExtractFrom;
            port.OnConnected.AddListener(e=>enabled = true);
            port.OnDisconnected.AddListener(()=>enabled = false);
            enabled = false;
        }

        void Update()
        {
            time += Time.deltaTime;
            if (time > TransferInterval)
            {
                time -= TransferInterval;
                var initialSourcePeek = toExtractFrom.Peek();
                var sourceHadAnything = !initialSourcePeek.IsDefault();
                var transferredAmount = transferer?.TransferPossible(TransferAmount)??default;
                var sourceIsNowEmpty = ExtractFrom.Peek().IsDefault();
                var destinationHasAnySpace = toInsertInto.RemainderIfInserted(initialSourcePeek).IsDefault();
                if (!transferredAmount.IsDefault())
                {
                    TransferEvent?.Invoke(transferredAmount);
                }

                if (sourceIsNowEmpty)
                    OnSourceEmpty?.Invoke();
                if (sourceHadAnything && !destinationHasAnySpace)
                {
                    OnDestinationFull?.Invoke();
                }
            } 
        }

        void HandleConnected(IPort connectedTo)
        {
            toInsertInto = connectedTo?.gameObject.GetComponent<IInsert<ItemStack>>();
            if (ErrorOnNonTransferableConnected&&connectedTo!=null&&toInsertInto==null)
                Debug.LogError($"A non transferable was connected to {gameObject}");
            
            transferer = toInsertInto!=null?new SimpleStackTransfer<ItemStack>(toExtractFrom, toInsertInto):null;
        }

        void HandleDisconnected()
        {
            toInsertInto = null;
        }
    }
}