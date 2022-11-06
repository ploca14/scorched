using System;
using System.Collections;
using UnityEngine;

namespace Polyperfect.Common
{
    /// <summary>
    /// Contains methods for simple tweening to be used via StartCoroutine
    /// </summary>
    public static class TweenExtensions
    {
        /// <summary>
        /// Starts a tweening coroutine on the calling script that oscillates between . 
        /// </summary>
        /// <param name="that">The script to have the coroutine started on.</param>
        /// <param name="duration">Seconds for the tween to take.</param>
        /// <param name="apply">Applies the tween. The value provided will be in the range 0-1.</param>
        /// <param name="onComplete">Invoked after the final apply() call</param>
        public static Coroutine Tween(this MonoBehaviour that, float duration, Action<float> apply, Action onComplete = null)
        {
            return that.StartCoroutine(Tween(duration, apply, onComplete));
        }

        /// <summary>
        /// Starts a tweening coroutine on the calling script that oscillates between . 
        /// </summary>
        /// <param name="that">The script to have the coroutine started on.</param>
        /// <param name="duration">Seconds for one direction of the tween to take.</param>
        /// <param name="apply">Applies the tween. The value provided will be in the range 0-1, and it oscillates back and forth forever.</param>
        public static Coroutine TweenPingPong(this MonoBehaviour that, float duration, Action<float> apply)
        {
            return that.StartCoroutine(TweenPingPong(duration, apply));
        }

        static IEnumerator Tween(float duration,Action<float> apply, Action onComplete)
        {
            var val = 0f;
            var durationMul = 1f / duration;
            while (val < 1f)
            {
                apply(val);
                yield return null;
                val += Time.deltaTime *durationMul;
            }
            apply(1f);
            onComplete?.Invoke();
        }
        static IEnumerator TweenPingPong(float oneWayDuration,Action<float> apply)
        {
            var val = 0f;
            var durationMul = 1f / oneWayDuration;
            while (true)
            {
                apply(Mathf.PingPong(val,1f));
                yield return null;
                val += Time.deltaTime*durationMul;
            }
        }
    }
    
}