using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public class TransferOverTime : PolyMono
    {
        public override string __Usage => "Transfers items from one place to another. Extracts from a Slot on this object if available.";
        [SerializeField] float tickSpeed;

        [SerializeField] int removeAmount = 1;

        public bool OnlyDrainIfTransferable;

        [SerializeField] BaseItemStackInventory StartingDestination;
        public ItemStackEvent OnTransfer = new ItemStackEvent();
        float counter;

        public float TickSpeed
        {
            get => tickSpeed;
            set => tickSpeed = value;
        }

        public float TickDuration
        {
            get => 1f / tickSpeed;
            set => tickSpeed = 1f / value;
        }

        public int RemoveAmount
        {
            get => removeAmount;
            set => removeAmount = value;
        }

        public IInsert<ItemStack> Destination { get; set; }
        public IExtract<Quantity,ItemStack> Source { get; set; }

        void Awake()
        {
            Source = GetComponent<ItemSlotComponent>();
            Destination = StartingDestination;
        }

        void Update()
        {
            counter += Time.deltaTime * tickSpeed;
            if (counter < 1f)
                return;
            Tick();
            counter -= 1f;
        }

        void Tick()
        {
            if (Source.IsDefault())
            {
                Debug.LogError("Source was default");
                return;
            }

            if (OnlyDrainIfTransferable)
            {
                if (Destination.IsDefault())
                    return;
                var peek = Source.Peek(RemoveAmount);

                if (Destination.CanInsertCompletely(peek))
                {
                    var extracted = Source.ExtractAmount(RemoveAmount);
                    Destination.InsertCompletely(extracted);
                    OnTransfer.Invoke(extracted);
                }
            }
            else
            {
                var extracted = Source.ExtractAmount(RemoveAmount);
                if (!Destination.IsDefault())
                    Destination.InsertPossible(extracted);
                if (!extracted.IsDefault())
                    OnTransfer.Invoke(extracted);
            }
        }
    }
}