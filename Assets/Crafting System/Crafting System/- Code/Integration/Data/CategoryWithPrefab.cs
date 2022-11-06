using System.Collections.Generic;
using UnityEngine;
using DATA_TYPE = UnityEngine.GameObject;

namespace Polyperfect.Crafting.Integration
{
    [CreateMenuTitle("With Prefab")]
    [CreateAssetMenu(menuName = "Polyperfect/Categories/With Prefab")]
    public class CategoryWithPrefab : BaseCategoryWithData<DATA_TYPE>
    {
        [SerializeField] protected List<DATA_TYPE> data = new List<DATA_TYPE>();
        public override string __Usage => "For items that have some type of Prefab associated with them, such as 3d models or visual effects.";
        public override IReadOnlyList<DATA_TYPE> Data => data;

        protected override void SetDataInternal(int index, DATA_TYPE value)
        {
            data[index] = value;
        }
    }
}