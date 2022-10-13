using System.Collections.Generic;
using UnityEngine;
using T = System.Int32;

namespace Polyperfect.Crafting.Integration
{
    [CreateMenuTitle("With Integer")]
    [CreateAssetMenu(menuName = "Polyperfect/Categories/With Integer")]
    public class CategoryWithInt : BaseCategoryWithData<T>
    {
        [SerializeField] protected List<T> data = new List<T>();
        public override string __Usage => "A collection whose members have floating point values as their data.";
        public override IReadOnlyList<T> Data => data;

        protected override void SetDataInternal(int index, T value)
        {
            data[index] = value;
        }
    }
}