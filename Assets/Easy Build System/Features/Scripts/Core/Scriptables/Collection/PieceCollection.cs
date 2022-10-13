using EasyBuildSystem.Features.Scripts.Core.Base.Piece;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif
using EasyBuildSystem.Features.Scripts.Extensions;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Scriptables.Collection
{
    public class PieceCollection : ScriptableObject
    {
        #region Fields

        public List<PieceBehaviour> Pieces = new List<PieceBehaviour>();

        #endregion Fields
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(PieceCollection))]
    public class PieceCollectionInspector : Editor
    {
        #region Fields

        private PieceCollection Target { get { return (PieceCollection)target; } }

        private PieceCollection[] Pieces;
        private string[] Options;
        private int Selection;

        private bool[] PieceFoldout = new bool[999];
        private readonly List<Editor> PiecePreviews = new List<Editor>();

        #endregion Fields

        #region Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Piece Collection - Component");

            GUILayout.Label("Contains a list of pieces that can be loaded into the Build Manager via the inspector.\n" +
                "Find more information about this component on the documentation.", EditorStyles.miniLabel);

            GUILayout.Space(5);

            bool Flag = false;

            if (PieceFoldout == null)
            {
                PieceFoldout = new bool[Target.Pieces.Count];
            }

            for (int i = 0; i < Target.Pieces.Count; i++)
            {
                if (Target.Pieces[i] == null)
                {
                    Flag = true;
                }
            }

            if (Flag)
            {
                Target.Pieces = Target.Pieces.Where(s => s != null).Distinct().ToList();
            }

            int Index = 0;

            GUI.color = Color.black / 4;
            GUILayout.BeginVertical("helpBox");
            GUI.color = Color.white;

            if (Target.Pieces.Count == 0)
            {
                GUILayout.Label("Pieces list does not contains any piece(s).");
            }
            else
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Sort By Name"))
                {
                    Target.Pieces = Target.Pieces.OrderBy(e => e.Name).ToList();
                }

                if (GUILayout.Button("Sort By Id"))
                {
                    Target.Pieces = Target.Pieces.OrderBy(e => e.Id).ToList();
                }

                GUILayout.EndHorizontal();

                foreach (PieceBehaviour Piece in Target.Pieces)
                {
                    if (Piece == null)
                    {
                        return;
                    }

                    GUILayout.BeginHorizontal();

                    GUILayout.Space(13);

                    EditorGUI.BeginChangeCheck();

                    string Format = string.Format("[{0}] ", Index) + Piece.Name;

                    GUILayout.BeginHorizontal();

                    PieceFoldout[Index] = EditorGUILayout.Foldout(PieceFoldout[Index],
                        Format, true);

                    GUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        if (PieceFoldout[Index] == true)
                        {
                            for (int i = 0; i < PieceFoldout.Length; i++)
                            {
                                if (i != Index)
                                {
                                    PieceFoldout[i] = false;
                                }
                            }
                        }

                        PiecePreviews.Clear();
                    }

                    GUI.color = Color.white;

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    if (PieceFoldout[Index])
                    {
                        GUI.color = Color.black / 4;
                        GUILayout.BeginHorizontal("helpBox");
                        GUI.color = Color.white;

                        GUILayout.BeginVertical();

                        if (Piece != null)
                        {
                            UnityEditor.Editor PreviewEditor = null;

                            if (PiecePreviews.Count > Index)
                            {
                                PreviewEditor = PiecePreviews[Index];
                            }

                            if (PreviewEditor == null)
                            {
                                PreviewEditor = CreateEditor(Piece.gameObject);

                                PiecePreviews.Add(PreviewEditor);

                                PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128, 128, 128), EditorStyles.textArea);
                            }
                            else
                            {
                                PreviewEditor.OnPreviewGUI(GUILayoutUtility.GetRect(128, 128, 128, 128), EditorStyles.textArea);
                            }

                            EditorGUILayout.ObjectField(serializedObject.FindProperty("Pieces").GetArrayElementAtIndex(Index), new GUIContent("Piece Behaviour :"));

                            if (GUILayout.Button("Remove Piece"))
                            {
                                Undo.RecordObject(target, "Remove Piece");
                                Target.Pieces.Remove(Piece);
                                Repaint();
                                EditorUtility.SetDirty(target);
                                PiecePreviews.Clear();
                                break;
                            }
                        }

                        GUILayout.EndVertical();

                        GUILayout.EndHorizontal();
                    }

                    GUILayout.EndHorizontal();

                    Index++;
                }
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Clear All Piece(s) List", GUILayout.MinWidth(200)))
            {
                Undo.RecordObject(target, "Cancel Clear List");
                Target.Pieces.Clear();
                Repaint();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("All Piece(s) List To Piece Collection", GUILayout.MinWidth(200)))
            {
                PieceCollection Collection = ScriptableObjectExtension.CreateAsset<PieceCollection>("New Piece Collection...");
                Collection.Pieces.AddRange(Target.Pieces);
                Repaint();
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            GUI.color = Color.black / 4;
            GUILayout.BeginVertical("helpBox");
            GUI.color = Color.white;

            Rect DropRect = GUILayoutUtility.GetRect(0, 20, GUILayout.ExpandWidth(true));

            GUI.Box(DropRect, "Drag & Drop your pieces or pieces collection here to add them in the list.", EditorStyles.centeredGreyMiniLabel);

            if (DropRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.DragPerform)
                {
                    for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                    {
                        if (DragAndDrop.objectReferences[i] is PieceCollection)
                        {
                            Target.Pieces.AddRange(((PieceCollection)DragAndDrop.objectReferences[i]).Pieces);
                            EditorUtility.SetDirty(target);
                            Repaint();
                        }
                        else
                        {
                            GameObject DraggedObject = DragAndDrop.objectReferences[i] as GameObject;

                            if (DraggedObject == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : Cannot add empty object!");
                                return;
                            }

                            if (!PrefabUtility.IsPartOfPrefabAsset(DraggedObject))
                            {
                                DraggedObject = PrefabUtility.GetCorrespondingObjectFromSource(DraggedObject);

                                if (DraggedObject == null)
                                {
                                    Debug.LogError("<b>Easy Build System</b> : Object have not PieceBehaviour component or the prefab is not the original.");
                                    return;
                                }
                            }

                            PieceBehaviour DraggedPiece = DraggedObject.GetComponent<PieceBehaviour>();

                            if (DraggedPiece == null)
                            {
                                Debug.LogError("<b>Easy Build System</b> : Only piece can be added to list!");
                                return;
                            }

                            if (Target.Pieces.Find(entry => entry.Id == DraggedPiece.Id) == null)
                            {
                                Target.Pieces.Add(DraggedPiece);
                                EditorUtility.SetDirty(target);
                                Repaint();
                            }
                            else
                            {
                                Debug.LogError("<b>Easy Build System</b> : The piece already exists in the list.");
                            }
                        }
                    }

                    Event.current.Use();
                }
            }

            EditorGUI.BeginChangeCheck();
            Selection = EditorGUILayout.Popup("Load Piece Collection", Selection, Options);

            if (EditorGUI.EndChangeCheck())
            {
                if (Selection - 1 != -1)
                {
                    Undo.RecordObject(target, "Cancel Add Piece Collection");
                    Target.Pieces.AddRange(Pieces[Selection - 1].Pieces);
                    EditorUtility.SetDirty(target);
                    Repaint();
                    Selection = 0;
                }
            }

            GUILayout.EndVertical();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            Pieces = ListExtension.FindAssetsByType<PieceCollection>().ToArray();
            Options = new string[Pieces.Length + 1];
            Options[0] = "Select Collection...";
            for (int i = 0; i < Pieces.Length; i++)
            {
                Options[i + 1] = Pieces[i].name;
            }

            PieceFoldout = new bool[999];
        }

        #endregion Methods
    }

#endif
}