using Polyperfect.Common;

namespace Polyperfect.Crafting.Integration
{
    public class DualSlot : PolyMono
    {
        public override string __Usage => $"Holds some references for other things to use. The {nameof(GhostSlot)} is typically used for displaying something to be crafted, while the {nameof(RealSlot)} is where the actual items are dealt with.";

        public  ItemSlotComponent GhostSlot;
        public  UGUITransferableItemSlot RealSlot;
        
    }
}