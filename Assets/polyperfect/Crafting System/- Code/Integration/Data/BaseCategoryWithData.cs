using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    public abstract class BaseCategoryWithData : BaseCategoryObject
    {
        public abstract IDictionary ConstructDictionary();
        public abstract Type GetDataType();
    }

    public abstract class BaseCategoryWithData<T> : BaseCategoryWithData
    {
        public override Type GetDataType() => typeof(T);

        public abstract IReadOnlyList<T> Data { get; }

        public IEnumerable<KeyValuePair<BaseObjectWithID, T>> Pairs
        {
            get
            {
                Assert.AreEqual(members.Count, Data.Count);
                for (var i = 0; i < members.Count; i++)
                    if (members[i])
                        yield return new KeyValuePair<BaseObjectWithID, T>(members[i], Data[i]);
            }
        }

        protected abstract void SetDataInternal(int index, T value);

        public override IDictionary ConstructDictionary()
        {
            return Pairs.ToDictionary(p => p.Key.ID, p => p.Value);
        }

#if UNITY_EDITOR
        public void SetData(BaseObjectWithID item, T value, SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            var index = GetMemberIndex(item);
            if (index < 0)
                throw new KeyNotFoundException();
            Undo.RegisterCompleteObjectUndo(this, "set value");
            SetDataInternal(index, value);
            owner.Update();
        }

        public override void AddMember(BaseObjectWithID item, SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            var memberArray = GetMemberArrayProperty(owner);
            var dataArray = GetDataArrayProperty(owner);

            var count = memberArray.arraySize;

            memberArray.arraySize++;
            dataArray.arraySize++;

            memberArray.GetArrayElementAtIndex(count).objectReferenceValue = item;
            owner.ApplyModifiedProperties();
        }

        public override void RemoveMember(BaseObjectWithID item, SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            var memberArray = GetMemberArrayProperty(owner);
            var dataArray = GetDataArrayProperty(owner);

            var index = GetMemberIndex(item);
            var count = memberArray.arraySize;

            memberArray.MoveArrayElement(index, count - 1);
            dataArray.MoveArrayElement(index, count - 1);
            memberArray.arraySize--;
            dataArray.arraySize--;

            owner.ApplyModifiedProperties();
        }

        public override void DuplicateMemberData(BaseObjectWithID toDupe, BaseObjectWithID target, SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            var memberArray = GetMemberArrayProperty(owner);
            var dataArray = GetDataArrayProperty(owner);

            var count = memberArray.arraySize;
            var index = GetMemberIndex(toDupe);
            if (index < 0)
                throw new KeyNotFoundException();

            //move source object to end for auto duplication since we don't know the type
            memberArray.MoveArrayElement(index, count - 1);
            dataArray.MoveArrayElement(index, count - 1);

            memberArray.arraySize++;
            dataArray.arraySize++;

            memberArray.GetArrayElementAtIndex(count).objectReferenceValue = target;

            owner.ApplyModifiedProperties();
        }

        protected SerializedProperty GetDataArrayProperty(SerializedObject owner)
        {
            return owner.FindProperty("data");
        }

        protected SerializedProperty GetDataProperty(BaseObjectWithID obj)
        {
            return GetDataArrayProperty(CreateSO()).GetArrayElementAtIndex(GetMemberIndex(obj));
        }


        public override VisualElement CreateInlineEditor(BaseObjectWithID obj, string label)
        {
            var so = CreateSO();
            var ve = new VisualElement().SetRow().SetGrow();
            var pf = new PropertyField(GetDataProperty(obj));
            pf.Bind(so);
            pf.schedule.DoubleDelay(() =>
            {
                var subLabel = pf.Q<Label>(className: BaseField<int>.labelUssClassName);
                if (subLabel == null)
                    Debug.LogError("There may have been a change to the way unity handles UI internally. If you see this, please report a bug.");
                else
                    subLabel.RemoveFromHierarchy();
            });
            if (!string.IsNullOrEmpty(label))
                ve.Add(new Label(label) {style = {width = 100f}});
            ve.Add(pf);
            return ve;
        }

        protected override void OnValidate() 
        {
            base.OnValidate();
            var so = CreateSO();
            var memberArray = GetMemberArrayProperty(so);
            var dataArray = GetDataArrayProperty(so);
            for (var i = 0; i < memberArray.arraySize; i++)
                if (!memberArray.GetArrayElementAtIndex(i).objectReferenceValue)
                {
                    memberArray.MoveArrayElement(i, memberArray.arraySize - 1);
                    dataArray.MoveArrayElement(i, dataArray.arraySize - 1);
                    memberArray.arraySize--;
                    dataArray.arraySize--;
                    i--;
                }

            so.ApplyModifiedPropertiesWithoutUndo();
        }
#endif
    }
}