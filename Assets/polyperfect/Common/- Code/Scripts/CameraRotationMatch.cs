using UnityEngine;

namespace Polyperfect.Common
{
    public class CameraRotationMatch : PolyMono
    {
        public override string __Usage => "Makes the Transform match the rotation of the main camera.";
        Transform target;


        void Start()
        {
            var cam = Camera.main;
            if (!cam)
            {
                Debug.LogError("No main cam found");
                enabled = false;
                return;
            }

            target = cam.transform;
        }

        void LateUpdate() => transform.rotation = target.rotation;
    }
}