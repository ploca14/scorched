using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    /// <summary>
    ///     A listview that has its elements filtered based on some criteria.
    /// </summary>
    /// <typeparam name="T">The Type that the collection contains.</typeparam>
    public class FilterableListview<T> : ListView
    {
        readonly List<T> filteredItems;
        readonly Func<IEnumerable<T>> getOriginals;
        readonly Func<T, bool> selector;
        T[] originalItems;

        public FilterableListview(Func<T, bool> selector, Func<IEnumerable<T>> getOriginals)
        {
            style.backgroundColor = new Color(0f, 0f, 0f, .1f);
            this.getOriginals = getOriginals;
            filteredItems = new List<T>();
            itemsSource = filteredItems;
            this.selector = selector;
            UpdateOriginals();
            UpdateFilter();
        }

        public IReadOnlyList<T> AllItems => originalItems;
        public IReadOnlyList<T> FilteredItems => filteredItems;

        /// <summary>
        ///     Re-applies the filter to the original object collection.
        /// </summary>
        public void UpdateFilter()
        {
            filteredItems.Clear();
            filteredItems.AddRange(originalItems.Where(i => selector(i)));
            Rebuild();
        }

        /// <summary>
        ///     Re-runs the original fetching method to get an updated list, then applies the filter.
        /// </summary>
        public void UpdateOriginals()
        {
            originalItems = getOriginals().ToArray();
            UpdateFilter();
        }
    }
}