using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyperfect.Crafting.Integration
{
    public abstract class BaseCategoryObject : BaseObjectWithID
    {
        [SerializeField] protected List<BaseObjectWithID> members = new List<BaseObjectWithID>();
        public virtual Func<BaseObjectWithID, bool> Criteria => o => o is BaseItemObject;
        public IEnumerable<BaseObjectWithID> ValidMembers => members.Where(m => m);

        public bool Contains(RuntimeID id)
        {
            return ValidMembers.Any(i => i.ID == id);
        }

        public bool Contains(BaseObjectWithID obj)
        {
            return ValidMembers.Contains(obj);
        }


#if UNITY_EDITOR
        public abstract void AddMember(BaseObjectWithID item, SerializedObject owner = null);
        public abstract void RemoveMember(BaseObjectWithID item, SerializedObject owner = null);
        public abstract void DuplicateMemberData(BaseObjectWithID toDupe, BaseObjectWithID target, SerializedObject owner = null);

        public abstract VisualElement CreateInlineEditor(BaseObjectWithID obj, string label);


        protected SerializedProperty GetMemberArrayProperty(SerializedObject owner = null)
        {
            owner = owner ?? CreateSO();
            return owner.FindProperty("members");
        }

        protected int GetMemberIndex(BaseObjectWithID id)
        {
            return members.IndexOf(id);
        }

        protected SerializedObject CreateSO()
        {
            return new SerializedObject(this);
        }
#endif
    }
}