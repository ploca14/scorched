using EasyBuildSystem.Features.Scripts.Core.Base.Group;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
using EasyBuildSystem.Features.Scripts.Core.Base.Storage.Data;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Scriptables.Blueprint
{
    public class BlueprintTemplate : ScriptableObject
    {
        #region Fields

        public PieceData Model = new PieceData();
        public string Data;
        public string SourceSceneName;

        #endregion Fields
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BlueprintTemplate), true)]
    public class BlueprintEditor : Editor
    {
        #region Fields

        private BlueprintTemplate Target;
        private Vector2 TextScrollPosition;

        #endregion Fields

        #region Methods

        private void OnEnable()
        {
            Target = (BlueprintTemplate)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            #region Blueprint Data General

            InspectorStyles.DrawSectionLabel("Blueprint Template - Component");

            GUILayout.Label("Contains the position, rotation, scale of all the pieces of a Group Behaviour.\n" +
                "Find more information about this component on the documentation.", EditorStyles.miniLabel);

            if (Target.Model == null)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label("The list does not contains of piece(s).");
                GUILayout.EndHorizontal();
            }
            else
            {
                PieceData.SerializedPiece[] Pieces = new PieceData.SerializedPiece[0];

                if (Target.Data != null)
                {
                    if (Target.Data != string.Empty && Target.Data.Length > 0)
                    {
                        Pieces = Target.Model.DecodeToStr(Target.Data);
                    }
                }

                GUILayout.Label("Generated on the scene : " + Target.SourceSceneName);

                GUILayout.Label("Number of pieces in the blueprint : " + Pieces.Length);

                GUILayout.Label("Generated blueprint data :");

                EditorGUILayout.HelpBox("The below data has been generated with specific scene settings.\n" +
                    "Any change on this data or of pieces in the Build Manager could corrupt the template.", MessageType.Warning);

                TextScrollPosition = GUILayout.BeginScrollView(TextScrollPosition, GUILayout.Height(200));

                EditorGUI.BeginChangeCheck();

                Target.Data = EditorGUILayout.TextArea(Target.Data, GUILayout.ExpandHeight(true));

                if (EditorGUI.EndChangeCheck())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }

                GUILayout.EndScrollView();

                GUI.enabled = Target.Model != null && Target.Data != string.Empty;

                if (GUILayout.Button("(Editor) Load Blueprint Template..."))
                {
                    if (BuildManager.Instance == null)
                    {
                        Debug.LogError("<b>Easy Build System</b> : The Build Manager does not exists.");
                        return;
                    }

                    PieceData.SerializedPiece[] SerializedPieces = Target.Model.DecodeToStr(Target.Data);

                    GroupBehaviour Group = new GameObject("(Editor) New Blueprint " + Target.name).AddComponent<GroupBehaviour>();

                    for (int i = 0; i < SerializedPieces.Length; i++)
                    {
                        PieceBehaviour InstantiatedPiece = BuildManager.Instance.PlacePrefab(BuildManager.Instance.GetPieceById(SerializedPieces[i].Id),
                            PieceData.ParseToVector3(SerializedPieces[i].Position),
                            PieceData.ParseToVector3(SerializedPieces[i].Rotation),
                            PieceData.ParseToVector3(SerializedPieces[i].Scale), Group);

                        InstantiatedPiece.ChangeSkin(SerializedPieces[i].SkinIndex);
                    }
                }

                GUI.enabled = Application.isPlaying;

                if (GUILayout.Button("(Runtime) Load Blueprint Template..."))
                {
                    if (BuildManager.Instance == null)
                    {
                        Debug.LogError("<b>Easy Build System</b> : The Build Manager does not exists.");

                        return;
                    }

                    PieceData.SerializedPiece[] SerializedPieces = Target.Model.DecodeToStr(Target.Data);

                    GroupBehaviour Group = new GameObject("(Runtime) New Blueprint " + Target.name).AddComponent<GroupBehaviour>();

                    for (int i = 0; i < SerializedPieces.Length; i++)
                    {
                        PieceBehaviour InstantiatedPiece = BuildManager.Instance.PlacePrefab(BuildManager.Instance.GetPieceById(SerializedPieces[i].Id),
                            PieceData.ParseToVector3(SerializedPieces[i].Position),
                            PieceData.ParseToVector3(SerializedPieces[i].Rotation),
                            PieceData.ParseToVector3(SerializedPieces[i].Scale), Group);

                        InstantiatedPiece.ChangeSkin(SerializedPieces[i].SkinIndex);
                    }
                }

                GUI.enabled = true;
            }

            #endregion Blueprint Data General

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }
#endif
}