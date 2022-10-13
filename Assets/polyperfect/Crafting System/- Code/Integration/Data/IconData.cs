using System;
using UnityEngine;

namespace Polyperfect.Crafting.Integration
{
    /// <summary>
    ///     References for item icons
    /// </summary>
    [Serializable]
    public struct IconData
    {
        public Sprite Icon;

        public IconData(Sprite icon)
        {
            Icon = icon;
        }
    }
}