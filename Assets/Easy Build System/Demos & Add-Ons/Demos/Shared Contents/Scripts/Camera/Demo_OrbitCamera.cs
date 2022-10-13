using UnityEngine;
#if EBS_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Demo_OrbitCamera : MonoBehaviour
{
    public float MoveSpeed = 20f;
    public float RotationSpeed = 10f;
    private float MouseX, MouseY;
    public Vector3 InitialPosition;

    private void Start()
    {
        transform.position = InitialPosition;
    }

    private void Update()
    {
        Movement();
        Rotation();
    }

    private void Movement()
    {
        Vector3 NextPosition = transform.position;
        Vector3 Forward = transform.forward;

        Forward.y = 0;
        Forward.Normalize();

        Vector3 Right = transform.right;

        Right.y = 0;
        Right.Normalize();

#if EBS_NEW_INPUT_SYSTEM
        if (Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().y > 0.5f)
        {
            NextPosition += Forward * MoveSpeed * Time.deltaTime;
        }

        if (Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().y < -0.5f)
        {
            NextPosition -= Forward * MoveSpeed * Time.deltaTime;
        }

        if (Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().x > 0.5f)
        {
            NextPosition += Right * MoveSpeed * Time.deltaTime;
        }

        if (Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().x < -0.5f)
        {
            NextPosition -= Right * MoveSpeed * Time.deltaTime;
        }
#else
        if (Input.GetAxis("Vertical") > 0.5f)
        {
            NextPosition += Forward * MoveSpeed * Time.deltaTime;
        }

        if (Input.GetAxis("Vertical") < -0.5f)
        {
            NextPosition -= Forward * MoveSpeed * Time.deltaTime;
        }

        if (Input.GetAxis("Horizontal") > 0.5f)
        {
            NextPosition += Right * MoveSpeed * Time.deltaTime;
        }

        if (Input.GetAxis("Horizontal") < -0.5f)
        {
            NextPosition -= Right * MoveSpeed * Time.deltaTime;
        }
        #endif

        transform.position = NextPosition;
    }

    private void Rotation()
    {
        #if EBS_NEW_INPUT_SYSTEM
        if (Mouse.current.middleButton.IsPressed())
        {
            MouseX += Demo_InputHandler.Instance.player.LookX.ReadValue<float>() * RotationSpeed;
            MouseY -= Demo_InputHandler.Instance.player.LookY.ReadValue<float>() * RotationSpeed;

            MouseY = Mathf.Clamp(MouseY, -30, 45);
            transform.rotation = Quaternion.Euler(MouseY, MouseX, 0);
        }
        #else
        if (Input.GetKey(KeyCode.Mouse2))
        {
            MouseX += Input.GetAxis("Mouse X") * RotationSpeed;
            MouseY -= Input.GetAxis("Mouse Y") * RotationSpeed;

            MouseY = Mathf.Clamp(MouseY, -30, 45);
            transform.rotation = Quaternion.Euler(MouseY, MouseX, 0);
        }
        #endif
    }
}