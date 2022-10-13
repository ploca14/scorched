using System;
using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    public class ConveyorBeltMovement : PolyMono
    {
        public override string __Usage => "Applies physics forces to contacting objects.";
        public float Speed = 2f;
        public float Friction = 1f;
        public Transform PointA, PointB;
        public bool ReverseA, ReverseB;
        void OnCollisionStay(Collision other)
        {
            var contactPoint = other.contacts[0].point;
            var currentVelocity = other.rigidbody.GetPointVelocity(contactPoint);
            var pointAPos = PointA.position;
            var pointBPos = PointB.position;
            var inoutDisplacement = pointBPos - pointAPos;
            var contactDisplacement = Vector3.Project(contactPoint - pointAPos, inoutDisplacement.normalized);

            var directionWeighting = Mathf.Clamp01(contactDisplacement.magnitude/inoutDisplacement.magnitude);
            var pointADir = PointA.forward;
            var pointBDir = PointB.forward;
            var intendedVelocity = Vector3.Slerp(ReverseA?-pointADir:pointADir,ReverseB?-pointBDir:pointBDir,directionWeighting)*Speed;
            var force = (intendedVelocity-currentVelocity)*Friction;
            other.rigidbody.AddForceAtPosition(force,contactPoint,ForceMode.VelocityChange);
        }

    }
}