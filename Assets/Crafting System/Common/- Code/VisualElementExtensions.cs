using System;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif

namespace Polyperfect.Common
{
    public static class VisualElementExtensions
    {
        public static void SetRadius(this VisualElement that, float radius)
        {
            that.style.borderBottomLeftRadius = radius;
            that.style.borderBottomRightRadius = radius;
            that.style.borderTopLeftRadius = radius;
            that.style.borderTopRightRadius = radius;
        }

        public static T SetPadding<T>(this T that, float padding) where T : VisualElement
        {
            that.style.paddingBottom = padding;
            that.style.paddingLeft = padding;
            that.style.paddingRight = padding;
            that.style.paddingTop = padding;
            return that;
        }

        public static T SetHorizontalPadding<T>(this T that, float padding) where T : VisualElement
        {
            that.style.paddingLeft = padding;
            that.style.paddingRight = padding;
            return that;
        }

        public static T SetVerticalPadding<T>(this T that, float padding) where T : VisualElement
        {
            that.style.paddingBottom = padding;
            that.style.paddingTop = padding;
            return that;
        }

        public static void SetMargin(this VisualElement that, float margin)
        {
            that.style.marginBottom = margin;
            that.style.marginLeft = margin;
            that.style.marginRight = margin;
            that.style.marginTop = margin;
        }

        public static void SetBorderWidth(this VisualElement that, float width)
        {
            that.style.borderLeftWidth = width;
            that.style.borderRightWidth = width;
            that.style.borderTopWidth = width;
            that.style.borderBottomWidth = width;
        }

        public static void SetBorderColor(this VisualElement that, Color color)
        {
            that.style.borderLeftColor = color;
            that.style.borderRightColor = color;
            that.style.borderTopColor = color;
            that.style.borderBottomColor = color;
        }

        public static T SetGrow<T>(this T that, float amount = 1f) where T : VisualElement
        {
            that.style.flexGrow = amount;
            return that;
        }

        public static T SetStretch<T>(this T that) where T : VisualElement
        {
            that.StretchToParentSize();
            return that;
        }

        public static T CenterContents<T>(this T that) where T : VisualElement
        {
            that.style.alignItems = Align.Center;
            that.style.justifyContent = Justify.Center;
            that.style.unityTextAlign = TextAnchor.MiddleCenter;
            return that;
        }

        public static VisualElement SetRow(this VisualElement that)
        {
            that.style.flexDirection = FlexDirection.Row;
            return that;
        }

        public static T SetWrap<T>(this T that) where T : VisualElement
        {
            that.style.whiteSpace = WhiteSpace.Normal;
            return that;
        }

        public static VisualElement SetColumn(this VisualElement that)
        {
            that.style.flexDirection = FlexDirection.Column;
            return that;
        }

        public static void Show(this VisualElement that)
        {
            that.style.display = DisplayStyle.Flex;
        }

        public static void Hide(this VisualElement that)
        {
            that.style.display = DisplayStyle.None;
        }

        public static void DisplayIf(this VisualElement that, bool b)
        {
            if (b)
                that.Show();
            else
                that.Hide();
        }


        public static T QP<T>(this VisualElement that) where T : class
        {
            var tested = that;
            while (true)
                switch (tested)
                {
                    case null:
                        return null;
                    case T ret:
                        return ret;
                    default:
                        tested = tested.parent;
                        break;
                }
        }

        #if UNITY_EDITOR
        public static VisualElement UpdatePropertyField(this VisualElement that, SerializedProperty property)
        {
            var propertyField = that.QP<PropertyField>();
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
            propertyField?.Unbind();
            propertyField?.Bind(property.serializedObject);
            return propertyField;
        }

        public static void HideLabel(this PropertyField that)
        {
            that.schedule.DoubleDelay(() =>
            {
                try
                {
                    that.ElementAt(0).ElementAt(0).Hide();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Something went wrong when processing {that}: {e}");
                }
            });
        }
        #endif
        
        public static IVisualElementScheduledItem DoubleDelay(this IVisualElementScheduler that, Action updateEvent)
        {
            return that.Execute(() => that.Execute(updateEvent));
        }
    }
}