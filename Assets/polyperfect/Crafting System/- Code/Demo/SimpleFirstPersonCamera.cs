using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    public class SimpleFirstPersonCamera : PolyMono
    {
        public override string __Usage => "A simple first-person camera controller. Only rotates if the mouse is locked.";

        [Header("Parameters")]
        public float YawSpeed = 5f;
        public float PitchSpeed = 5f;
        public float PitchMax = 60f;
        
        [Header("Inputs")]
        public string HorizontalAxis = "Mouse X";
        public string VerticalAxis = "Mouse Y";


        void Update()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
                return;
            
            var trans = transform;
            trans.Rotate(Vector3.up, Input.GetAxisRaw(HorizontalAxis) * YawSpeed, Space.World);
            trans.Rotate(Vector3.left, Input.GetAxisRaw(VerticalAxis) * PitchSpeed);
            var euler = trans.eulerAngles;
            trans.eulerAngles = new Vector3(ClampAngle(euler.x, -PitchMax, PitchMax), euler.y, 0);
        }

        float ClampAngle(float value, float min, float max)
        {
            if (value > 180f)
                value -= 360f;
            return Mathf.Clamp(value, min, max);
        }
    }
}