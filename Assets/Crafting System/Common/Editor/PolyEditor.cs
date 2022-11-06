using System.Text;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyperfect.Common.Edit
{
    //derived from https://forum.unity.com/threads/property-drawers.595369/#post-5118800
    [CustomEditor(typeof(PolyMono), true, isFallback = true)]
    public class PolyMonoEditor : PolyEditor
    {
        protected override VisualElement CreateUsageField()
        {
            return new NoteBox(((PolyMono) target).__Usage);
        }
    }

    [CustomEditor(typeof(PolyObject), true, isFallback = true)]
    public class PolyObjectEditor : PolyEditor
    {
        protected override VisualElement CreateUsageField()
        {
            return new NoteBox(((PolyObject) target).__Usage);
        }
    }

    public abstract class PolyEditor : Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var container = new VisualElement();

            container.Add(CreateRequiredComponentsField(serializedObject));
            container.Add(CreateUsageField());

            var spacer = new VisualElement();
            spacer.style.height = 12f;
            container.Add(spacer);

            var iterator = serializedObject.GetIterator();
            if (iterator.NextVisible(true))
                do
                {
                    var serializedProperty = iterator.Copy();
                    var propertyField = new PropertyField(serializedProperty) {name = "PropertyField:" + iterator.propertyPath};
                    TryMarkAttributes(propertyField, serializedProperty);
                    if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null)
                        propertyField.SetEnabled(false);

                    container.Add(propertyField);
                } while (iterator.NextVisible(false));

            container.schedule.Execute(() => { HighlightNulls(container); }).Every(500);
            SendHighPriorityToTop(container);
            SendLowPriorityToBottom(container);
            DisableMarked(container);
            RemoveScriptField(container);
            return container;
        }

        void DisableMarked(VisualElement container)
        {
            foreach (var item in container.Query(className: UI_Constants.DisableInInspector).ToList())
                item.SetEnabled(false);
        }

        void HighlightNulls(VisualElement container)
        {
            foreach (var item in container.Query(className: UI_Constants.HighlightNull).ToList())
                HighlightNull(item);
        }

        void HighlightNull(VisualElement item)
        {
            var objectField = item.Q<ObjectField>();
            if (objectField == null)
                return;

            var highlight = !objectField.value;

            item.style.backgroundColor = highlight ? UI_Constants.NullHighlightColor : new StyleColor(StyleKeyword.Undefined);
        }

        VisualElement CreateRequiredComponentsField(SerializedObject obj)
        {
            var builder = new StringBuilder();
            var hasAny = false;
            foreach (var item in obj.GetTypeFromSerializedObject().GetRequiredTypes())
            {
                hasAny = true;
                if (item.m_Type0 != null)
                    builder.AppendLine(item.m_Type0.Name);
                if (item.m_Type1 != null)
                    builder.AppendLine(item.m_Type1.Name);
                if (item.m_Type2 != null)
                    builder.AppendLine(item.m_Type2.Name);
            }

            return hasAny ? new NoteBox("Requires:\n" + builder.ToString().Trim()) : null;
        }

        protected abstract VisualElement CreateUsageField();

        void RemoveScriptField(VisualElement container)
        {
            container.Q<PropertyField>("PropertyField:m_Script")?.RemoveFromHierarchy();
        }

        static void SendHighPriorityToTop(VisualElement container)
        {
            foreach (var item in container.Query(className: UI_Constants.HighPriority).ToList())
                MoveToTop(item);
        }

        static void MoveToTop(VisualElement v)
        {
            v.SendToBack();
        }

        static void SendLowPriorityToBottom(VisualElement container)
        {
            foreach (var item in container.Query(className: UI_Constants.LowPriority).ToList())
                MoveToBottom(item);
        }

        static void MoveToBottom(VisualElement v)
        {
            v.BringToFront();
        }

        static void TryMarkAttributes(PropertyField pf, SerializedProperty property)
        {
            var fieldInfo = property.GetFieldInfo();
            if (fieldInfo.IsHighPriority())
                pf.AddToClassList(UI_Constants.HighPriority);
            else if (fieldInfo.IsLowPriority())
                pf.AddToClassList(UI_Constants.LowPriority);

            if (fieldInfo.IsHighlightNull())
                pf.AddToClassList(UI_Constants.HighlightNull);
            if (fieldInfo.IsDisableInInspector())
                pf.AddToClassList(UI_Constants.DisableInInspector);
        }
    }
}