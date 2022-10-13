using UnityEngine;
using Random = UnityEngine.Random;

namespace Polyperfect.Common
{
    public class SimpleRotate : PolyMono
    {
        public override string __Usage => "Rotates the object around the prescribed axis.";
        public Vector3 Axis = Vector3.up;
        public float Speed = 120f;
        public bool RandomPhase = true;

        void Start()
        {
            if (RandomPhase)
                transform.Rotate(Axis,Random.Range(0f,360f));
        }
        void Update()
        {
            transform.Rotate(Axis,Speed*Time.deltaTime,Space.Self);
        }
    }
}