using Polyperfect.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Crafting.Edit
{
    public sealed class SimpleIconElement : TextElement
    {
        public SimpleIconElement(Texture2D icon, string backupText, float size = 32f)
        {
            style.flexShrink = 0f;
            style.unityBackgroundScaleMode = ScaleMode.StretchToFill;
            style.width = size;
            style.height = size;
            style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            this.CenterContents();
            ChangeContents(icon, backupText);
        }

        public void ChangeContents(Texture2D icon, string backupText)
        {
            text = icon ? "" : backupText;
            style.backgroundImage = icon;
        }
    }
}