using System;
using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using UnityEngine;
using UnityEngine.Profiling;

namespace Polyperfect.Crafting.Placement
{
    public static class PortManager
    {
        static readonly Dictionary<IPort,IPort> closedPorts = new Dictionary<IPort, IPort>();
        static readonly Dictionary<PortIdentifier,LinkedList<IPort>> openPorts = new Dictionary<PortIdentifier, LinkedList<IPort>>(); 

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Init()
        {
            closedPorts.Clear();
            openPorts.Clear();
        }
        public static void RegisterPort(IPort port)
        {
            var type = port.Type;
            EnsureTypeExists(type);
            openPorts[type].AddLast(port);
        }

        public static void UnregisterPort(IPort port)
        {
            var type = port.Type;
            if (openPorts[type].Contains(port))
                openPorts[type].Remove(port);

            if (closedPorts.ContainsKey(port)) 
                DisconnectPort(port,false);
        }

        public static void DisconnectPort(IPort port,bool autoReregisterThis)
        {
            var connected = closedPorts[port];
            closedPorts.Remove(port);
            closedPorts.Remove(connected);
            if (autoReregisterThis) 
                RegisterPort(port);
            RegisterPort(connected);
            
            port.NotifyConnect(null);
            connected.NotifyConnect(null);
        }

        public static IEnumerable<IPort> EnumeratePotentialConnections(IPort port)
        {
            return port.Type.ConnectsTo.SelectMany(EnumerateOpenPortsOfType);
        }
        public static IEnumerable<IPort> EnumerateOpenPortsOfType(PortIdentifier type)
        {
            EnsureTypeExists(type);
            return openPorts[type];
        }

        static void EnsureTypeExists(PortIdentifier type)
        {
            if (!openPorts.ContainsKey(type))
                openPorts.Add(type, new LinkedList<IPort>());
        }

        const float ANGLE_WEIGHTING = 1f / 180;
        public static IPort GetNearestWhere(IPort port,Func<IPort,bool> whereClause)
        {
            Profiler.BeginSample("Finding Nearest Port");
            var position = port.Position;
            var nearestWhere = EnumeratePotentialConnections(port).Where(whereClause).MinBy(p => Vector3.Distance(position, p.Position)+Quaternion.Angle(port.Rotation,p.Rotation)*ANGLE_WEIGHTING);
            Profiler.EndSample();
            return nearestWhere;
        }
        
        public static bool TryConnectToNearest(IPort port, Func<IPort,bool> whereClause)
        {
            if (IsConnected(port))
                return false;
            var pos = port.Position;
            var nearest = GetNearestWhere(port, whereClause);
            
            if (nearest.IsDefault()||Vector3.Distance(nearest.Position,pos)>.1f)
                return false;
            
            return TryConnectPorts(port,nearest);
        }
        public static bool IsConnected(IPort a)
        {
            return closedPorts.ContainsKey(a);
        }

        public static IPort GetConnected(IPort a)
        {
            return closedPorts[a];
        }
        public static bool TryConnectPorts(IPort a, IPort b)
        {
            
            if (!AreCompatibleIfMatched(a, b))
                return false;
            if (closedPorts.ContainsKey(a) || closedPorts.ContainsKey(b))
                return false;
            Debug.DrawLine(a.Position,b.Position,Color.red,5f);
            closedPorts.Add(a,b);
            closedPorts.Add(b,a);
            openPorts[a.Type].Remove(a);
            openPorts[b.Type].Remove(b);
            a.NotifyConnect(b);
            b.NotifyConnect(a);
            return true;
        }

        public static bool AreCompatibleIfMatched(IPort a, IPort b)
        {
            return a.CanConnectToMatching(b) && b.CanConnectToMatching(a);
        }
    }
}