using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Extensions
{
    public static class GameObjectExtension
    {
        #region Methods

        public static Rigidbody AddRigibody(this GameObject target, bool useGravity, bool isKinematic, float maxDepenetrationVelocity = 15f, HideFlags flag = HideFlags.HideAndDontSave)
        {
            if (target == null)
            {
                return null;
            }

            if (target.GetComponent<Rigidbody>() != null)
            {
                return target.GetComponent<Rigidbody>();
            }

            Rigidbody Component = target.AddComponent<Rigidbody>();
            Component.maxDepenetrationVelocity = maxDepenetrationVelocity;
            Component.useGravity = useGravity;
            Component.isKinematic = isKinematic;
            Component.hideFlags = flag;

            return Component;
        }

        public static void AddSphereCollider(this GameObject target, float radius, bool isTrigger = true, HideFlags flag = HideFlags.HideAndDontSave)
        {
            if (target == null)
            {
                return;
            }

            if (target.GetComponent<Rigidbody>() != null)
            {
                return;
            }

            SphereCollider Component = target.AddComponent<SphereCollider>();
            Component.radius = radius;
            Component.isTrigger = isTrigger;
            Component.hideFlags = flag;
        }

        public static void AddBoxCollider(this GameObject target, Vector3 size, Vector3 center, bool isTrigger = true, HideFlags flag = HideFlags.HideAndDontSave)
        {
            if (target == null)
            {
                return;
            }

            if (target.GetComponent<Rigidbody>() != null)
            {
                return;
            }

            BoxCollider Component = target.AddComponent<BoxCollider>();
            Component.size = size;
            Component.center = center;
            Component.isTrigger = isTrigger;
            Component.hideFlags = flag;
        }

        #endregion
    }
}