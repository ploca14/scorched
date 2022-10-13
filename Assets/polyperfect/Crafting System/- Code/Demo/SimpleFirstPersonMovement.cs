using Polyperfect.Common;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(CharacterController))]
    public class SimpleFirstPersonMovement : PolyMono
    {
        public override string __Usage => "A simple first-person movement controller.";

        public float PrimarySpeed = 5f;
        public float SecondarySpeed = 8f;
        public float JumpSpeed = 12f;
        public float Acceleration = 20f;
        public float StickStrength = 3f;

        public string SecondaryButton = "Fire3";
        public string JumpButton = "Jump";
        public string HorizontalAxis = "Horizontal";
        public string VerticalAxis = "Vertical";


        CharacterController controller;
        Vector3 movementVelocity;
        Vector3 physicsVelocity;
        Vector3 hitNormal;
        bool isGrounded;
        void Start()
        {
            controller = GetComponent<CharacterController>();
            hitNormal = Vector3.up;
        }

        void Update() => HandleMovement();

        void HandleMovement()
        {
            var speed = Input.GetButton(SecondaryButton) ? SecondarySpeed : PrimarySpeed;
            var groundNormal = isGrounded ? hitNormal : Vector3.up;
            var inputVector = Vector3.ClampMagnitude(new Vector3(Input.GetAxisRaw(HorizontalAxis), 0f, Input.GetAxisRaw(VerticalAxis)),1f);
            var excludedUpDirection = Quaternion.LookRotation(Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized);
            physicsVelocity += Physics.gravity * Time.deltaTime;
            movementVelocity = Vector3.MoveTowards(movementVelocity, excludedUpDirection * inputVector * speed, Acceleration * Time.deltaTime);
            if (isGrounded) 
                physicsVelocity = Physics.gravity.normalized * StickStrength;

            if (isGrounded && Input.GetButtonDown(JumpButton))
                physicsVelocity += -Physics.gravity.normalized * (JumpSpeed + StickStrength);

            var appliedVelocity = Quaternion.FromToRotation(Vector3.up, groundNormal) * movementVelocity + physicsVelocity;
            var flags = controller.Move(Time.deltaTime * appliedVelocity);
            isGrounded = (flags & CollisionFlags.Below) != 0;
        }

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (Vector3.Angle(hit.normal, Vector3.up) < controller.slopeLimit)
            {
                hitNormal = hit.normal;
                physicsVelocity = Vector3.zero;
            }
        }
    }
}