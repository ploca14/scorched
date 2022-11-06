using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Common.Edit;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Integration;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    [CustomEditor(typeof(BaseCategoryObject), true)]
    public class CategoryObjectEditor : Editor
    {
        IReadOnlyDictionary<RuntimeID, IconData> _iconLookup;

        void OnEnable()
        {
            RefreshIconLookup();
        }

        void RefreshIconLookup()
        {
            //throw new System.NotImplementedException();
            _iconLookup = EditorItemDatabase.Data.GetReadOnlyAccessor<IconData>();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var edited = (BaseCategoryObject) target;
            var ve = new VisualElement();
            var searchField = new TextField("Search");
            var groupControl = new MemberGroupControl<BaseObjectWithID>(
                () => AssetUtility.FindAssetsOfType<BaseObjectWithID>().Where(o => edited.Criteria(o)).Where(o => !edited.Contains(o.ID)),
                () => AssetUtility.FindAssetsOfType<BaseObjectWithID>().Where(o => edited.Criteria(o)).Where(o => edited.Contains(o.ID)),
                m => { edited.AddMember(m); },
                m => { edited.RemoveMember(m); },
                (v, item) =>
                {
                    v.Clear();
                    var sprite = _iconLookup.GetDataOrDefault(item).Icon;
                    var simpleIconElement = new SimpleIconElement(sprite ? sprite.texture : null, item.name.NewlineSpaces()).SetGrow();
                    v.Add(simpleIconElement);
                    simpleIconElement.Add(VisualElementPresets.CreateHoverText(simpleIconElement, item.name.NewlineSpaces()));
                },
                (v, item) =>
                {
                    v.Clear();
                    var sprite = _iconLookup.GetDataOrDefault(item).Icon;
                    var simpleIconElement = new SimpleIconElement(sprite ? sprite.texture : null, item.name);
                    simpleIconElement.style.width = 64f;
                    v.Add(simpleIconElement);
                    simpleIconElement.Add(VisualElementPresets.CreateHoverText(simpleIconElement, item.name));
                    var inlineEditor = edited.CreateInlineEditor(item, "");
                    v.Add(inlineEditor);
                    inlineEditor.Query<ObjectField>().ForEach(o => o.RegisterValueChangedCallback(e =>
                    {
                        RefreshIconLookup();
                        o.QP<FilterableListview<BaseObjectWithID>>().Rebuild();
                    }));
                },
                o => string.IsNullOrEmpty(searchField.text) || o.name.ToLower().Contains(searchField.text.ToLower())
            );
            groupControl.SetGrow();
            searchField.RegisterValueChangedCallback(e => groupControl.UpdateLists());
            ve.Add(searchField);
            ve.Add(groupControl);
            return ve;
        }
    }
}