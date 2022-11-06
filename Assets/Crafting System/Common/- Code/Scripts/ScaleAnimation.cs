using System;
using UnityEngine;

namespace Polyperfect.Common
{
    public class ScaleAnimation:PolyMono
    {
        public override string __Usage => "Makes an item scale based on an animation curve.";
        [SerializeField] AnimateMode Mode;
        public float Duration = .5f;
        public AnimationCurve Curve = AnimationCurve.EaseInOut(0f,0f,1f,1f);
        Vector3 startingScale;
        public enum AnimateMode
        {
            InOut,
            Continuous
        }
        void Awake()
        {
            startingScale = transform.localScale;
        }

        void OnEnable()
        {
            switch (Mode)
            {
                case AnimateMode.InOut:
                    AnimateIn();
                    break;
                case AnimateMode.Continuous:
                    this.TweenPingPong(Duration,t => transform.localScale = Curve.Evaluate(t) * startingScale);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }                
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        public void AnimateIn()
        {
            StopAllCoroutines();
            this.Tween(Duration,
                t => transform.localScale = Curve.Evaluate(t) * startingScale);
        }

        public void AnimateOut()
        {
            StopAllCoroutines();
            var curStartScale = transform.localScale;
            this.Tween(Duration,
                t => transform.localScale = Curve.Evaluate(1 - t) * curStartScale);
        }
    }
}