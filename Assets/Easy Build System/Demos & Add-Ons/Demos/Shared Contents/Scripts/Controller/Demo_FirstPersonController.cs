using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Demo_FirstPersonController : MonoBehaviour
{
    #region Public Fields

    [Header("First Person Controller Settings")]
    [HideInInspector]
    public bool IsWalking;
    public float WalkSpeed;
    public float RunSpeed;
    [Range(0f, 1f)] public float RunstepLenghten;
    public float JumpSpeed;
    public float StickToGroundForce;
    public float GravityMultiplier;
    public Demo_MouseLook MouseLook;
    public float StepInterval;
    public AudioClip[] FootstepSounds;
    public AudioClip JumpSound;
    public AudioClip LandSound;

    #endregion Public Fields

    #region Private Fields

    private UnityEngine.Camera Camera;
    private bool Jump;
    private float YRotation;
    private Vector2 Input;
    private Vector3 MoveDir = Vector3.zero;
    private CharacterController CharacterController;
    private CollisionFlags CollisionFlags;
    private bool PreviouslyGrounded;
    private float StepCycle;
    private float NextStep;
    private bool Jumping;
    private AudioSource AudioSource;

    private float ClimbSpeed = 2;
    private bool IsClimbing = false;

    #endregion Private Fields

    #region Private Methods

    private void Start()
    {
        CharacterController = GetComponent<CharacterController>();
        Camera = UnityEngine.Camera.main;
        StepCycle = 0f;
        NextStep = StepCycle / 2f;
        Jumping = false;
        AudioSource = GetComponent<AudioSource>();
        MouseLook.Init(transform, Camera.transform);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        RotateView();

        if (!Jump)
        {
#if EBS_NEW_INPUT_SYSTEM
            Jump = Demo_InputHandler.Instance.player.Jump.triggered;
#else
            Jump = UnityEngine.Input.GetKeyDown(KeyCode.Space);
#endif
        }

        if (!PreviouslyGrounded && CharacterController.isGrounded)
        {
            PlayLandingSound();
            MoveDir.y = 0f;
            Jumping = false;
        }
        if (!CharacterController.isGrounded && !Jumping && PreviouslyGrounded)
        {
            MoveDir.y = 0f;
        }

        PreviouslyGrounded = CharacterController.isGrounded;
    }

    private void PlayLandingSound()
    {
        AudioSource.clip = LandSound;
        AudioSource.Play();
        NextStep = StepCycle + .5f;
    }

    private void FixedUpdate()
    {
        float speed;

        GetInput(out speed);

        Vector3 desiredMove = transform.forward * Input.y + transform.right * Input.x;

        if (IsClimbing)
        {
            desiredMove = Vector3.forward * Input.y;

            MoveDir.z = 0;
            MoveDir.x = 0;
            MoveDir.y = desiredMove.z * ClimbSpeed;

#if EBS_NEW_INPUT_SYSTEM
            if (Demo_InputHandler.Instance.player.Jump.triggered) IsClimbing = false;
#else
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space)) IsClimbing = false;
#endif

            CollisionFlags = CharacterController.Move(MoveDir * Time.fixedDeltaTime);
        }
        else
        {
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, CharacterController.radius, Vector3.down, out hitInfo,
                                CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            MoveDir.x = desiredMove.x * speed;
            MoveDir.z = desiredMove.z * speed;

            if (CharacterController.isGrounded)
            {
                MoveDir.y = -StickToGroundForce;

                if (Jump)
                {
                    MoveDir.y = JumpSpeed;
                    PlayJumpSound();
                    Jump = false;
                    Jumping = true;
                }
            }
            else
            {
                MoveDir += Physics.gravity * GravityMultiplier * Time.fixedDeltaTime;
            }

            CollisionFlags = CharacterController.Move(MoveDir * Time.fixedDeltaTime);

            ProgressStepCycle(speed);
        }
    }

    private void PlayJumpSound()
    {
        AudioSource.clip = JumpSound;
        AudioSource.Play();
    }

    private void ProgressStepCycle(float speed)
    {
        if (CharacterController.velocity.sqrMagnitude > 0 && (Input.x != 0 || Input.y != 0))
        {
            StepCycle += (CharacterController.velocity.magnitude + (speed * (IsWalking ? 1f : RunstepLenghten))) *
                            Time.fixedDeltaTime;
        }

        if (!(StepCycle > NextStep))
        {
            return;
        }

        NextStep = StepCycle + StepInterval;

        PlayFootStepAudio();
    }

    private void PlayFootStepAudio()
    {
        if (!CharacterController.isGrounded)
        {
            return;
        }

        if (FootstepSounds.Length == 0)
            return;

        int Rnd = Random.Range(1, FootstepSounds.Length);

        AudioSource.clip = FootstepSounds[Rnd];
        AudioSource.PlayOneShot(AudioSource.clip);

        FootstepSounds[Rnd] = FootstepSounds[0];
        FootstepSounds[0] = AudioSource.clip;
    }

    private void GetInput(out float speed)
    {
#if EBS_NEW_INPUT_SYSTEM
        float h = Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().x;
        float v = Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().y;
#else
        float h = UnityEngine.Input.GetAxis("Horizontal");
        float v = UnityEngine.Input.GetAxis("Vertical");
#endif

        IsWalking = true;

        speed = IsWalking ? WalkSpeed : RunSpeed;
        Input = new Vector2(h, v);

        if (Input.sqrMagnitude > 1)
        {
            Input.Normalize();
        }
    }

    private void RotateView()
    {
        MouseLook.LookRotation(transform, Camera.transform);
    }

    private void OnTriggerStay(Collider other)
    {
#if EBS_NEW_INPUT_SYSTEM
        if (Demo_InputHandler.Instance.player.Jump.triggered)
        {
            IsClimbing = false;
            return;
        }
#else
        if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
        {
            IsClimbing = false;
            return;
        }
#endif

        if (other.name == "Ladder Trigger")
        {
            if (!IsClimbing)
            {
                IsClimbing = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Ladder Trigger")
        {
            if (!IsClimbing)
            {
                IsClimbing = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.name == "Ladder Trigger")
        {
            IsClimbing = false;
        }
    }

    #endregion Private Methods
}