using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Extensions
{
    public static class MathExtension
    {
        #region Methods 

        public static Bounds GetChildsBounds(this GameObject target)
        {
            MeshRenderer[] Renders = target.GetComponentsInChildren<MeshRenderer>();

            Quaternion CurrentRotation = target.transform.rotation;

            Vector3 CurrentScale = target.transform.localScale;

            target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            target.transform.localScale = Vector3.one;

            Bounds ResultBounds = new Bounds(target.transform.position, Vector3.zero);

            foreach (Renderer Render in Renders)
            {
                ResultBounds.Encapsulate(Render.bounds);
            }

            Vector3 RelativeCenter = ResultBounds.center - target.transform.position;

            ResultBounds.center = RelativeCenter;

            ResultBounds.size = ResultBounds.size;

            target.transform.rotation = CurrentRotation;

            target.transform.localScale = CurrentScale;

            return ResultBounds;
        }

        public static Bounds GetParentBounds(this GameObject target)
        {
            MeshRenderer[] Renders = target.GetComponents<MeshRenderer>();

            Quaternion CurrentRotation = target.transform.rotation;

            Vector3 CurrentScale = target.transform.localScale;

            target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            target.transform.localScale = Vector3.one;

            Bounds ResultBounds = new Bounds(target.transform.position, Vector3.zero);

            foreach (Renderer Render in Renders)
            {
                ResultBounds.Encapsulate(Render.bounds);
            }

            Vector3 RelativeCenter = ResultBounds.center - target.transform.position;

            ResultBounds.center = PositionToGridPosition(0.1f, 0f, RelativeCenter);

            ResultBounds.size = PositionToGridPosition(0.1f, 0f, ResultBounds.size);

            target.transform.rotation = CurrentRotation;

            target.transform.localScale = CurrentScale;

            return ResultBounds;
        }

        public static Bounds ConvertBoundsToWorld(this Transform transform, Bounds localBounds)
        {
            if (transform != null)
            {
                return new Bounds(transform.TransformPoint(localBounds.center), new Vector3(localBounds.size.x * transform.localScale.x,
                    localBounds.size.y * transform.localScale.y,
                    localBounds.size.z * transform.localScale.z));
            }
            else
            {
                return new Bounds(localBounds.center, new Vector3(localBounds.size.x * transform.localScale.x,
                    localBounds.size.y * transform.localScale.y,
                    localBounds.size.z * transform.localScale.z));
            }
        }

        public static float ConvertToGrid(float gridSize, float gridOffset, float axis)
        {
            return Mathf.Round(axis) * gridSize + gridOffset;
        }

        public static Vector3 PositionToGridPosition(float gridSize, float gridOffset, Vector3 position)
        {
            position -= Vector3.one * gridOffset;
            position /= gridSize;
            position = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
            position *= gridSize;
            position += Vector3.one * gridOffset;
            return position;
        }

        public static Vector3 Clamp(Vector3 value, Vector3 min, Vector3 max)
        {
            value.x = Mathf.Clamp(value.x, min.x, max.x);
            value.y = Mathf.Clamp(value.y, min.y, max.y);
            value.z = Mathf.Clamp(value.z, min.z, max.z);
            return value;
        }

        #endregion
    }
}