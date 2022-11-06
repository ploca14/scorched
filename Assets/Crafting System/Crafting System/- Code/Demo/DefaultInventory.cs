using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(BaseItemStackInventory))]
    public class DefaultInventory : PolyMono
    {
        public override string __Usage => "Fills the target inventory with the provided items on Start";
        public List<ObjectItemStack> ToInsert;

        void Start() => CreateAndInsert();

        public void CreateAndInsert() => GetComponent<BaseItemStackInventory>().InsertPossible(ToInsert.Select(i => (ItemStack)i));
    }
}