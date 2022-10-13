using System.Linq;
using Polyperfect.Common;
using UnityEngine;

namespace Polyperfect.Crafting.Placement
{
    public class Placement_PortSnapper : PolyMono, IPlacementProcessor
    {
        public override string __Usage =>
            $"An {nameof(IPlacementProcessor)} that snaps to nearby ports. Requires at least one active child to have an {nameof(IPort)} attached, which is considered the Main Port.";

        public float PortSnapRange = 2f;
        public bool RequiresPort;
        [Range(0,1)] public float BiasTowardsPreviousSnapTarget = .5f;
        IPort[] childPorts;

        IPort lastSnapTarget;
        //IPort mainPort;
        bool initted = false;
        public static object ConnectedPortKey { get; } = new object();
        void Start() => TryInit();

        void TryInit()
        {
            if (initted)
                return;
            childPorts = GetComponentsInChildren<IPort>(false);
            initted = true;
        }

        public void ProcessPlacement(ref PlacementInfo info)
        {
            TryInit();
            //the following assumes that the .Position and .Rotation properties access the transform directly.
            //it's a preferred evil for now for making some other things more intuitive.
            
            var trans = transform;
            var previousPosition = trans.position;
            var previousRotation = trans.rotation;
            trans.position = info.Position;
            trans.rotation = info.Rotation;
            
            var nearestSnapCandidate = childPorts.SelectMany(a => PortManager.EnumeratePotentialConnections(a).Select(b => new SnapCandidate(a, b))).MinBy(CalculateSnapDistance);
            
            if (nearestSnapCandidate != null)
            {
                if (Vector3.Distance(nearestSnapCandidate.ExistingPort.Position, nearestSnapCandidate.PlacedPort.Position) > PortSnapRange
                    && Vector3.Distance(nearestSnapCandidate.ExistingPort.Position,info.Position)>PortSnapRange)
                    nearestSnapCandidate = default;
                else
                    SnapToPort(ref info, nearestSnapCandidate.PlacedPort,nearestSnapCandidate.ExistingPort);
            }

            lastSnapTarget = nearestSnapCandidate?.ExistingPort;
            info.SetGenericData(ConnectedPortKey,nearestSnapCandidate?.ExistingPort);
            
            trans.position = previousPosition;
            trans.rotation = previousRotation;

            if (nearestSnapCandidate == null && RequiresPort)
                info.IsValid = false;

        }
        

        void SnapToPort(ref PlacementInfo info,IPort placedPort, IPort existingPort)
        {
            //the following relies on the transforms being synced with the info
            
            var trans = transform;
            trans.position = Vector3.zero;
            trans.rotation = Quaternion.identity;

            var localOffset = placedPort.Position;
            info.Rotation = existingPort.Rotation * Quaternion.Inverse(placedPort.Rotation);
            info.Position = existingPort.Position - info.Rotation*localOffset;

        }

        float CalculateSnapDistance(SnapCandidate candidate)
        {
            var rawDistance = Vector3.Distance(candidate.PlacedPort.Position, candidate.ExistingPort.Position) / PortSnapRange 
                                + Quaternion.Angle(candidate.PlacedPort.Rotation, candidate.ExistingPort.Rotation) / 180f;
            if (candidate.ExistingPort == lastSnapTarget)
                rawDistance *= 1f-BiasTowardsPreviousSnapTarget;
            return rawDistance;
        }
        class SnapCandidate
        {
            public readonly IPort PlacedPort, ExistingPort;

            public SnapCandidate(IPort placedPort, IPort existingPort)
            {
                PlacedPort = placedPort;
                ExistingPort = existingPort;
            }
        }
    }
}