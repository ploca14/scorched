using System;
using System.Collections.Generic;
using Polyperfect.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Polyperfect.Crafting.Integration.UGUI
{
    public class UGUITabs : PolyMono
    {
        [SerializeField] List<TabPair> Tabs = new List<TabPair>();
        public override string __Usage => "Easily switch between different UI menus";

        GameObject lastGO;
        void Start()
        {
            foreach (var item in Tabs)
            {
                var go = item.WindowObject;
                item.TabButton.onClick.AddListener(() => SwitchToTab(go));
            }

            SwitchToTab(null);
        }

        void SwitchToTab(GameObject go)
        {
            if (lastGO == go)
                go = null;
            
            foreach (var item in Tabs)
                item.WindowObject.SetActive(item.WindowObject == go);

            lastGO = go;
        }

        [Serializable]
        class TabPair
        {
            public Button TabButton;
            public GameObject WindowObject;
        }
    }
}