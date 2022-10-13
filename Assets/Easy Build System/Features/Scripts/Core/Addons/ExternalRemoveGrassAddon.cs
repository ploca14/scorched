using EasyBuildSystem.Features.Scripts.Core.Base.Addon;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Event;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager.Surface;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Addons
{
    [Addon("External Remove Grass Add-on", "Remove terrain grass when the piece is placed, this can cause performance drop according the terrain size.\n" +
        "You can find more information about this component in the documentation.", AddonTarget.PieceBehaviour)]
    public class ExternalRemoveGrassAddon : AddonBehaviour
    {
        #region Fields

        public static bool ShowGizmos;

        public float RemoveGrassRadius = 5.0f;

        PieceBehaviour Piece;

        #endregion Fields

        #region Methods

        private void Awake()
        {
            Piece = GetComponent<PieceBehaviour>();

            if (BuildManager.Instance.BuildableSurfaces.Contains(SupportType.TerrainCollider))
                TerrainManager.Initialize();
        }

        private void Start()
        {
            if (!BuildManager.Instance.BuildableSurfaces.Contains(SupportType.TerrainCollider))
                return;

            if (Piece.CurrentState == StateType.Placed)
            {
                if (!TerrainManager.Instance.CheckDetailtAt(transform.position, RemoveGrassRadius))
                    return;

                StartCoroutine(RemoveGrassToPosition(transform.position, RemoveGrassRadius));
            }

            BuildEvent.Instance.OnPieceInstantiated.AddListener(OnPieceInstantiated);
        }

        private void OnPieceInstantiated(PieceBehaviour part, SocketBehaviour socket)
        {
            if (!BuildManager.Instance.BuildableSurfaces.Contains(SupportType.TerrainCollider))
                return;

            if (part != Piece) return;

            if (part.CurrentState != StateType.Placed) return;

            if (!TerrainManager.Instance.CheckDetailtAt(transform.position, RemoveGrassRadius))
                return;

            StartCoroutine(RemoveGrassToPosition(part.transform.position, RemoveGrassRadius));
        }

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, RemoveGrassRadius);
        }

        public IEnumerator RemoveGrassToPosition(Vector3 position, float radius)
        {
            if (TerrainManager.Instance == null)
                yield break;

            Terrain ActiveTerrain = TerrainManager.Instance.ActiveTerrain;

            if (ActiveTerrain == null)
                yield break;

            for (int Layer = 0; Layer < ActiveTerrain.terrainData.detailPrototypes.Length; Layer++)
            {
                int TerrainDetailMapSize = ActiveTerrain.terrainData.detailResolution;

                if (ActiveTerrain.terrainData.size.x != ActiveTerrain.terrainData.size.z)
                    yield break;

                float DetailSize = TerrainDetailMapSize / ActiveTerrain.terrainData.size.x;

                Vector3 TexturePoint3D = position - ActiveTerrain.transform.position;

                TexturePoint3D = TexturePoint3D * DetailSize;

                float[] Matrix = new float[4];
                Matrix[0] = TexturePoint3D.z + radius;
                Matrix[1] = TexturePoint3D.z - radius;
                Matrix[2] = TexturePoint3D.x + radius;
                Matrix[3] = TexturePoint3D.x - radius;

                int[,] Data = ActiveTerrain.terrainData.GetDetailLayer(0, 0, ActiveTerrain.terrainData.detailWidth, ActiveTerrain.terrainData.detailHeight, Layer);

                for (int y = 0; y < ActiveTerrain.terrainData.detailHeight; y++)
                {
                    for (int x = 0; x < ActiveTerrain.terrainData.detailWidth; x++)
                    {
                        if (Matrix[0] > x && Matrix[1] < x && Matrix[2] > y && Matrix[3] < y)
                        {
                            Data[x, y] = 0;
                        }
                    }
                }

                ActiveTerrain.terrainData.SetDetailLayer(0, 0, Layer, Data);
            }
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(ExternalRemoveGrassAddon), true)]
    public class ExternalRemoveGrassAddonInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ExternalRemoveGrassAddon.ShowGizmos = UnityEditor.EditorGUILayout.Toggle("Remove Grass Show Gizmos", ExternalRemoveGrassAddon.ShowGizmos);

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("RemoveGrassRadius"), new GUIContent("Remove Grass Radius"));

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}