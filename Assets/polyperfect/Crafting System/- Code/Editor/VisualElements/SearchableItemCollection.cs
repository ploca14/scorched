namespace Polyperfect.Crafting.Edit
{
    /*public class SearchableItemCollection : VisualElement
    {
        public FilterableListview<BaseObjectWithID> FilterableListview { get; }
        public SearchableItemCollection(IReadOnlyDictionary<BaseObjectWithID, NameData> accessor)
        {
            var tf = new TextField(){label = "Search"};
            tf.labelElement.style.minWidth = 50f;
            tf.labelElement.style.flexGrow = 0f;
            tf.style.marginBottom = 8f;
            var listview = new FilterableListview<BaseObjectWithID>(
                o => accessor.GetDataOrDefault(o).Name?.ToLower().Contains(tf.text.ToLower().Trim()) ?? false,
                AssetUtility.FindAssetsOfType<BaseObjectWithID>);
            listview.style.flexGrow = 1f;
            tf.RegisterValueChangedCallback(e =>
            {
                listview.UpdateFilter();
            });
            Add(tf);
            Add(listview);
            FilterableListview = listview;
        }
    }*/
}