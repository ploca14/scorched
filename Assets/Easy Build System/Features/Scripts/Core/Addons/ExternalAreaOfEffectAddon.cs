using EasyBuildSystem.Features.Scripts.Core.Base.Addon;
using EasyBuildSystem.Features.Scripts.Core.Base.Addon.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Addons
{
    [Addon("External Area Of Effect Add-On", "Allows to disable the areas, pieces, sockets which are far from the camera to improve the performances.\n" +
        "You can find more information about this component in the documentation.", AddonTarget.BuilderBehaviour)]
    public class ExternalAreaOfEffectAddon : AddonBehaviour
    {
        #region Fields

        public bool AffectAreas = true;
        public bool AffectParts = false;
        public bool AffectSockets = true;
        public float Radius = 30f;
        public float RefreshInterval = 0.5f;

        #endregion Fields

        #region Methods

        private void Start()
        {
            InvokeRepeating("Refresh", RefreshInterval, RefreshInterval);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }

        private void Refresh()
        {
            if (AffectAreas)
                for (int i = 0; i < BuildManager.Instance.CachedAreas.Count; i++)
                    BuildManager.Instance.CachedAreas[i].gameObject.SetActive((Vector3.Distance(transform.position, BuildManager.Instance.CachedAreas[i].transform.position) <= Radius));

            if (AffectParts)
                for (int i = 0; i < BuildManager.Instance.CachedParts.Count; i++)
                    BuildManager.Instance.CachedParts[i].gameObject.SetActive((Vector3.Distance(transform.position, BuildManager.Instance.CachedParts[i].transform.position) <= Radius));

            if (AffectSockets)
                for (int i = 0; i < BuildManager.Instance.CachedSockets.Count; i++)
                    BuildManager.Instance.CachedSockets[i].gameObject.SetActive(Vector3.Distance(transform.position, BuildManager.Instance.CachedSockets[i].transform.position) <= Radius);
        }

        #endregion Methods
    }

#if UNITY_EDITOR

    [UnityEditor.CustomEditor(typeof(ExternalAreaOfEffectAddon), true)]
    public class ExternalAreaOfEffectAddonInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("Radius"), new GUIContent("Area Of Effect Radius :"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("RefreshInterval"), new GUIContent("Area Of Effect Refresh Interval :"));

            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("AffectAreas"), new GUIContent("Area Of Effect Affect Areas :"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("AffectParts"), new GUIContent("Area Of Effect Affect Pieces :"));
            UnityEditor.EditorGUILayout.PropertyField(serializedObject.FindProperty("AffectSockets"), new GUIContent("Area Of Effect Affect Sockets :"));

            serializedObject.ApplyModifiedProperties();
        }
    }

#endif
}