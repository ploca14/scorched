using System.Collections.Generic;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Extensions
{
    public static class PhysicExtension
    {
        #region Fields

        public const int MAX_ALLOC_COUNT = 256;

        #endregion

        #region Methods

        public static void SetLayerRecursively(this GameObject target, LayerMask layer)
        {
            if (target == null)
            {
                return;
            }

            target.layer = ToLayer(layer.value);

            for (int i = 0; i < target.transform.childCount; i++)
            {
                if (target.transform.GetChild(i) == null)
                {
                    continue;
                }

                SetLayerRecursively(target.transform.GetChild(i).gameObject, layer);
            }
        }

        public static int ToLayer(int bitmask)
        {
            int result = bitmask > 0 ? 0 : 31;

            while (bitmask > 1)
            {
                int v = bitmask >> 1;
                bitmask = v;

                result++;
            }

            return result;
        }

        private static Collider[] sphereColliders = new Collider[MAX_ALLOC_COUNT];
        public static T[] GetNeighborsTypeBySphere<T>(Vector3 position, float size, LayerMask layer, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
        {
            sphereColliders = new Collider[MAX_ALLOC_COUNT];
            int ColliderCount = Physics.OverlapSphereNonAlloc(position, size, sphereColliders, layer, query);

            List<T> types = new List<T>();

            for (int i = 0; i < ColliderCount; i++)
            {
                T type = sphereColliders[i].GetComponentInParent<T>();

                if (type != null)
                {
                    if (type is T)
                    {
                        if (!types.Contains(type))
                        {
                            types.Add(type);
                        }
                    }
                }
            }

            return types.ToArray();
        }

        private static Collider[] boxColliders = new Collider[MAX_ALLOC_COUNT];
        public static T[] GetNeighborsTypeByBox<T>(Vector3 position, Vector3 size, Quaternion rotation, LayerMask layer, QueryTriggerInteraction query = QueryTriggerInteraction.UseGlobal)
        {
            bool initQueries = Physics.queriesHitTriggers;

            Physics.queriesHitTriggers = true;

            boxColliders = new Collider[MAX_ALLOC_COUNT];

            int colliderCount = Physics.OverlapBoxNonAlloc(position, size, boxColliders, rotation, layer, query);

            Physics.queriesHitTriggers = initQueries;

            List<T> types = new List<T>();

            for (int i = 0; i < colliderCount; i++)
            {
                T type = boxColliders[i].GetComponentInParent<T>();

                if (type != null)
                {
                    if (type is T)
                    {
                        if (!types.Contains(type))
                        {
                            types.Add(type);
                        }
                    }
                }
            }

            return types.ToArray();
        }

        #endregion
    }
}