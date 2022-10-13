using System.Collections.Generic;
using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    [CreateAssetMenu(menuName = "Polyperfect/Port Identifier")]
    public class PortIdentifier : PolyObject
    {
        public override string __Usage => "A simple identifier for ports.";

        public List<PortIdentifier> ConnectsTo;
        
    }
}