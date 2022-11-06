using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    public class ReceiverPort : PolyMono,IInsert<ItemStack>
    {
        public override string __Usage => "Allows receiving of items through the port.";
        IInsert<ItemStack> insertInto;
        [HighlightNull] [SerializeField] ItemSlotComponent InsertInto;

        void Awake() => insertInto = InsertInto;

        public ItemStack RemainderIfInserted(ItemStack toInsert) => insertInto.RemainderIfInserted(toInsert);

        public ItemStack InsertPossible(ItemStack toInsert) => insertInto.InsertPossible(toInsert);
    }
}