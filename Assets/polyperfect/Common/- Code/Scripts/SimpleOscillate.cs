using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Polyperfect.Common
{
    public class SimpleOscillate : PolyMono
    {
        public override string __Usage => "Moves the object back and forth.";
        [Range(0.01f,10f)] public float Frequency = 1f;
        public bool RandomPhase = true;
        public Vector3 LocalMaxOffset = Vector3.up * .25f;
        float phaseOffset;
        Vector3 lowPosition, highPosition;

        void Start()
        {
            var localPosition = transform.localPosition;
            lowPosition = localPosition - LocalMaxOffset;
            highPosition = localPosition + LocalMaxOffset;
            if (RandomPhase)
                phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            transform.localPosition = Vector3.Lerp(lowPosition, highPosition, .5f + .5f * Mathf.Sin(phaseOffset + Frequency * Time.time));
        }
    }
}