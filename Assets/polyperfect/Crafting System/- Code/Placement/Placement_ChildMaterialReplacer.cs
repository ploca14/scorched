using System;
using System.Linq;
using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public class Placement_ChildMaterialReplacer : PolyMono,IPlacementPostprocessor
    {
        public override string __Usage => $"An {nameof(IPlacementProcessor)} that replaces all materials on child renderers with the ones specified, when placement info is updated.";
        [SerializeField] bool ExcludeRenderersPresentInAwake = true;
        [SerializeField] Material ValidMaterial;
        [SerializeField] Material InvalidMaterial;
        Renderer[] renderers;
        Renderer[] exclude;
        bool initialized = false;

        void Awake() => exclude = ExcludeRenderersPresentInAwake ? GetComponentsInChildren<Renderer>():Array.Empty<Renderer>();

        void Start() => TryInit();

        void TryInit()
        {
            if (initialized)
                return;
            renderers = GetComponentsInChildren<Renderer>(true).Except(exclude).ToArray();
            initialized = true;
        }

        void ReplaceMaterials(Material mat)
        {
            TryInit();
            
            foreach (var item in renderers)
                item.sharedMaterial = mat;
        }

        public void PostprocessPlacement(in PlacementInfo info)
        {
            ReplaceMaterials(info.IsValid?ValidMaterial:InvalidMaterial);
        }
    }
}