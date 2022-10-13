using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public class Placement_GridSnapper : PolyMono, IPlacementProcessor
    {
        public override string __Usage => $"An {nameof(IPlacementProcessor)} that snaps placement info to the specified grid.";
        public float GridSize = 1f;
        public bool SnapX = true;
        public bool SnapY = true;
        public bool SnapZ = true;
        public void ProcessPlacement(ref PlacementInfo info) => info.Position = SnapToGrid(info.Position);

        Vector3 SnapToGrid(Vector3 pos) => 
            new Vector3(
                SnapX?Mathf.RoundToInt(pos.x / GridSize) * GridSize:pos.x, 
                SnapY?Mathf.RoundToInt(pos.y / GridSize) * GridSize:pos.y, 
                SnapZ?Mathf.RoundToInt(pos.z / GridSize) * GridSize:pos.z);
    }
}