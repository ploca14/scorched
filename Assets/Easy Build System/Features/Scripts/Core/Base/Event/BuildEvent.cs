using System;

using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Socket;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;

#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Event
{
    [DefaultExecutionOrder(-999)]
    public class BuildEvent : MonoBehaviour
    {
        #region Fields

        public static BuildEvent Instance;
                  
        [Serializable] public class StorageSaving : UnityEvent { }
        public StorageSaving OnStorageSaving;

        [Serializable] public class StorageLoading : UnityEvent { }
        public StorageLoading OnStorageLoading;

        [Serializable] public class StorageLoadingResult : UnityEvent<PieceBehaviour[]> { }
        public StorageLoadingResult OnStorageLoadingResult;

        [Serializable] public class StorageSavingResult : UnityEvent<PieceBehaviour[]> { }
        public StorageSavingResult OnStorageSavingResult;

        [Serializable] public class PieceInstantiated : UnityEvent<PieceBehaviour, SocketBehaviour> { }
        public PieceInstantiated OnPieceInstantiated;

        [Serializable] public class PieceDestroyed : UnityEvent<PieceBehaviour> { }
        public PieceDestroyed OnPieceDestroyed;

        [Serializable] public class PieceChangedState : UnityEvent<PieceBehaviour, StateType> { }
        public PieceChangedState OnPieceChangedState;

        [Serializable] public class PieceChangedSkin : UnityEvent<PieceBehaviour, int> { }
        public PieceChangedSkin OnPieceChangedSkin;

        [Serializable] public class GroupInstantiated : UnityEvent<GroupBehaviour> { }
        public GroupInstantiated OnGroupInstantiated;

        [Serializable] public class GroupUpdated : UnityEvent<GroupBehaviour> { }
        public GroupUpdated OnGroupUpdated;

        [Serializable] public class ChangedBuildMode : UnityEvent<BuildMode> { }
        public ChangedBuildMode OnChangedBuildMode;

        [Serializable] public class ChangedSocketState : UnityEvent<SocketBehaviour, bool> { }
        public ChangedSocketState OnChangedSocketState;

        #endregion

        #region Methods

        private void Awake()
        {
            Instance = this;
        }

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BuildEvent))]
    public class BuildEventInspector : Editor
    {
        #region Fields

        private static bool[] FoldoutArray = new bool[6];

        #endregion Fields

        #region Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region Build Event General

            InspectorStyles.DrawSectionLabel("Build Event - Component");

            GUILayout.Label("Manages all the events which are triggered to each system action.\n" +
                "Find more information about this component in the documentation.", EditorStyles.miniLabel);

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "Builder Behaviour Events", true);
                GUILayout.EndHorizontal();

                if (FoldoutArray[1])
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnChangedBuildMode"), new GUIContent("OnChangedBuildMode", "Define the origin offset."), true);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                FoldoutArray[2] = EditorGUILayout.Foldout(FoldoutArray[2], "Piece Behaviour Events", true);
                GUILayout.EndHorizontal();

                if (FoldoutArray[2])
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPieceInstantiated"), new GUIContent("OnPieceInstantiated", "Define the origin offset."), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPieceDestroyed"), new GUIContent("OnPieceDestroyed", "Define the origin offset."), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPieceChangedState"), new GUIContent("OnPieceChangedState", "Define the origin offset."), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnPieceChangedSkin"), new GUIContent("OnPieceChangedSkin", "Define the origin offset."), true);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                FoldoutArray[3] = EditorGUILayout.Foldout(FoldoutArray[3], "Socket Behaviour Events", true);
                GUILayout.EndHorizontal();

                if (FoldoutArray[3])
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnChangedSocketState"), new GUIContent("OnChangedSocketState", "Define the origin offset."), true);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                FoldoutArray[4] = EditorGUILayout.Foldout(FoldoutArray[4], "Group Behaviour Events", true);
                GUILayout.EndHorizontal();

                if (FoldoutArray[4])
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnGroupInstantiated"), new GUIContent("OnGroupInstantiated", "Define the origin offset."), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnGroupUpdated"), new GUIContent("OnGroupUpdated", "Define the origin offset."), true);
                }

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                FoldoutArray[5] = EditorGUILayout.Foldout(FoldoutArray[5], "Build Storage Events", true);
                GUILayout.EndHorizontal();

                if (FoldoutArray[5])
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStorageSaving"), new GUIContent("OnStorageSaving", "Define the origin offset."), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStorageLoading"), new GUIContent("OnStorageLoading", "Define the origin offset."), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStorageLoadingResult"), new GUIContent("OnStorageLoadingResult", "Define the origin offset."), true);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("OnStorageSavingResult"), new GUIContent("OnStorageSavingResult", "Define the origin offset."), true);
                }
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Fields
    }
#endif
}