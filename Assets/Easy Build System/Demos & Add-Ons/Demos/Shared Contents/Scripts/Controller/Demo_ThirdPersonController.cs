using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class Demo_ThirdPersonController : MonoBehaviour
{
    #region Public Fields

    [Header("Third Person Controller Settings")]
    public Transform ThirdCameraRoot;

    public float MovingTurnSpeed = 360;

    public float StationaryTurnSpeed = 180;

    public float JumpPower = 12f;

    [Range(1f, 4f)]
    public float GravityMultiplier = 2f;

    public float RunCycleLegOffset = 0.2f;

    public float MoveSpeedMultiplier = 1f;

    public float AnimSpeedMultiplier = 1f;

    public float GroundCheckDistance = 0.1f;

    #endregion Public Fields

    #region Private Fields

    private Rigidbody RigidbodyComponent;

    private Animator AnimatorComponent;

    private bool IsGrounded;

    private float OriginGroundCheckDistance;

    private const float k_Half = 0.5f;

    private float TurnAmount;

    private float ForwardAmount;

    private Vector3 GroundNormal;

    private float CapsuleHeight;

    private Vector3 CapsuleCenter;

    private CapsuleCollider Capsule;

    private bool Crouching;

    #endregion Private Fields

    #region Private Methods

    private void Start()
    {
        AnimatorComponent = GetComponent<Animator>();
        RigidbodyComponent = GetComponent<Rigidbody>();
        Capsule = GetComponent<CapsuleCollider>();
        CapsuleHeight = Capsule.height;
        CapsuleCenter = Capsule.center;

        RigidbodyComponent.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

        OriginGroundCheckDistance = GroundCheckDistance;

        if (ThirdCameraRoot != null)
            ThirdCameraRoot.parent = null;
    }

    private void ScaleCapsuleForCrouching(bool crouch)
    {
        if (IsGrounded && crouch)
        {
            if (Crouching) return;
            Capsule.height = Capsule.height / 2f;
            Capsule.center = Capsule.center / 2f;
            Crouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(RigidbodyComponent.position + Vector3.up * Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = CapsuleHeight - Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                Crouching = true;
                return;
            }
            Capsule.height = CapsuleHeight;
            Capsule.center = CapsuleCenter;
            Crouching = false;
        }
    }

    private void PreventStandingInLowHeadroom()
    {
        if (!Crouching)
        {
            Ray crouchRay = new Ray(RigidbodyComponent.position + Vector3.up * Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = CapsuleHeight - Capsule.radius * k_Half;

            if (Physics.SphereCast(crouchRay, Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                Crouching = true;
        }
    }

    private void UpdateAnimator(Vector3 move)
    {
        AnimatorComponent.SetFloat("Forward", ForwardAmount, 0.1f, Time.deltaTime);
        AnimatorComponent.SetFloat("Turn", TurnAmount, 0.1f, Time.deltaTime);
        AnimatorComponent.SetBool("Crouch", Crouching);
        AnimatorComponent.SetBool("OnGround", IsGrounded);

        if (!IsGrounded)
        {
            AnimatorComponent.SetFloat("Jump", RigidbodyComponent.velocity.y);
        }

        float RunCycle = Mathf.Repeat(AnimatorComponent.GetCurrentAnimatorStateInfo(0).normalizedTime + RunCycleLegOffset, 1);

        float JumpLeg = (RunCycle < k_Half ? 1 : -1) * ForwardAmount;

        if (IsGrounded)
        {
            AnimatorComponent.SetFloat("JumpLeg", JumpLeg);
        }

        if (IsGrounded && move.magnitude > 0)
        {
            AnimatorComponent.speed = AnimSpeedMultiplier;
        }
        else
        {
            AnimatorComponent.speed = 1;
        }
    }

    private void HandleAirborneMovement()
    {
        Vector3 extraGravityForce = (Physics.gravity * GravityMultiplier) - Physics.gravity;
        RigidbodyComponent.AddForce(extraGravityForce);

        GroundCheckDistance = RigidbodyComponent.velocity.y < 0 ? OriginGroundCheckDistance : 0.01f;
    }

    private void HandleGroundedMovement(bool crouch, bool jump)
    {
        if (jump && !crouch && AnimatorComponent.GetCurrentAnimatorStateInfo(0).IsName("Grounded"))
        {
            RigidbodyComponent.velocity = new Vector3(RigidbodyComponent.velocity.x, JumpPower, RigidbodyComponent.velocity.z);
            IsGrounded = false;
            AnimatorComponent.applyRootMotion = false;
            GroundCheckDistance = 0.1f;
        }
    }

    private void ApplyExtraTurnRotation()
    {
        float TurnSpeed = Mathf.Lerp(StationaryTurnSpeed, MovingTurnSpeed, ForwardAmount);

        transform.Rotate(0, TurnAmount * TurnSpeed * Time.deltaTime, 0);
    }

    private void CheckGroundStatus()
    {
        RaycastHit hitInfo;

        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, GroundCheckDistance))
        {
            GroundNormal = hitInfo.normal;
            IsGrounded = true;
            AnimatorComponent.applyRootMotion = true;
        }
        else
        {
            IsGrounded = false;
            GroundNormal = Vector3.up;
            AnimatorComponent.applyRootMotion = false;
        }
    }

    #endregion Private Methods

    #region Public Methods

    public void Move(Vector3 move, bool crouch, bool jump)
    {
        if (move.magnitude > 1f)
            move.Normalize();

        move = transform.InverseTransformDirection(move);

        CheckGroundStatus();

        move = Vector3.ProjectOnPlane(move, GroundNormal);

        TurnAmount = Mathf.Atan2(move.x, move.z);

        ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        if (IsGrounded)
        {
            HandleGroundedMovement(crouch, jump);
        }
        else
        {
            HandleAirborneMovement();
        }

        ScaleCapsuleForCrouching(crouch);

        PreventStandingInLowHeadroom();

        UpdateAnimator(move);
    }

    public void OnAnimatorMove()
    {
        if (IsGrounded && Time.deltaTime > 0)
        {
            Vector3 v = (AnimatorComponent.deltaPosition * MoveSpeedMultiplier) / Time.deltaTime;

            v.y = RigidbodyComponent.velocity.y;
            RigidbodyComponent.velocity = v;
        }
    }

    #endregion Public Methods
}