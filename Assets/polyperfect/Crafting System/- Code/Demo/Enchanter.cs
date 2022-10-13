using System.Collections.Generic;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    public class Enchanter : ItemUserBase
    {
        [SerializeField] List<string> Enchantments = new List<string>();
        [SerializeField] ItemSlotComponent Slot;
        public override string __Usage { get; }

        
        public void Enchant()
        {
            //require crystals to be charged to impart an enchantment
            
        }
    }
}