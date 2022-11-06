using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    public class MemberGroupControl<T> : VisualElement where T : Object
    {
        public MemberGroupControl(Func<IEnumerable<T>> getNonMembers, Func<IEnumerable<T>> getMembers, Action<T> onMakeMember, Action<T> onRemoveMember,
            Action<VisualElement, T> bindNonMember, Action<VisualElement, T> bindMember, Func<T, bool> listFilter)
        {
            var categoryRow = new VisualElement();
            var nonmemberColumn = new VisualElement();
            var memberColumn = new VisualElement();

            categoryRow.SetGrow();
            nonmemberColumn.SetGrow(.5f);
            memberColumn.SetGrow();

            categoryRow.SetRow();

            categoryRow.style.minHeight = 210f;
            var memberCategoryList = new FilterableListview<T>(listFilter, getMembers);
            var nonmemberCategoryList = new FilterableListview<T>(listFilter, getNonMembers);
            memberCategoryList.itemHeight = 38;
            nonmemberCategoryList.itemHeight = 38;
            var objectLookup = new Dictionary<VisualElement, T>();
            memberCategoryList.SetGrow();
            nonmemberCategoryList.SetGrow();
            memberCategoryList.SetMargin(4f);
            nonmemberCategoryList.SetMargin(4f);


            memberCategoryList.makeItem = () =>
            {
                var ve = new VisualElement().SetRow();
                ve.style.alignItems = Align.Center;
                ve.AddManipulator(new DataDragManipulator<T>(() => objectLookup[ve]));
                ve.AddManipulator(new OpenForEditManipulator(() => objectLookup[ve], 2));
                ve.AddManipulator(new ClickEater());
                objectLookup.Add(ve, null);
                return ve;
            };
            nonmemberCategoryList.makeItem = () =>
            {
                var ve = new VisualElement().SetRow().CenterContents();
                ve.AddManipulator(new DataDragManipulator<T>(() => objectLookup[ve]));
                ve.AddManipulator(new OpenForEditManipulator(() => objectLookup[ve], 2));
                ve.AddManipulator(new ClickEater());
                objectLookup.Add(ve, null);
                return ve;
            };
            memberCategoryList.bindItem = (v, i) =>
            {
                var collection = memberCategoryList.FilteredItems[i];
                objectLookup[v] = collection;
                bindMember(v, collection);
            };
            nonmemberCategoryList.bindItem = (v, i) =>
            {
                var collection = nonmemberCategoryList.FilteredItems[i];
                objectLookup[v] = collection;
                bindNonMember(v, collection);
            };

            var nonmemberLabel = new Label("NOT");
            var memberLabel = new Label("IS");
            nonmemberLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            memberLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nonmemberColumn.Add(nonmemberLabel);
            nonmemberColumn.Add(nonmemberCategoryList);
            memberColumn.Add(memberLabel);
            memberColumn.Add(memberCategoryList);

            categoryRow.Add(nonmemberColumn);
            categoryRow.Add(memberColumn);

            Add(categoryRow);

            memberCategoryList.UpdateOriginals();
            nonmemberCategoryList.UpdateOriginals();

            nonmemberCategoryList.AddManipulator(new DataDropManipulator<T>(o =>
            {
                if (nonmemberCategoryList.AllItems.Contains(o))
                    return;
                onRemoveMember(o);
                UpdateLists();
            }));
            memberCategoryList.AddManipulator(new DataDropManipulator<T>(o =>
            {
                if (memberCategoryList.AllItems.Contains(o))
                    return;
                onMakeMember(o);
                UpdateLists();
            }));

            NonMembersView = nonmemberCategoryList;
            MembersView = memberCategoryList;
        }

        public FilterableListview<T> NonMembersView { get; }
        public FilterableListview<T> MembersView { get; }

        public void UpdateLists()
        {
            MembersView.UpdateOriginals();
            NonMembersView.UpdateOriginals();
        }
    }
}