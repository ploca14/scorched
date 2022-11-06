using System.Collections;
using System.Collections.Generic;
using Polyperfect.Common;
using UnityEngine;
using UnityEngine.AI;

namespace Polyperfect.Crafting.Demo
{
    public class AutoUpdateSceneNavmesh : PolyMono
    {
        public override string __Usage => $"Automatically creates and updates a navmesh in the region surrounding this object.\nWill automatically enable the {nameof(NavMeshAgent)} on a parent GameObject after the NavMesh is rebuilt.";
        public LayerMask Mask = ~0;
        public float UpdateCooldown = .5f;
        public float Range = 100f;
        public float AgentClimb = .2f;
        public float AgentHeight = 2f;
        public float AgentRadius = .2f;
        public int TileSize = 1;
        public float AgentSlope = 30f;
        public float VoxelSize = .5f;
        public float MinRegionArea = 1f;
        NavMeshAgent agent;
        NavMeshDataInstance navMeshInstance;

        void Awake()
        {
            agent = GetComponentInParent<NavMeshAgent>();
            if (agent)
                agent.enabled = false;
        }

        void Start()
        {
            var bounds = new Bounds(transform.position,Vector3.one*Range);
            
            var settings = CreateSettings();

            var built = NavMeshBuilder.BuildNavMeshData(settings,new List<NavMeshBuildSource>(),bounds,Vector3.zero,Quaternion.identity);
            navMeshInstance = NavMesh.AddNavMeshData(built);
            
            StartCoroutine(UpdateNavmeshLoop(built));
        }

        void OnDestroy()
        {
            NavMesh.RemoveNavMeshData(navMeshInstance);
        }

        IEnumerator UpdateNavmeshLoop(NavMeshData meshData)
        {
            var buildSources = new List<NavMeshBuildSource>();
            var markups = new List<NavMeshBuildMarkup>();
            markups.Add(new NavMeshBuildMarkup(){});
            while (true)
            {
                var settings = CreateSettings();
                var bounds = new Bounds(transform.position,Vector3.one*Range);
                buildSources.Clear();
                NavMeshBuilder.CollectSources(bounds, Mask, NavMeshCollectGeometry.PhysicsColliders, 0, markups, buildSources);
                var operation = NavMeshBuilder.UpdateNavMeshDataAsync(meshData, settings, buildSources, bounds);
                yield return operation;
                agent.enabled = true;
                yield return new WaitForSeconds(UpdateCooldown);
            }
        }
        
        NavMeshBuildSettings CreateSettings()
        {
            return new NavMeshBuildSettings
            { 
                agentClimb = AgentClimb, 
                agentHeight = AgentHeight, 
                agentRadius = AgentRadius, 
                tileSize = TileSize, 
                agentSlope = AgentSlope, 
                voxelSize = VoxelSize, 
                minRegionArea = MinRegionArea,
                overrideTileSize = true,
                overrideVoxelSize = true
            };
        }
    }
}