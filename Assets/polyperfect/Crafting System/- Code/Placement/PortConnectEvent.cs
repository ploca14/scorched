using System;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Placement
{
    [Serializable]
    public class PortConnectEvent : UnityEvent<IPort> { }
}