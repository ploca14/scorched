using UnityEngine;

#if EBS_XR
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
#endif
public class Demo_XRController : MonoBehaviour
{
#if EBS_XR
    public float Speed = 1f;
    public XRNode InputSource;

    public float OffsetHeight = 0.3f;

    public float Gravity = -9.81f;
    private float FallingSpeed;
    public LayerMask GroundLayer;

    private XRRig Rig;
    private Vector2 InputAxis;

    private CharacterController Controller;

    private void Awake()
    {
        Controller = GetComponent<CharacterController>();
        Rig = GetComponent<XRRig>();
    }

    private void Update()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InputDevice Device = InputDevices.GetDeviceAtXRNode(InputSource);
        Device.TryGetFeatureValue(CommonUsages.primary2DAxis, out InputAxis);
    }

    private void FixedUpdate()
    {
        CapsuleFollowHeadset();

        Quaternion HeadYaw = Quaternion.Euler(0, Rig.cameraGameObject.transform.eulerAngles.y, 0);
        Vector3 Direction = HeadYaw * new Vector3(InputAxis.x, 0f, InputAxis.y);
        Controller.Move(Direction * Speed * Time.fixedDeltaTime);

        if (IsGrounded())
            FallingSpeed = 0f;
        else
            FallingSpeed += Gravity * Time.fixedDeltaTime;

        Controller.Move(Vector3.up * FallingSpeed * Time.fixedDeltaTime);
    }

    private void CapsuleFollowHeadset()
    {
        Controller.height = Rig.cameraInRigSpaceHeight + OffsetHeight;
        Vector3 CapsuleCenter = transform.InverseTransformPoint(Rig.cameraGameObject.transform.position);
        Controller.center = new Vector3(CapsuleCenter.x, Controller.height / 2 + Controller.skinWidth, CapsuleCenter.z);
    }

    private bool IsGrounded()
    {
        Vector3 Start = transform.TransformPoint(Controller.center);
        float Length = Controller.center.y + 0.01f;
        return Physics.SphereCast(Start, Controller.radius, Vector3.down, out RaycastHit Hit, Length, GroundLayer);
    }
#endif
}