using Polyperfect.Common;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Placement
{
    public class ConnectorPortGroup : PolyMono
    {
        public override string __Usage => $"Registers all children {nameof(ConnectorPort)} events so that only one child port can be connected at a time. Works by Enabling and Disabling said components.";

        public PortConnectEvent OnConnected;
        public UnityEvent OnDisconnected;
        
        ConnectorPort[] childPorts;

        void Awake()
        {
            childPorts = GetComponentsInChildren<ConnectorPort>(true);
            foreach (var port in childPorts)
            {
                var nohoist = port;
                port.OnConnected.AddListener(_=> HandleConnected(nohoist));
                port.OnDisconnected.AddListener(HandleDisconnected);
            }
        }

        void HandleDisconnected()
        {
            foreach (var item in childPorts)
                item.enabled = true;
            OnDisconnected?.Invoke();
        }

        void HandleConnected(ConnectorPort port)
        {
            foreach (var item in childPorts)
                item.enabled = item==port;
            OnConnected?.Invoke(port);
        }
    }
}