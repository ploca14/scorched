using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Demo_ThirdPersonController))]
public class Demo_AIThirdPersonController : MonoBehaviour
{
    #region Public Fields

    [Header("AI Third Person Settings")]
    public NavMeshAgent Agent;

    public Demo_ThirdPersonController Controller;

    public Transform Target;

    #endregion Public Fields

    #region Private Methods

    private void Start()
    {
        Agent = GetComponentInChildren<NavMeshAgent>();
        Controller = GetComponent<Demo_ThirdPersonController>();

        Agent.updateRotation = false;
        Agent.updatePosition = true;
    }

    private void Update()
    {
        if (Target != null)
            Agent.SetDestination(Target.position);

        if (Agent.remainingDistance > Agent.stoppingDistance)
            Controller.Move(Agent.desiredVelocity, false, false);
        else
            Controller.Move(Vector3.zero, false, false);
    }

    #endregion Private Methods

    #region Public Methods

    public void SetTarget(Transform target)
    {
        Target = target;
    }

    #endregion Public Methods
}