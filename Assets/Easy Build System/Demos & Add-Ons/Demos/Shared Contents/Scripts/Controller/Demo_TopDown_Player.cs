using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using UnityEngine;
using UnityEngine.AI;
#if EBS_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Demo_TopDown_Player : MonoBehaviour
{
    #region Public Fields

    public Transform TopDownCamera;
    public LayerMask MovementLayers;

    #endregion

    #region Private Fields

    private NavMeshAgent Agent;

    #endregion

    #region Private Methods

    private void Awake()
    {
        TopDownCamera.parent = null;

        Agent = GetComponent<NavMeshAgent>();

        BuildEvent.Instance.OnChangedBuildMode.AddListener((BuildMode mode) =>
        {
            if (mode == BuildMode.None)
                Agent.isStopped = false;
            else
                Agent.isStopped = true;
        });
    }

    private void Update()
    {
#if EBS_NEW_INPUT_SYSTEM
        if (Mouse.current.leftButton.IsPressed())
        {
            RaycastHit Hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f)), out Hit, Mathf.Infinity, MovementLayers))
                Agent.destination = Hit.point;
        }
#else
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit Hit;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)), out Hit, Mathf.Infinity, MovementLayers))
                Agent.destination = Hit.point;
        }
#endif

    }

    #endregion
}