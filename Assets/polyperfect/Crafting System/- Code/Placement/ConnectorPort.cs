using System;
using System.Collections.Generic;
using Polyperfect.Common;
using UnityEngine;
using UnityEngine.Events;

namespace Polyperfect.Crafting.Placement
{
    public class ConnectorPort : PolyMono,IPort,IIncludeInVisualization
    {

        public override string __Usage => $"Uses {nameof(PortManager)} to connect with other ports. Used by the conveyor belt system, for example.";
        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        public PortConnectEvent OnConnected;
        public UnityEvent OnDisconnected;
        public PortIdentifier Type => type;
        readonly List<Func<IPort,bool>> canConnectProcessors = new List<Func<IPort, bool>>();
        public void NotifyConnect(IPort other)
        {
            if (other!=null)
                OnConnected.Invoke(other);
            else
                OnDisconnected.Invoke();
        }

        void Start()
        {
            if (!type)
            {
                Debug.LogError($"You must specify a {nameof(PortIdentifier)} on the {nameof(ConnectorPort)} on {gameObject.name} for it to function with the {nameof(PortManager)}");
                enabled = false;
            }

        }

        public void AddConnectionCondition(Func<IPort, bool> criteria)
        {
            canConnectProcessors.Add(criteria);
        }
        public bool CanConnectToMatching(IPort other)
        {
            return canConnectProcessors.TrueForAll(p => p(other));
        }

        [HighlightNull] [SerializeField] PortIdentifier type;
        public void MarkIsVisualization() => enabled = false;

        void OnEnable() => PortManager.RegisterPort(this);

        void OnDisable() => PortManager.UnregisterPort(this);

        public void DisconnectPort()
        {
            PortManager.DisconnectPort(this,true);
        }
        void OnDrawGizmosSelected()
        {
            var startColor = Gizmos.color;
            Gizmos.color = Color.green;
            if (PortManager.IsConnected(this))
                Gizmos.DrawLine(Position,PortManager.GetConnected(this).Position);
            Gizmos.color = startColor;
        }
    }
}