using UnityEngine;

public abstract class Demo_PivotBasedCameraRig : Demo_AbstractTargetFollower
{
    #region Public Fields

    protected Transform Camera;
    protected Transform Pivot;
    protected Vector3 LastTargetPosition;

    #endregion

    #region Public Methods

    protected virtual void Awake()
    {
        Camera = GetComponentInChildren<UnityEngine.Camera>().transform;
        Pivot = Camera.parent;
    }

    #endregion
}