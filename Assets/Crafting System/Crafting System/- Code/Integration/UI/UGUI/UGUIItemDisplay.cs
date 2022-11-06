using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Polyperfect.Crafting.Integration.UGUI
{
    [RequireComponent(typeof(ItemSlotComponent))]
    public class UGUIItemDisplay : ItemUserBase
    {
        public Text NameLabel;
        public Text QuantityLabel;
        [FormerlySerializedAs("Icon")] public Image ImageComponent;
        public IconsCategory Icons;
        public string QuantityDisplayFormat = "{0}"; 
        ItemSlotComponent itemSlot;

        public override string __Usage => $"Shows the icon, name, and quantity of the item stack in the attached {nameof(ItemSlotComponent)}.";

        protected new void Start()
        {
            base.Start();
            if (!Icons) 
                Debug.LogError("An Icons category should be assigned within the script as a default reference.");
            itemSlot = GetComponent<ItemSlotComponent>();
            itemSlot.Changed += HandleChange;

            HandleChange();
        }

        void HandleChange()
        {
            var hasItem = !itemSlot.Contained.ID.IsDefault();
            if (NameLabel) 
                NameLabel.text = hasItem ? World.GetName(itemSlot.Contained.ID) : "";
            if (QuantityLabel)
            {
                var showNumber = itemSlot.Contained.Value > 0;
                if (showNumber)
                    QuantityLabel.text = string.Format(QuantityDisplayFormat,itemSlot.Contained.Value);

                QuantityLabel.enabled = showNumber;
            }

            if (ImageComponent)
            {
                var world = ItemWorldReference.Instance.World;
                var accessor = world.GetReadOnlyAccessor<IconData>(Icons);
                ImageComponent.sprite = hasItem ? accessor.GetDataOrDefault(itemSlot.Contained.ID).Icon : null;
                ImageComponent.enabled = ImageComponent.sprite;
            }
        }
    }
}