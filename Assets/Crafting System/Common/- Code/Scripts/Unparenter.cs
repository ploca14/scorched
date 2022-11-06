using UnityEngine;

namespace Polyperfect.Common
{
    [DefaultExecutionOrder(500)]
    public class Unparenter : PolyMono
    {
        public override string __Usage => "Unparents the object on Start. Registers with Destroy and Enable/Disable events of its initial parent to match their state.";

        void Start()
        {
            var parent = transform.parent;
            if (!parent)
                return;
            transform.SetParent(null,false);
            
            var events = parent.gameObject.AddOrGetComponent<SimpleEvents>();
            events.OnActive.AddListener(()=>gameObject.SetActive(true));
            events.OnInactive.AddListener(()=>gameObject.SetActive(false));
            events.OnDestroyed.AddListener(()=>Destroy(gameObject));
        }
        
    }
}