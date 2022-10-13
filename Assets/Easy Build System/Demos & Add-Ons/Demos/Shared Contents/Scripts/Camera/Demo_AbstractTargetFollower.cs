using UnityEngine;

public abstract class Demo_AbstractTargetFollower : MonoBehaviour
{
    #region Public Enums

    public enum UpdateType
    {
        FixedUpdate,
        LateUpdate,
        ManualUpdate
    }

    #endregion Public Enums

    #region Public Fields

    [Header("Target Follower Settings")]

    [SerializeField] public Transform Target;
    public bool AutoTargetPlayer = true;
    public UpdateType UpdateMode;

    #endregion Public Fields

    #region Private Fields

    protected Rigidbody targetRigidbody;

    #endregion Private Fields

    #region Private Methods

    public virtual void Start()
    {
        if (AutoTargetPlayer)
        {
            FindAndTargetPlayer();
        }
        if (Target == null) return;
        targetRigidbody = Target.GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (AutoTargetPlayer && (Target == null || !Target.gameObject.activeSelf))
        {
            FindAndTargetPlayer();
        }
        if (UpdateMode == UpdateType.FixedUpdate)
        {
            FollowTarget(Time.deltaTime);
        }
    }

    private void LateUpdate()
    {
        if (AutoTargetPlayer && (Target == null || !Target.gameObject.activeSelf))
        {
            FindAndTargetPlayer();
        }
        if (UpdateMode == UpdateType.LateUpdate)
        {
            FollowTarget(Time.deltaTime);
        }
    }

    #endregion Private Methods

    #region Public Methods

    public void ManualUpdate()
    {
        if (AutoTargetPlayer && (Target == null || !Target.gameObject.activeSelf))
        {
            FindAndTargetPlayer();
        }
        if (UpdateMode == UpdateType.ManualUpdate)
        {
            FollowTarget(Time.deltaTime);
        }
    }

    protected abstract void FollowTarget(float deltaTime);

    public void FindAndTargetPlayer()
    {
        var targetObj = GameObject.FindGameObjectWithTag("Player");
        if (targetObj)
        {
            SetTarget(targetObj.transform);
        }
    }

    public virtual void SetTarget(Transform newTransform)
    {
        Target = newTransform;
    }

    #endregion Public Methods
}