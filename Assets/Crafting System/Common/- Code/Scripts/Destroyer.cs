using UnityEngine;

namespace Polyperfect.Common
{
    public class Destroyer : PolyMono
    {
        public override string __Usage => "Allows UnityEvents to destroy this or other objects.";

        public void DestroyThis()
        {
            Destroy(this.gameObject);
        }
        public void DestroyThisAfter(float delay)
        {
            Destroy(this.gameObject, delay);
        }
        public void DestroyThat(GameObject that)
        {
            Destroy(that);
        }
    }
}