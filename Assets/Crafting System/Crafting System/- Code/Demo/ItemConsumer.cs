using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class ItemConsumer : PolyMono
    {
        public override string __Usage => "Periodically consumes items from the attached slot.";

        public float ConsumeInterval = .5f;
        public Quantity ConsumeAmount = 1;
        public bool OnlyFullyConsume = true;
        public UnityEvent OnConsumeBegin, OnConsumeEnd;
        public ItemStackEvent OnConsumed;

        ItemSlotComponent slot;
        bool successful;
        float time;

        void Start()
        {
            slot = GetComponent<ItemSlotComponent>();
        }

        void Update()
        {
            time += Time.deltaTime;
            if ((time <= ConsumeInterval)) 
                return;
            
            time -= ConsumeInterval;
            var extractable = slot.Peek(ConsumeAmount);
            var fullyConsumable = extractable.Value >= ConsumeAmount;
            var shouldConsume = (fullyConsumable) || (!OnlyFullyConsume&&!extractable.IsDefault());
            if (successful && !shouldConsume)
            {
                Debug.LogError("Failure");
                successful = false;
                OnConsumeEnd?.Invoke();
            }
            if (shouldConsume)
            {
                var consumed = slot.ExtractAmount(ConsumeAmount);
                if (!consumed.IsDefault())
                {
                    if (!successful)
                        OnConsumeBegin?.Invoke();
                    successful = true;
                    OnConsumed?.Invoke(consumed);
                }
            }
        }

    }
}