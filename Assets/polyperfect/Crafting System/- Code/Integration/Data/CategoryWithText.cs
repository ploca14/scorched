using System.Collections.Generic;
using UnityEngine;
using DATA_TYPE = System.String;

namespace Polyperfect.Crafting.Integration
{
    [CreateMenuTitle("With Text")]
    [CreateAssetMenu(menuName = "Polyperfect/Categories/With Text")]
    public class CategoryWithText : BaseCategoryWithData<DATA_TYPE>
    {
        [SerializeField] protected List<DATA_TYPE> data = new List<DATA_TYPE>();
        public override string __Usage => "A collection whose members have floating point values as their data.";
        public override IReadOnlyList<DATA_TYPE> Data => data;

        protected override void SetDataInternal(int index, DATA_TYPE value)
        {
            data[index] = value;
        }
    }
}