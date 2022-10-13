using System;
using Polyperfect.Common;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Polyperfect.Crafting.Edit
{
    public class ObjectEditWindow : EditorWindow
    {
        //depending on setup windows can go behind the main view. This is to avoid aggregating hundreds of hidden unfocused windows in such a case
        const int AUTO_CLOSE_DELAY_MS = 120000;

        IVisualElementScheduledItem closeSchedule; //handles closing the window if it's unfocused for a long enough time
        bool disallowInitialize;
        VisualElement editorElement;
        VisualElement rootScroller;
        Object target; //persist between domain reloads
        Action updateEvent;
        public TextField NameEditField { get; private set; }

        void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndo;
            if (target)
                TryInitialize();
        }

        void OnDisable()
        {
            DoUpdate();
            Undo.undoRedoPerformed -= HandleUndo;
            disallowInitialize = false;
        }

        void OnFocus()
        {
            closeSchedule?.Pause();
            updateEvent?.Invoke();
        }


        void OnLostFocus()
        {
            closeSchedule?.ExecuteLater(AUTO_CLOSE_DELAY_MS);
            DoUpdate();
        }

        public static ObjectEditWindow CreateForObject(Object obj, Action updateEvent = null)
        {
            if (!obj)
            {
                Debug.LogError("The object does not exist or has been destroyed.");
                return null;
            }

            var window = CreateInstance<ObjectEditWindow>();
            window.rootVisualElement.Add(new Label("Loading..."));
            window.ShowUtility();
            window.rootVisualElement.schedule.Execute(() =>
            {
                window.rootVisualElement.Clear();
                window.updateEvent = updateEvent;
                window.target = obj;
                window.TryInitialize();
            });
            return window;
        }

        public void FocusNameEditor()
        {
            rootVisualElement.schedule.Execute(() => NameEditField.Q("unity-text-input")?.Focus());
        }

        VisualElement AssignEditor(Object obj)
        {
            if (!obj)
                throw new Exception("Null object");
            editorElement?.Unbind();
            editorElement?.RemoveFromHierarchy();
            NameEditField.SetValueWithoutNotify(obj.name);
            UpdateTitle(obj.name);
            var editor = Editor.CreateEditor(obj);
            editorElement = editor.CreateInspectorGUI();
            editorElement.Bind(new SerializedObject(obj));
            editorElement.SetGrow();
            rootScroller.Add(editorElement);
            return editorElement;
        }

        void TryInitialize()
        {
            if (disallowInitialize)
                return;

            try
            {
                rootScroller = new ScrollView().SetGrow();
                //rootScroller.contentContainer.SetGrow();
                var row = new VisualElement().SetRow();
                NameEditField = new TextField {name = "name-edit-field"}.SetGrow();
                NameEditField.isDelayed = true;
                NameEditField.RegisterValueChangedCallback(HandleNameChange);

                row.Add(NameEditField);
                row.Add(new Button(HandleDelete) {text = "Delete"});
                row.style.marginBottom = 16f;

                rootScroller.Add(row);
                rootVisualElement.Add(rootScroller);

                if (target)
                    AssignEditor(target);


                (closeSchedule = rootVisualElement.schedule.Execute(Close)).Pause();
                disallowInitialize = true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Error when initializing window for {target.name}\n{e}");
                Close();
            }
        }

        void HandleDelete()
        {
            if (EditorUtility.DisplayDialog("Confirm Delete", $"Are you sure you want to delete {target.name}? This cannot be undone.", "Delete", "Cancel"))
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(target));
                Close();
            }
        }

        void HandleNameChange(ChangeEvent<string> evt)
        {
            var errorString = AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(target), evt.newValue);
            if (!string.IsNullOrEmpty(errorString))
            {
                Debug.LogError(errorString);
                NameEditField.SetValueWithoutNotify(evt.previousValue);
                return;
            }

            var newName = evt.newValue;
            target.name = newName;
            UpdateTitle(newName);
            EditorUtility.SetDirty(target);
        }

        void UpdateTitle(string newName)
        {
            titleContent = new GUIContent($"{newName} ({target.GetType().Name})");
        }

        void DoUpdate()
        {
            try
            {
                updateEvent?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception when executing update function from {nameof(ObjectEditWindow)}:\n{e}");
            }
        }

        void HandleUndo()
        {
            try
            {
                AssignEditor(target);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error doing undo for thing. Removing.\n{e}");

                Undo.undoRedoPerformed -= HandleUndo;
                DestroyImmediate(this);
            }
        }
    }
}