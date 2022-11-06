using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyperfect.Crafting.Integration
{
    [CreateMenuTitle("Simple")]
    [CreateAssetMenu(menuName = "Polyperfect/Categories/Simple")]
    public class CategoryObject : BaseCategoryObject
    {
        public override string __Usage => "Container for objects that are members of particular categories.";

#if UNITY_EDITOR
        public override void AddMember(BaseObjectWithID item, SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            var memberArray = GetMemberArrayProperty(owner);
            var count = memberArray.arraySize;
            memberArray.arraySize++;
            memberArray.GetArrayElementAtIndex(count).objectReferenceValue = item;
            owner.ApplyModifiedProperties();
        }

        public override void RemoveMember(BaseObjectWithID item, SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            var memberArray = GetMemberArrayProperty(owner);
            var index = GetMemberIndex(item);
            var count = memberArray.arraySize;
            memberArray.MoveArrayElement(index, count - 1);
            memberArray.arraySize--;
            owner.ApplyModifiedProperties();
        }

        public override void DuplicateMemberData(BaseObjectWithID toDupe, BaseObjectWithID target, SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            var memberArray = GetMemberArrayProperty(owner);

            var count = memberArray.arraySize;
            var index = GetMemberIndex(toDupe);
            if (index < 0)
                throw new KeyNotFoundException();

            memberArray.MoveArrayElement(index, count - 1);

            memberArray.arraySize++;

            memberArray.GetArrayElementAtIndex(count).objectReferenceValue = target;

            owner.ApplyModifiedProperties();
        }

        public override VisualElement CreateInlineEditor(BaseObjectWithID obj, string label)
        {
            return new Label(label);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            var so = CreateSO();
            var memberArray = GetMemberArrayProperty(so);
            for (var i = 0; i < memberArray.arraySize; i++)
                if (!memberArray.GetArrayElementAtIndex(i).objectReferenceValue)
                {
                    memberArray.MoveArrayElement(i, memberArray.arraySize - 1);
                    memberArray.arraySize--;
                    i--;
                }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }
}