using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Polyperfect.Common
{
    public static class GameObjectExtensions
    {
        public static T AddAndDisableComponent<T>(this GameObject that) where T : MonoBehaviour
        {
            var component = that.AddComponent<T>();
            component.enabled = false;
            return component;
        }

        public static T AddOrGetComponent<T>(this GameObject that) where T : Component
        {
            var ret = that.GetComponent<T>();
            if (!ret)
                ret = that.AddComponent<T>();
            return ret;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ClearCache()
        {
            InstantiateLookup.Clear();
        }

        static readonly Dictionary<GameObject, GameObject> InstantiateLookup = new Dictionary<GameObject, GameObject>();
        public static GameObject InstantiateVisualizationSubtractively(this GameObject original)
        {
            Profiler.BeginSample(nameof(InstantiateVisualizationSubtractively));
            if (!InstantiateLookup.ContainsKey(original))
                InstantiateLookup[original] = CreateVisualizationObject(original);
            var inst = Object.Instantiate(InstantiateLookup[original]);
            inst.hideFlags = HideFlags.None;
            inst.SetActive(true);
            Profiler.EndSample();
            return inst;
        }

        static GameObject CreateVisualizationObject(GameObject original)
        {
            Profiler.BeginSample("Creating Cached Visualization");
            var ogActiveState = original.activeSelf;
            original.SetActive(false);
            var inst = Object.Instantiate(original);
            foreach (var item in inst.GetComponentsInChildren<Component>(true).Reverse().ToArray())
            {
                if (!(item is Renderer || item is MeshFilter || item is IIncludeInVisualization || item is Transform || item is Graphic || item is CanvasRenderer))
                    Object.DestroyImmediate(item);
                else
                {
                    if (item is IIncludeInVisualization vis)
                        vis.MarkIsVisualization();
                }
            }
            Object.DontDestroyOnLoad(inst);
            
            inst.hideFlags = HideFlags.HideAndDontSave;
            original.SetActive(ogActiveState);
            
            Profiler.EndSample();
            return inst;
        }
    }
}