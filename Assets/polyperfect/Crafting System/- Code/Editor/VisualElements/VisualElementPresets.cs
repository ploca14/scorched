using System.Linq;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Integration;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    public static class VisualElementPresets
    {
        public static VisualElement CreateHoverText(VisualElement parent, string name)
        {
            var labelContainer = new VisualElement().SetStretch().CenterContents();
            labelContainer.style.overflow = Overflow.Visible;
            var hoverLabel = new HoverLabel(parent, name).SetHorizontalPadding(12f).SetVerticalPadding(4f).CenterContents();
            hoverLabel.style.flexShrink = 0f;
            hoverLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            hoverLabel.style.backgroundColor = new Color(0f, 0f, 0f, .7f);
            hoverLabel.SetRadius(12f);
            hoverLabel.OnEnter += () => labelContainer.DoubleDelay(() =>
            {
                hoverLabel.style.position = Position.Absolute;
                var measureSize = hoverLabel.MeasureTextSize(hoverLabel.text, 0, VisualElement.MeasureMode.Undefined, 0, VisualElement.MeasureMode.Undefined);
                hoverLabel.style.width = measureSize.x + 16;
            });
            hoverLabel.pickingMode = PickingMode.Ignore;
            hoverLabel.Hide();
            labelContainer.Add(hoverLabel);
            return labelContainer;
        }

        public static MemberGroupControl<BaseCategoryObject> CreateStandardCategoryEditor(BaseObjectWithID obj)
        {
            var categories = AssetUtility.FindAssetsOfType<BaseCategoryObject>().ToArray();
            return new MemberGroupControl<BaseCategoryObject>(
                () => categories.Where(c => c.Criteria(obj) && !c.Contains(obj)),
                () => categories.Where(c => c.Criteria(obj) && c.Contains(obj)),
                c => c.AddMember(obj),
                c => c.RemoveMember(obj),
                (v, collection) =>
                {
                    v.Clear();
                    v.Add(new Label(collection.name).SetWrap());
                },
                (v, collection) =>
                {
                    v.Clear();
                    v.Add(collection.CreateInlineEditor(obj, collection.name).SetWrap());
                },
                f => true);
        }
    }
}