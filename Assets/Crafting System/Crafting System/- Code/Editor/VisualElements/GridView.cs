using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    public class GridView<T> : VisualElement
    {
        readonly LinkedList<VisualElement> elementPool = new LinkedList<VisualElement>();
        readonly Dictionary<int, KeyValuePair<VisualElement, T>> mappedElements = new Dictionary<int, KeyValuePair<VisualElement, T>>();

        readonly VisualElement rowContainer;

        readonly List<VisualElement> rows = new List<VisualElement>();
        readonly ScrollView scrollView;

        IReadOnlyList<T> items;
        int maximumIndex;
        int minimumIndex;

        public GridView(Vector2 minSize, Func<VisualElement> makeItem, Action<VisualElement, T> bindItem, Func<IEnumerable<T>> getItems)
        {
            MinimumDimensions = minSize;
            MakeItem = makeItem;
            BindItem = bindItem;
            GetItems = getItems;
            scrollView = new ScrollView().SetStretch();
            scrollView.Add(rowContainer = new VisualElement());
            rowContainer.style.position = Position.Absolute;
            Add(scrollView);
            scrollView.contentViewport.RegisterCallback<GeometryChangedEvent>(HandleGeoChange);
            scrollView.verticalScroller.valueChanged += HandleScrollChange;
            schedule.Execute(Refresh);
        }

        Func<VisualElement> MakeItem { get; }
        Action<VisualElement, T> BindItem { get; }
        Func<IEnumerable<T>> GetItems { get; }
        public Vector2 MinimumDimensions { get; set; }

        void HandleScrollChange(float obj)
        {
            if (UpdateIndexRangeIfNecessary())
                UpdateDisplay();
        }

        void HandleGeoChange(GeometryChangedEvent evt)
        {
            UpdateDisplay();
        }

        void UpdateDisplay()
        {
            if (items == null || GetItemCount() <= 0)
                return;
            UpdateIndexRangeIfNecessary();
            EnsureElements();
            EnsureRows();
            UpdateContainerSizes();

            var activeElements = mappedElements.ToArray();
            foreach (var item in activeElements)
            {
                var index = item.Key;
                if (index < minimumIndex || index > maximumIndex)
                {
                    var elem = item.Value.Key;
                    mappedElements.Remove(index);
                    elementPool.AddLast(elem);
                }
            }

            foreach (var item in rows) item.Clear();

            for (var i = minimumIndex; i <= maximumIndex; i++)
                if (mappedElements.ContainsKey(i))
                {
                    GetRow(i).Add(mappedElements[i].Key);
                }
                else
                {
                    var pooled = elementPool.Last?.Value;
                    if (pooled == null)
                    {
                        Debug.LogError($"Nani. Mapped:{mappedElements.Count} Pooled: {elementPool.Count}");
                        break;
                    }

                    elementPool.RemoveLast();
                    BindItem(pooled, items[i]);
                    mappedElements.Add(i, new KeyValuePair<VisualElement, T>(pooled, items[i]));
                    GetRow(i).Add(pooled);
                }

            var toRemove = mappedElements.Where(e => e.Key < minimumIndex || e.Key > maximumIndex).ToList();
            foreach (var item in toRemove)
            {
                elementPool.AddLast(mappedElements[item.Key].Key);
                mappedElements.Remove(item.Key);
            }

            UpdateElementWidths();
        }

        void UpdateElementWidths()
        {
            var elementWidth = CalculateElementWidth();
            foreach (var item in mappedElements) item.Value.Key.style.width = elementWidth;
        }

        void UpdateContainerSizes()
        {
            rowContainer.style.top = minimumIndex / GetHorizontalCount() * CalculateElementHeight();
            rowContainer.style.height = rows.Count * CalculateElementHeight();
            scrollView.contentContainer.style.height = GetTotalHeight();
        }

        void EnsureElements()
        {
            var needed = maximumIndex - minimumIndex + 1;
            needed -= mappedElements.Count + elementPool.Count;
            for (var i = 0; i < needed; i++) elementPool.AddLast(MakeItem());
        }

        VisualElement GetRow(int index)
        {
            var rowIndex = (index - minimumIndex) / GetHorizontalCount();
            return rows[rowIndex];
        }

        int GetIndexInRow(int index)
        {
            return index % GetHorizontalCount();
        }

        void EnsureRows()
        {
            var count = Mathf.Max(CalculateVisibleVerticalCount(), 0);
            while (rows.Count < count)
            {
                var row = new VisualElement().SetRow();
                row.style.flexGrow = 1f;
                row.style.height = CalculateElementHeight();
                rows.Add(row);
                rowContainer.Add(row);
            }

            while (rows.Count > count)
            {
                rows[rows.Count - 1].RemoveFromHierarchy();
                rows.RemoveAt(rows.Count - 1);
            }

            for (var i = count; i < rows.Count; i++) rows[i].RemoveFromHierarchy();
        }

        public int GetHorizontalCount()
        {
            var width = resolvedStyle.width;
            var hCount = (int) (width / MinimumDimensions.x);
            hCount = Mathf.Max(hCount, 1);
            return hCount;
        }

        public int CalculateVisibleVerticalCount()
        {
            var height = scrollView.contentViewport.resolvedStyle.height;
            var vCountMin = (int) (height / CalculateElementHeight());
            var vCountMax = vCountMin + 2;
            return vCountMax;
        }

        public int CalculateTotalRequiredRows()
        {
            var hSize = GetHorizontalCount();
            var vSize = (GetItemCount() - 1) / hSize + 1;
            return vSize;
        }

        public float GetTotalHeight()
        {
            return CalculateTotalRequiredRows() * CalculateElementHeight();
        }

        public float CalculateElementWidth()
        {
            var width = resolvedStyle.width;
            var finalWidth = width / GetHorizontalCount();
            return finalWidth;
        }

        public float CalculateElementHeight()
        {
            return MinimumDimensions.y;
        }

        bool UpdateIndexRangeIfNecessary()
        {
            var lastMinimum = minimumIndex;
            var lastMaximum = maximumIndex;
            var scrolledIndex = (int) (scrollView.scrollOffset.y / CalculateElementHeight());
            minimumIndex = Mathf.Min(scrolledIndex * GetHorizontalCount(), GetItemCount() - 1);
            maximumIndex = Mathf.Min(minimumIndex + CalculateNeededElements() - 1, GetItemCount() - 1);
            if (minimumIndex != lastMinimum || maximumIndex != lastMaximum)
                return true;
            return false;
        }

        int CalculateNeededElements()
        {
            return Mathf.Min(GetItemCount(), GetHorizontalCount() * CalculateVisibleVerticalCount());
        }

        int GetItemCount()
        {
            return items?.Count ?? 0;
        }

        public void Refresh()
        {
            items = GetItems().ToList();
            rows.Clear();
            rowContainer.Clear();
            foreach (var item in mappedElements)
                elementPool.AddLast(item.Value.Key);
            mappedElements.Clear();
            UpdateDisplay();
        }
    }
}