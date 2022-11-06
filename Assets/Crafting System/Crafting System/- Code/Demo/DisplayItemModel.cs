using System.Collections.Generic;
using Polyperfect.Common;
using Polyperfect.Crafting.Integration;
using UnityEngine;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class DisplayItemModel : ItemUserBase
    {
        public override string __Usage => "Creates a prefab based on the specified data for a given item, and sizes it so it fits within the given bounds. Connects to the attached ItemSlotComponent automatically.";
        public float MaxSize = 1f;
        [SerializeField] CategoryWithPrefab DataSource;
        [SerializeField] Transform ModelParent;
        static readonly Dictionary<GameObject, Bounds> prefabBounds = new Dictionary<GameObject, Bounds>();
        public ItemStackEvent OnModelSucceeded = new ItemStackEvent(), OnModelFailed = new ItemStackEvent();
        public bool AutoRemovePhysicsComponents = true;
        GameObject created;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetPrefabBounds() => prefabBounds.Clear();

        void Awake()
        {
            if (!ModelParent)
                ModelParent = transform;
            var itemSlotComponent = GetComponent<ItemSlotComponent>();
            itemSlotComponent.Changed += () => CreateAndDisplay(itemSlotComponent.Peek());
        }

        public void CreateAndDisplay(ItemStack itemStack)
        {
            CleanupExistingModel();

            var accessor = World.GetReadOnlyAccessor<GameObject>(DataSource);
            if (!accessor.TryGetValue(itemStack.ID, out var prefab))
            {
                OnModelFailed.Invoke(itemStack);
                return;
            }

            CreateModel(prefab);
            OnModelSucceeded.Invoke(itemStack);
        }

        void CleanupExistingModel()
        {
            if (created)
                Destroy(created);
            created = null;
        }

        void CreateModel(GameObject prefab)
        {
            var inst = InstantiateAndAutoScaleModel(prefab);

            // if (AutoRemovePhysicsComponents)
            // {
            //     foreach (var item in inst.GetComponentsInChildren<Collider>())
            //         Destroy(item);
            //     foreach (var item in inst.GetComponentsInChildren<Rigidbody>())
            //         Destroy(item);
            // }

            created = inst;
        }

        GameObject InstantiateAndAutoScaleModel(GameObject prefab)
        {
            var inst = AutoRemovePhysicsComponents?prefab.InstantiateVisualizationSubtractively():Instantiate(prefab);

            inst.transform.SetParent(ModelParent,false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.identity;
            
            var size = GetPrefabBounds(prefab).size;
            var max = Mathf.Max(size.x, size.y, size.z);
            var mul = MaxSize / max;

            inst.transform.localScale *= mul;
            return inst;
        }

        Bounds GetPrefabBounds(GameObject prefab)
        {
            if (prefabBounds.TryGetValue(prefab, out var bounds))
                return bounds;
            bounds = new Bounds(Vector3.zero, Vector3.one);
            var first = true;
            foreach (var item in prefab.GetComponentsInChildren<MeshRenderer>())
            {
                if (first)
                {
                    bounds = item.bounds;
                    first = false;
                }

                bounds.Encapsulate(item.bounds);
            }
            prefabBounds[prefab] = bounds;
            return bounds;
        }
    }
}