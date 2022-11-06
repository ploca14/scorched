using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class ItemGenerator : PolyMono
    {
        public override string __Usage => "Creates and inserts an item into the attached slot regularly.";

        public float CreationInterval = 1f;
        public ObjectItemStack Generated;
        

        float time;

        ItemSlotComponent slot;
        void Start()
        {
            slot = GetComponent<ItemSlotComponent>();
        }

        void Update()
        {
            time += Time.deltaTime;
            if (time > CreationInterval)
            {
                time -= CreationInterval;
                slot.InsertPossible(Generated);
            }
        }
    }
}