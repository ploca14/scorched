using UnityEngine;

namespace Polyperfect.Common
{
    public class SimpleTranslate : PolyMono
    {
        public override string __Usage => "Makes the object move with the specified velocity.";

        public Vector3 Velocity = Vector3.forward;
        public Space Space = Space.Self;

        void Update()
        {
            transform.Translate(Velocity*Time.deltaTime,Space);
        }
    }
}