using System.Collections.Generic;
using UnityEngine;
using DATA_TYPE = Polyperfect.Crafting.Integration.IconData;

namespace Polyperfect.Crafting.Integration
{
    [CreateMenuTitle("With Sprite")]
    [CreateAssetMenu(menuName = "Polyperfect/Categories/With Icon")]
    public class IconsCategory : BaseCategoryWithData<IconData>
    {
        [SerializeField] List<IconData> data = new List<IconData>();

        public override string __Usage => "Container for sprite icons.";

        public override IReadOnlyList<IconData> Data => data;

        protected override void SetDataInternal(int index, IconData value)
        {
            data[index] = value;
        }
    }
}