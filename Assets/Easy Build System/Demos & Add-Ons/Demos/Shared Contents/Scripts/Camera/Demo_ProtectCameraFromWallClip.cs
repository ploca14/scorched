using System;
using System.Collections;
using UnityEngine;

public class Demo_ProtectCameraFromWallClip : MonoBehaviour
{
    #region Public Fields

    public float ClipMoveTime = 0.05f;
    public float ReturnTime = 0.4f;
    public float SphereCastRadius = 0.1f;
    public bool VisualiseInEditor;
    public float ClosestDistance = 0.5f;
    public bool Protecting { get; private set; }
    public string DontClipTag = "Player";

    #endregion

    #region Private Fields

    private Transform Camera;
    private Transform Pivot;
    private float OriginalDist;
    private float MoveVelocity;
    private float CurrentDist;
    private Ray RayP = new Ray();
    private RaycastHit[] Hits;
    private RayHitComparer RayHitCompr;

    #endregion

    #region Private Methods

    private void Start()
    {
        Camera = GetComponentInChildren<UnityEngine.Camera>().transform;
        Pivot = Camera.parent;
        OriginalDist = Camera.localPosition.magnitude;
        CurrentDist = OriginalDist;

        RayHitCompr = new RayHitComparer();
    }

    private void LateUpdate()
    {
        float TargetDist = OriginalDist;

        RayP.origin = Pivot.position + Pivot.forward * SphereCastRadius;
        RayP.direction = -Pivot.forward;

        var Cols = Physics.OverlapSphere(RayP.origin, SphereCastRadius);

        bool InitialIntersect = false;
        bool HitSomething = false;

        for (int i = 0; i < Cols.Length; i++)
        {
            if ((!Cols[i].isTrigger) &&
                !(Cols[i].attachedRigidbody != null && Cols[i].attachedRigidbody.CompareTag(DontClipTag)))
            {
                InitialIntersect = true;
                break;
            }
        }

        if (InitialIntersect)
        {
            RayP.origin += Pivot.forward * SphereCastRadius;

            Hits = Physics.RaycastAll(RayP, OriginalDist - SphereCastRadius);
        }
        else
        {
            Hits = Physics.SphereCastAll(RayP, SphereCastRadius, OriginalDist + SphereCastRadius);
        }

        Array.Sort(Hits, RayHitCompr);

        float nearest = Mathf.Infinity;

        for (int i = 0; i < Hits.Length; i++)
        {
            if (Hits[i].distance < nearest && (!Hits[i].collider.isTrigger) &&
                !(Hits[i].collider.attachedRigidbody != null &&
                    Hits[i].collider.attachedRigidbody.CompareTag(DontClipTag)))
            {
                nearest = Hits[i].distance;
                TargetDist = -Pivot.InverseTransformPoint(Hits[i].point).z;
                HitSomething = true;
            }
        }

        if (HitSomething)
        {
            Debug.DrawRay(RayP.origin, -Pivot.forward * (TargetDist + SphereCastRadius), Color.red);
        }

        Protecting = HitSomething;
        CurrentDist = Mathf.SmoothDamp(CurrentDist, TargetDist, ref MoveVelocity,
                                        CurrentDist > TargetDist ? ClipMoveTime : ReturnTime);
        CurrentDist = Mathf.Clamp(CurrentDist, ClosestDistance, OriginalDist);
        Camera.localPosition = -Vector3.forward * CurrentDist;
    }

    #endregion

    public class RayHitComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
        }
    }
}