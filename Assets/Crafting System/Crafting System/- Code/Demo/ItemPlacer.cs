using System;
using System.Collections;
using System.Collections.Generic;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using Polyperfect.Crafting.Placement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Polyperfect.Crafting.Demo
{
    [RequireComponent(typeof(EquippedSlot))]
    public class ItemPlacer : ItemUserBase
    {
        [Serializable]
        public class PlacedEvent : UnityEvent<GameObject> { }

        public override string __Usage => $"Allows placing items in the scene. If an {nameof(ChangeEquippedUsingAxis)} is attached, it will be disabled on use.";
        [HighlightNull] [SerializeField] GameObject DefaultVessel;
        [HighlightNull] [SerializeField] CategoryWithPrefab PlaceableData;
        [HighlightNull] [SerializeField] CategoryWithPrefab VesselData;
        public UnityEvent OnPlacementActivated, OnPlacementDeactivated;
        public PlacedEvent OnObjectPlaced;
        public event Action<ItemStack, GameObject> OnStackPlacedAsObject; 
        public KeyCode HeightAdjustKey = KeyCode.LeftShift;
        public static readonly object PlacedItemStackKey = new object();
        public static readonly object PlacedGameObjectKey = new object();
        
        EquippedSlot equippedSlot;
        GameObject placing;
        Vector3? verticalPlacementStart;
        RaycastResult? castData;
        void Awake()
        {
            if (!PlaceableData)
            {
                Debug.LogError($"No {nameof(PlaceableData)} has been specified on {gameObject.name}");
                return;
            }
            
            equippedSlot = GetComponent<EquippedSlot>();
            equippedSlot.OnEquippedChange.AddListener(HandleEquippedChange);
            if (gameObject.TryGetComponent(out ChangeEquippedUsingAxis equippedChanger))
            {
                OnPlacementActivated.AddListener(() => equippedChanger.enabled = false);
                OnPlacementDeactivated.AddListener(() => equippedChanger.enabled = true);
            }
        }

        ItemStack currentlyEquipped;

        void HandleEquippedChange(ItemStack stack)
        {
            if (stack.ID != currentlyEquipped.ID)
                DeactivatePlacingMode();
            currentlyEquipped = stack;
        }

        void Update()
        {
            if (castData.HasValue)
            {
                HandlePlaceUpdate(castData.Value);
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (placing)
                    DeactivatePlacingMode();
                else if (World.CategoryContains(PlaceableData.ID, currentlyEquipped.ID))
                    ActivatePlacingMode(currentlyEquipped.ID);
            }
        }

        public void ActivatePlacingMode(RuntimeID itemID)
        {
            placing = CreatePlaceableVisualization(itemID);
            InteractiveCursor.CursorUpdate += HandlePointerUpdate;
            OnPlacementActivated.Invoke();
        }

        public void DeactivatePlacingMode()
        {
            if (placing)
                Destroy(placing);
            InteractiveCursor.CursorUpdate -= HandlePointerUpdate;
            OnPlacementDeactivated.Invoke();
            castData = null;
        }


        void OnDisable()
        {
            if (placing)
                DeactivatePlacingMode();
        }

        public void HandlePointerUpdate(RaycastResult raycastResult)
        {   
            var isPhysicsHit = raycastResult.module is PhysicsRaycaster;
            var isPlacing = isPhysicsHit||verticalPlacementStart.HasValue;
            placing.SetActive(isPlacing);
            if (!isPlacing)
                return;
            castData = raycastResult;
        }

        void HandlePlaceUpdate(RaycastResult raycastResult)
        {
            var placePosition = raycastResult.worldPosition;
            if (verticalPlacementStart.HasValue)
            {
                var forward = Camera.main.transform.forward;
                var plane = new Plane(new Vector3(forward.x, 0f, forward.z).normalized, verticalPlacementStart.Value);
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (!plane.Raycast(ray, out var dist))
                    Debug.LogError("Mouse missed somehow");
                placePosition = verticalPlacementStart.Value;
                placePosition.y = ray.GetPoint(dist).y;
            }

            if (Input.GetKey(HeightAdjustKey))
            {
                if (!verticalPlacementStart.HasValue)
                    verticalPlacementStart = placePosition;
            }
            else
                verticalPlacementStart = null;

            var placementInfo = new PlacementInfo(placePosition, Quaternion.identity);

            foreach (var item in placing.GetComponentsInChildren<IPlacementProcessor>(true))
                item.ProcessPlacement(ref placementInfo);
            foreach (var item in placing.GetComponentsInChildren<IPlacementPostprocessor>(true))
                item.PostprocessPlacement(in placementInfo);

            placementInfo.SetGenericData(PlacedItemStackKey, new ItemStack(equippedSlot.Slot.Peek().ID, 1));
            if (Input.GetMouseButtonUp(0) && placementInfo.IsValid)
            {
                placementInfo.ConfirmPlacement();
                TellPlayerToPlace(placementInfo);
            }
        }

        void TellPlayerToPlace(PlacementInfo info)
        {
            var commandable = GetComponent<ICommandable>();
            var command = new PlaceCommand(info);
            command.OnPlaced += HandlePlacementComplete;
            commandable.IssueCommand(command);
        }

        void HandlePlacementComplete(GameObject go,ItemStack stack)
        {
            var hashSet = new HashSet<IPort>();
            var thing = go.GetComponentsInChildren<IPort>();
            foreach (var item in thing)
                hashSet.Add(item);
            foreach (var item in thing)
            {
                if (!PortManager.IsConnected(item))
                    PortManager.TryConnectToNearest(item, p => !hashSet.Contains(p));
            }

            OnObjectPlaced?.Invoke(go);
            OnStackPlacedAsObject?.Invoke(stack,go);
        }

        
        GameObject CreatePlaceableVisualization(RuntimeID itemID)
        {
            if (!World.GetReadOnlyAccessor<GameObject>(PlaceableData).TryGetValue(itemID, out var data))
                return null;


            World.GetReadOnlyAccessor<GameObject>(VesselData).TryGetValue(itemID, out var vesselObject);
            var vessel = Instantiate(vesselObject ? vesselObject : DefaultVessel);
            var instantiatedObject = data.InstantiateVisualizationSubtractively().transform;
            instantiatedObject.localPosition = Vector3.zero;
            instantiatedObject.localRotation = Quaternion.identity;
            instantiatedObject.SetParent(vessel.transform,false);
            return vessel;
        }
    }
}