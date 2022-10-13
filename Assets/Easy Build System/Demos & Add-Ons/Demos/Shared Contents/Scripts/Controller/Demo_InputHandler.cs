using UnityEngine;

public class Demo_InputHandler : MonoBehaviour
{
#if EBS_NEW_INPUT_SYSTEM

    #region Fields

    public static Demo_InputHandler Instance;

    public DemoInputActions.PlayerActions player;

    private DemoInputActions Inputs;

    #endregion

    #region Methods

    private void OnEnable()
    {
        Inputs.Player.Enable();
    }

    private void OnDisable()
    {
        Inputs.Player.Disable();
    }

    private void OnDestroy()
    {
        Inputs.Player.Disable();
    }

    private void Awake()
    {
        Instance = this;

        Inputs = new DemoInputActions();

        player = Inputs.Player;
    }

    #endregion

#endif
}