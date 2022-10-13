using UnityEngine;
using UnityEngine.UIElements;

namespace Polyperfect.Common
{
    public class NoteBox : VisualElement
    {
        public NoteBox(string text)
        {
            var ve = this;
            ve.SetPadding(4f);
            ve.SetMargin(4f);
            ve.SetBorderWidth(1f);
            ve.SetRadius(4f);
            ve.SetBorderColor(new Color(0, 0, 0, .3f));
            ve.style.backgroundColor = new Color(1, 1, 1, .05f);
            var label = new Label(text);
            label.style.whiteSpace = WhiteSpace.Normal;
            ve.Add(label);
        }
    }
}