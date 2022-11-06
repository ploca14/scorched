using System;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Polyperfect.Crafting.Integration
{
    public class ItemGameObjectCreator : PolyMono
    {
        public override string __Usage => $"Lets you spawn objects that can have items inserted.\nThe spawned object should usually have a script that implements {nameof(IInsert<ItemStack>)}, such as an {nameof(ItemSlotComponent)}.\nIf there is an {nameof(ItemSlotComponent)} attached to this GameObject, it will be connected and extracted from automatically.";
        public Transform SpawnAtTransform;
        public GameObject ToSpawn;

        [SerializeField]
        bool AutoAttachToSlot;
        ItemSlotComponent _attachedSlot;
        void Awake()
        {
            if (!SpawnAtTransform)
            {
                SpawnAtTransform = transform;
                while (SpawnAtTransform is RectTransform)
                {
                    if (!SpawnAtTransform)
                        break;
                    SpawnAtTransform = SpawnAtTransform.parent;
                }

                if (!SpawnAtTransform)
                    SpawnAtTransform = transform;
            }
        }

        void Start()
        {
            if (TryGetComponent(out _attachedSlot)&&AutoAttachToSlot)
            {
                _attachedSlot.Changed += HandleAttachedSlotChanged;
            }
        }

        void HandleAttachedSlotChanged()
        {
            if (_attachedSlot.Peek().IsDefault())
                return;
            var pulled = _attachedSlot.ExtractAll();
            SpawnAndInsertItem(pulled);
        }

        public void SpawnAndInsertItem(ItemStack stack)
        {
            if (!gameObject.scene.isLoaded)
                return;

            var go = Instantiate(ToSpawn,SpawnAtTransform.position,SpawnAtTransform.rotation);
             var spawnedSlot = go.GetComponentInChildren<IInsert<ItemStack>>();
             spawnedSlot?.InsertPossible(stack);
        }

        public void SpawnFromAttachedSlot()
        {
            if (!gameObject.scene.isLoaded)
                return;
            if (!_attachedSlot)
            {
                Debug.LogError($"No attached slot registered on {gameObject.name}.");
                return;
            }
            var pulled = _attachedSlot.ExtractAll();
            SpawnAndInsertItem(pulled);
        }
    }
}