using UnityEngine;

[RequireComponent(typeof(Demo_ThirdPersonController))]
public class Demo_ThirdPersonControls : MonoBehaviour
{
    #region Private Fields

    private Demo_ThirdPersonController Character;
    private Transform Camera;
    private Vector3 CameraForward;
    private Vector3 Direction;
    private bool Jump;

    #endregion Private Fields

    #region Private Methods

    private void Start()
    {
        if (UnityEngine.Camera.main != null)
        {
            Camera = UnityEngine.Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\", for camera-relative controls.");
        }

        Character = GetComponent<Demo_ThirdPersonController>();
    }

    private void Update()
    {
        if (!Jump)
        {
#if EBS_NEW_INPUT_SYSTEM
            Jump = Demo_InputHandler.Instance.player.Jump.triggered;
#else
            Jump = Input.GetKeyDown(KeyCode.Space);
#endif
        }
    }

    private void FixedUpdate()
    {
#if EBS_NEW_INPUT_SYSTEM
        float h = Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().x;
        float v = Demo_InputHandler.Instance.player.Move.ReadValue<Vector2>().y;
#else
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
#endif

        if (Camera != null)
        {
            CameraForward = Vector3.Scale(Camera.forward, new Vector3(1, 0, 1)).normalized;
            Direction = v * CameraForward + h * Camera.right;
        }
        else
        {
            Direction = v * Vector3.forward + h * Vector3.right;
        }

        Character.Move(Direction, false, Jump);
        Jump = false;
    }

#endregion Private Methods
}