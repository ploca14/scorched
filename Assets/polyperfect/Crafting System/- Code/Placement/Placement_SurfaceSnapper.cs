using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public class Placement_SurfaceSnapper : PolyMono, IPlacementProcessor
    {
        public override string __Usage => $"An {nameof(IPlacementProcessor)} that snaps the item vertically during placement processing using a raycast in the direction of gravity.";
        public float SnapHeight = 2f;
        public LayerMask Mask = ~0;
        public void ProcessPlacement(ref PlacementInfo info) => info.Position = SnapToSurface(info.Position);
        Vector3 SnapToSurface(Vector3 pos) => !Physics.Raycast(new Ray(pos - Physics.gravity.normalized*SnapHeight, Vector3.down), out var hit, SnapHeight*2f, Mask,QueryTriggerInteraction.Ignore) ? pos : hit.point;
    }
}