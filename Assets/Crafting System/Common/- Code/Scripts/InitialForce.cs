using UnityEngine;

namespace Polyperfect.Common
{
    [RequireComponent(typeof(Rigidbody))]
    public class InitialForce : PolyMono
    {
        public override string __Usage => "Gives the attached Rigidbody a force on start.";
        public ForceMode Mode = ForceMode.VelocityChange;
        public Vector3 LocalVector = Vector3.forward * 5;

        void Start() => GetComponent<Rigidbody>().AddForce(transform.TransformDirection(LocalVector),Mode);
    }
}