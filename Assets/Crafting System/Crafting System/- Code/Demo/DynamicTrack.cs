using System.Collections.Generic;
using System.Linq;
using Polyperfect.Common;
using Polyperfect.Crafting.Framework;
using Polyperfect.Crafting.Placement;
using UnityEngine;
using UnityEngine.Profiling;

namespace Polyperfect.Crafting.Demo
{
    public class DynamicTrack : PolyMono, IIncludeInVisualization,IPlacementProcessor
    {
        public override string __Usage =>
            "Handles swapping out the parts of the conveyor in both visualization and when actually created to ensure they connect end-to-end.";
        [SerializeField] GameObject Straight, Left, Right, Up, Down;

        static object PlacedObjectIndexKey = new object();
        public void ProcessPlacement(ref PlacementInfo info)
        {
            if (!isVisualization)
                ProcessActualPlacement(ref info);
            else
                ProcessVisualizationPlacement(ref info);
        }

        void ProcessActualPlacement(ref PlacementInfo info)
        {
            var index = 0;
            var hasPlacementIndex = info.TryGetGenericData(PlacedObjectIndexKey, out int placedIndex);
            foreach (var item in Possibilities)
            {
                if (hasPlacementIndex)
                    item.SetActive(index == placedIndex);
                else
                    item.SetActive(index == 0);
                
                index++;
            }
        }

        void ProcessVisualizationPlacement(ref PlacementInfo info)
        {
            Profiler.BeginSample(nameof(ProcessVisualizationPlacement));


            info.TryGetGenericData(Placement_PortSnapper.ConnectedPortKey, out IPort intendedPort);
            var desired = pickObject(ref info,intendedPort);
            var index = 0;
            foreach (var item in Possibilities)
            {
                var setActive = item == desired;
                item.SetActive(setActive);
                if (setActive)
                    info.SetGenericData(PlacedObjectIndexKey, index);
                index++;
            }
            Profiler.EndSample();
        }

        GameObject pickObject(ref PlacementInfo info,IPort port)
        {
            if (port.IsDefault())
                return Straight;
            var relativeToPortMatrix = Matrix4x4.TRS(port.Position, port.Rotation, Vector3.one);
            var relativePoint=relativeToPortMatrix.inverse.MultiplyPoint(info.InitialPosition);
            var rightDot = Vector3.Dot(relativePoint.normalized, Vector3.right);
            if (rightDot > .4f)
                return Right;
            if (rightDot < -.4f)
                return Left;
            return Straight;
        }

        IEnumerable<GameObject> Possibilities
        {
            get
            {
                yield return Straight;
                yield return Left;
                yield return Right;
                yield return Up;
                yield return Down;
            }
        }
        //serialized so its state is maintained when instatiation from within the InstantiateVisuals thing
        [SerializeField] [HideInInspector] bool isVisualization;
        public void MarkIsVisualization() => isVisualization = true;
    }
}