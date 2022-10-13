using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public class Placement_TransformMatcher : PolyMono, IPlacementPostprocessor
    {
        public override string __Usage => "Makes the object's transform follow the placement information.";

        public float SmoothDuration = .1f;
        Vector3 velocity;
        Vector3 forwardDirectionVelocity, upwardDirectionVelocity;
        public void PostprocessPlacement(in PlacementInfo info)
        {
            var trans = transform;

            var intendedForward = info.Rotation*Vector3.forward;
            if (Vector3.Angle(trans.forward, intendedForward) > 179)
                intendedForward = Quaternion.AngleAxis(1, trans.up)*intendedForward;
            var newForward = Vector3.SmoothDamp(trans.forward,intendedForward,ref forwardDirectionVelocity,SmoothDuration);
            var newUp = Vector3.SmoothDamp(trans.up,info.Rotation*Vector3.up,ref upwardDirectionVelocity,SmoothDuration);

            trans.rotation = Quaternion.LookRotation(newForward, newUp);
            trans.position = Vector3.SmoothDamp(trans.position,info.Position,ref velocity,SmoothDuration);
        }
    }
}