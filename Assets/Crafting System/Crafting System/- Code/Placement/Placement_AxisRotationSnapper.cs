using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public class Placement_AxisRotationSnapper : PolyMono, IPlacementProcessor
    {
        public override string __Usage => $"An {nameof(IPlacementProcessor)} that snaps to the specified number of increment rotations.";
        public int SnapCount = 4;
        public Vector3 RotationAxis = Vector3.up;
        public string InputAxis = "Mouse ScrollWheel";
        int currentOrientation;


        public void ProcessPlacement(ref PlacementInfo info)
        {
            var axis = Input.GetAxisRaw(InputAxis);
            if (axis > 0)
                currentOrientation++;
            else if (axis < 0)
                currentOrientation--;
            currentOrientation = (currentOrientation % SnapCount + SnapCount) % SnapCount; //make sure there's so shenanigans with negative numbers

            info.Rotation = Quaternion.AngleAxis(currentOrientation*360f/SnapCount,RotationAxis);
        }
    }
}