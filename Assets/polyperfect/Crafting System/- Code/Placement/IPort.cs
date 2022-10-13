using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public interface IPort
    {
        Vector3 Position { get; }
        Quaternion Rotation { get; }
        PortIdentifier Type { get; }
        GameObject gameObject { get; }
        
        void NotifyConnect(IPort other);
        
        bool CanConnectToMatching(IPort other);
    }
}