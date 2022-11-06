using UnityEngine;
using UnityEngine.EventSystems;

namespace Polyperfect.Common
{
    public class SurfaceCursor : BaseInstantiatingCursor
    {
        public override string __Usage => "Registers with an InteractiveCursor and allows showing a cursor along a surface";

        protected override void UpdateCursorPosition(RaycastResult obj)
        {
            instantiated.transform.position = obj.worldPosition;
            var intendedUp = obj.worldNormal;
            var intendedForward = forward.forward;
            Vector3.OrthoNormalize(ref intendedUp, ref intendedForward);
            instantiated.transform.rotation = Quaternion.LookRotation(intendedForward, intendedUp);
        }
    }
}