using UnityEngine;

namespace Polyperfect.Common
{
    public class RemainUpright : PolyMono
    {
        public override string __Usage => "Ensures the upwards direction is actually up.";

        void Update() => transform.up = Vector3.up;
    }
}