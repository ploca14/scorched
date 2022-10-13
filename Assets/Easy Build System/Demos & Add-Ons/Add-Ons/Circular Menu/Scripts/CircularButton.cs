using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EasyBuildSystem.Addons.CircularMenu.Scripts
{
    public class CircularButton : MonoBehaviour
    {
        #region Fields

        public Image Icon;
        public string Text;
        public string Description;
        public UnityEvent Action;

        #endregion

        #region Methods

        public void Init(string name, string description, Sprite sprite, UnityEvent action)
        {
            Text = name;
            Description = description;

            if (sprite != null)
                Icon.sprite = sprite;

            Action = action;
        }

        #endregion
    }

    [Serializable]
    public class CircularButtonData
    {
        #region Fields

        public Sprite Icon;
        public int Order;
        public string Text;
        public string Description;
        public UnityEvent Action;

        #endregion
    }
}