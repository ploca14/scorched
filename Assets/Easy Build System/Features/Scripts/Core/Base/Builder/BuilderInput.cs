using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;

#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif

namespace EasyBuildSystem.Features.Scripts.Core.Base.Builder
{
    [RequireComponent(typeof(BuilderBehaviour))]
    [AddComponentMenu("Easy Build System/Builders/Inputs/Builder Input Behaviour")]
    public class BuilderInput : MonoBehaviour
    {
        #if EBS_NEW_INPUT_SYSTEM

        #region Fields

        public static BuilderInput Instance;

        public bool UIBlocking = false;

        public bool UsePlacementMode = true;
        public bool ResetModeAfterPlacement = false;
        public float PlacementActionDelay = 0.1f;

        public bool UseDestructionMode = true;
        public bool ResetModeAfterDestruction = false;
        public float DestructionActionDelay = 0.1f;

        public bool UseEditMode = true;
        public bool ResetModeAfterEdit = false;
        public float EditActionDelay = 0.1f;

        public DemoInputActions.BuildingActions building;
        public DemoInputActions.UIActions userInteraface;

        private DemoInputActions Inputs;

        public int SelectedIndex { get; set; }

        private float LastActionTime;

        private bool WheelRotationReleased;
        private bool WheelSelectionReleased;

        #endregion

        #region Methods

        public virtual void OnEnable()
        {
            Inputs.Building.Enable();
            Inputs.UI.Enable();
        }

        public virtual void OnDisable()
        {
            Inputs.Building.Disable();
            Inputs.UI.Disable();
        }

        public virtual void OnDestroy()
        {
            Inputs.Building.Disable();
            Inputs.UI.Disable();
        }

        public virtual void Awake()
        {
            Instance = this;

            Inputs = new DemoInputActions();
            building = Inputs.Building;
            userInteraface = Inputs.UI;
        }

        public virtual void Update()
        {
            if (IsPointerOverUIElement())
                return;

            if (UsePlacementMode && building.Placement.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
            }

            if (UseDestructionMode && building.Destruction.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Destruction);
            }

            if (UseEditMode && building.Edition.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Edit);
            }

            if (BuilderBehaviour.Instance.CurrentMode != BuildMode.Placement)
            {
                UpdatePrefabSelection();
            }

            if (building.Cancel.triggered)
            {
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            }

            if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement)
            {
                if (building.Validate.triggered)
                {
                    if (Time.time > PlacementActionDelay + LastActionTime)
                    {
                        LastActionTime = Time.time;

                        BuilderBehaviour.Instance.PlacePrefab();

                        if (ResetModeAfterPlacement)
                            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);

                        if (ResetModeAfterEdit && BuilderBehaviour.Instance.LastMode == BuildMode.Edit)
                            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                    }
                }

                float WheelAxis = building.Rotate.ReadValue<float>();

                if (WheelAxis > 0 && !WheelRotationReleased)
                {
                    WheelRotationReleased = true;
                    BuilderBehaviour.Instance.RotatePreview(BuilderBehaviour.Instance.SelectedPiece.RotationAxis);
                }
                else if (WheelAxis < 0 && !WheelRotationReleased)
                {
                    WheelRotationReleased = true;
                    BuilderBehaviour.Instance.RotatePreview(-BuilderBehaviour.Instance.SelectedPiece.RotationAxis);
                }
                else if (WheelAxis == 0)
                {
                    WheelRotationReleased = false;
                }

                if (building.Cancel.triggered)
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Edit)
            {
                if (building.Validate.triggered)
                {
                    if (Time.time > EditActionDelay + LastActionTime)
                    {
                        LastActionTime = Time.time;
                        BuilderBehaviour.Instance.EditPrefab();
                    }
                }

                if (building.Cancel.triggered)
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Destruction)
            {
                if (building.Validate.triggered)
                {
                    if (BuilderBehaviour.Instance.CurrentRemovePreview != null)
                    {
                        if (Time.time > DestructionActionDelay + LastActionTime)
                        {
                            LastActionTime = Time.time;
                            BuilderBehaviour.Instance.DestroyPrefab();

                            if (ResetModeAfterDestruction)
                                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                        }
                    }
                }

                if (building.Cancel.triggered)
                {
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                }
            }
        }

        public virtual void UpdatePrefabSelection()
        {
            float WheelAxis = building.Switch.ReadValue<float>();

            if (WheelAxis > 0 && !WheelSelectionReleased)
            {
                WheelSelectionReleased = true;

                if (SelectedIndex < BuildManager.Instance.Pieces.Count - 1)
                {
                    SelectedIndex++;
                }
                else
                {
                    SelectedIndex = 0;
                }
            }
            else if (WheelAxis < 0 && !WheelSelectionReleased)
            {
                WheelSelectionReleased = true;

                if (SelectedIndex > 0)
                {
                    SelectedIndex--;
                }
                else
                {
                    SelectedIndex = BuildManager.Instance.Pieces.Count - 1;
                }
            }
            else if (WheelAxis == 0)
            {
                WheelSelectionReleased = false;
            }

            if (SelectedIndex == -1)
            {
                return;
            }

            if (BuildManager.Instance.Pieces.Count != 0)
            {
                BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[SelectedIndex]);
            }
        }

        /// <summary>
        /// Check if the cursor is above a UI element or if the ciruclar menu is open.
        /// </summary>
        private bool IsPointerOverUIElement()
        {
            if (!UIBlocking)
            {
                return false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                return false;
            }

            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData EventData = new PointerEventData(EventSystem.current)
            {        
                position = new Vector2(UnityEngine.InputSystem.Mouse.current.position.x.ReadValue(), UnityEngine.InputSystem.Mouse.current.position.y.ReadValue())
            };

            List<RaycastResult> Results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(EventData, Results);
            return Results.Count > 0;
        }

        #endregion

        #else

        #region Fields

        public bool UIBlocking = false;

        public bool UsePlacementMode = true;
        public KeyCode PlacementModeKey = KeyCode.E;
        public bool ResetModeAfterPlacement = false;
        public float PlacementActionDelay = 0.1f;

        public bool UseDestructionMode = true;
        public KeyCode DestructionModeKey = KeyCode.R;
        public bool ResetModeAfterDestruction = false;
        public float DestructionActionDelay = 0.1f;

        public bool UseEditMode = true;
        public KeyCode EditModeKey = KeyCode.T;
        public bool ResetModeAfterEdit = false;
        public float EditActionDelay = 0.1f;

        public bool UseMouseWheelForRotation = true;
        public KeyCode RotationActionKey = KeyCode.Tab;

        public bool UseMouseWheelForSelection = true;
        public KeyCode SelectionPositiveActionKey = KeyCode.G;
        public KeyCode SelectionNegativeActionKey = KeyCode.H;

        public KeyCode ValidateActionKey = KeyCode.Mouse0;
        public KeyCode CancelActionKey = KeyCode.Mouse1;

        public int SelectedIndex { get; set; }

        private float LastActionTime;

        #endregion

        #region Methods

        private void OnEnable()
        {
#if UNITY_EDITOR
#if ENABLE_INPUT_SYSTEM
#if !EBS_NEW_INPUT_SYSTEM
            Debug.LogWarning("The new input system is detected on this project.\n" +
                "Please import the cross-platform support from the Package Importer or switch to legacy input manager.\n" +
                "A setup guide to install the cross-platform support is available on the documentation.");
#endif
#endif
#endif
        }

        private void Update()
        {
            if (IsPointerOverUIElement())
                return;

            if (Input.GetKeyDown(PlacementModeKey) && UsePlacementMode)
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);

            if (Input.GetKeyDown(DestructionModeKey) && UseDestructionMode)
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Destruction);

            if (Input.GetKeyDown(EditModeKey) && UseEditMode)
                BuilderBehaviour.Instance.ChangeMode(BuildMode.Edit);

            if (BuilderBehaviour.Instance.CurrentMode != BuildMode.Placement)
                UpdatePrefabSelection();

            if (Input.GetKeyDown(CancelActionKey))
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);

            if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Placement)
            {
                if (Input.GetKeyDown(ValidateActionKey))
                {
                    if (Time.time > PlacementActionDelay + LastActionTime)
                    {
                        LastActionTime = Time.time;

                        BuilderBehaviour.Instance.PlacePrefab(BuilderBehaviour.Instance.NearestGroup);

                        if (ResetModeAfterPlacement)
                            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);

                        if (ResetModeAfterEdit && BuilderBehaviour.Instance.LastMode == BuildMode.Edit)
                            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                    }
                }

                if (UseMouseWheelForRotation)
                {
                    float WheelAxis = Input.GetAxis("Mouse ScrollWheel");

                    if (WheelAxis > 0)
                        BuilderBehaviour.Instance.RotatePreview(BuilderBehaviour.Instance.SelectedPiece.RotationAxis);
                    else if (WheelAxis < 0)
                        BuilderBehaviour.Instance.RotatePreview(-BuilderBehaviour.Instance.SelectedPiece.RotationAxis);
                }
                else
                {
                    if (Input.GetKeyDown(RotationActionKey))
                        BuilderBehaviour.Instance.RotatePreview(BuilderBehaviour.Instance.SelectedPiece.RotationAxis);
                }

                if (Input.GetKeyDown(CancelActionKey))
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Edit)
            {
                if (Input.GetKeyDown(ValidateActionKey))
                {
                    if (Time.time > EditActionDelay + LastActionTime)
                    {
                        LastActionTime = Time.time;
                        BuilderBehaviour.Instance.EditPrefab();
                    }
                }

                if (Input.GetKeyDown(CancelActionKey))
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            }
            else if (BuilderBehaviour.Instance.CurrentMode == BuildMode.Destruction)
            {
                if (Input.GetKeyDown(ValidateActionKey))
                {
                    if (BuilderBehaviour.Instance.CurrentRemovePreview != null)
                    {
                        if (Time.time > DestructionActionDelay + LastActionTime)
                        {
                            LastActionTime = Time.time;
                            BuilderBehaviour.Instance.DestroyPrefab();

                            if (ResetModeAfterDestruction)
                                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
                        }
                    }
                }

                if (Input.GetKeyDown(CancelActionKey))
                    BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
            }
        }

        private void UpdatePrefabSelection()
        {
            if (UseMouseWheelForSelection)
            {
                float WheelAxis = Input.GetAxis("Mouse ScrollWheel");

                if (WheelAxis > 0)
                {
                    if (SelectedIndex < BuildManager.Instance.Pieces.Count - 1)
                        SelectedIndex++;
                    else
                        SelectedIndex = 0;
                }
                else if (WheelAxis < 0)
                {
                    if (SelectedIndex > 0)
                        SelectedIndex--;
                    else
                        SelectedIndex = BuildManager.Instance.Pieces.Count - 1;
                }

                if (SelectedIndex == -1)
                    return;

                if (BuildManager.Instance.Pieces.Count != 0)
                    BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[SelectedIndex]);
            }
            else
            {
                if (Input.GetKeyDown(SelectionPositiveActionKey))
                {
                    if (SelectedIndex < BuildManager.Instance.Pieces.Count - 1)
                        SelectedIndex++;
                    else
                        SelectedIndex = 0;
                }

                if (Input.GetKeyDown(SelectionNegativeActionKey))
                {
                    if (SelectedIndex > 0)
                        SelectedIndex--;
                    else
                        SelectedIndex = BuildManager.Instance.Pieces.Count - 1;
                }

                if (SelectedIndex == -1)
                    return;

                if (BuildManager.Instance.Pieces.Count != 0)
                    BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.Pieces[SelectedIndex]);
            }
        }

        private bool IsPointerOverUIElement()
        {
            if (!UIBlocking)
            {
                return false;
            }

            if (Cursor.lockState == CursorLockMode.Locked)
            {
                return false;
            }

            if (EventSystem.current == null)
            {
                return false;
            }

            PointerEventData EventData = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(Input.mousePosition.x, Input.mousePosition.y)
            };

            List<RaycastResult> Results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(EventData, Results);
            return Results.Count > 0;
        }

        #endregion

        #endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BuilderInput), true)]
    public class BuilderInputEditor : Editor
    {
        #region Fields

        private static bool[] FoldoutArray = new bool[1];

        #endregion Fields

        #region Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Builder Input - Component");

            GUILayout.Label("Manages the selection of build modes and pieces via keyboard & mouse shortcuts.\n" +
                "You can import the Cross-platform support to use this component with other controllers.\n" +
                "Find more information about this component in the documentation.", EditorStyles.miniLabel);

#if EBS_NEW_INPUT_SYSTEM
            EditorGUILayout.HelpBox("New Input System was detected, the keyboard inputs have been replaced by the universal inputs.\n" +
                "You can change the input actions settings in the file.", MessageType.Info);

            if (GUILayout.Button("Edit Input Action Settings..."))
            {
                if (Resources.Load<UnityEngine.InputSystem.InputActionAsset>("Demo - Input Actions") != null)
                    Selection.activeObject = Resources.Load<UnityEngine.InputSystem.InputActionAsset>("Demo - Input Actions");
                else
                    Debug.LogWarning("Demo - Input Actions could be not found, the file not existing or has been moved or renamed.");
            }
#endif

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UIBlocking"),
                    new GUIContent("UI Blocking :", "Avoid using the shortcuts action when the cursor is above a UI element."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UsePlacementMode"),
                    new GUIContent("Use Placement Keyboard Shortcut :", "Use placement mode."));

                if (serializedObject.FindProperty("UsePlacementMode").boolValue)
                {
                    EditorGUI.indentLevel = 1;

#if !EBS_NEW_INPUT_SYSTEM
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PlacementModeKey"),
                        new GUIContent("Placement Mode Key :", "Input key to pass in placement mode."));
#endif

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("PlacementActionDelay"),
                        new GUIContent("Placement Interval :", "Delay in milliseconds after a piece is placed."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterPlacement"),
                        new GUIContent("Reset Mode After Placement :", "Reset the placement mode after a piece is placed."));

                    EditorGUI.indentLevel = 0;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseDestructionMode"),
                    new GUIContent("Use Destruction Keyboard Shortcut :", "Use destruction mode."));

                if (serializedObject.FindProperty("UseDestructionMode").boolValue)
                {
                    EditorGUI.indentLevel = 1;

#if !EBS_NEW_INPUT_SYSTEM
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DestructionModeKey"),
                        new GUIContent("Destruction Mode Key :", "Input key to pass in destruction mode."));
#endif

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DestructionActionDelay"),
                        new GUIContent("Destruction Interval :", "Delay in milliseconds after a piece is destroyed."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterDestruction"),
                        new GUIContent("Reset Mode After Destruction :", "Reset the placement mode after a piece is destroyed."));

                    EditorGUI.indentLevel = 0;
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseEditMode"),
                    new GUIContent("Use Edit Keyboard Shortcut :", "Use edit mode."));

                if (serializedObject.FindProperty("UseEditMode").boolValue)
                {
                    EditorGUI.indentLevel = 1;

#if !EBS_NEW_INPUT_SYSTEM
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("EditModeKey"),
                        new GUIContent("Edit Mode Key :", "Input key to pass in edit mode."));
#endif

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("EditActionDelay"),
                        new GUIContent("Edit Interval :", "Delay in milliseconds after a piece is edited."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ResetModeAfterEdit"),
                        new GUIContent("Reset Mode After Edit :", "Reset the placement mode after a piece is edited."));

                    EditorGUI.indentLevel = 0;
                }

#if !EBS_NEW_INPUT_SYSTEM
                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseMouseWheelForRotation"),
                    new GUIContent("Use Mouse Wheel For Rotation :", "Use the mousewheel for rotate the current preview."));

                if (!serializedObject.FindProperty("UseMouseWheelForRotation").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("RotationActionKey"),
                        new GUIContent("Rotation Action Key :", "Input key for rotate the current preview."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("UseMouseWheelForSelection"),
                    new GUIContent("Use Mouse Wheel For Selection :", "Use the mousewheel for the pieces selection."));

                if (!serializedObject.FindProperty("UseMouseWheelForSelection").boolValue)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SelectionPositiveActionKey"),
                        new GUIContent("Selection Positive Action Key :", "Input key for pieces selection."));

                    EditorGUILayout.PropertyField(serializedObject.FindProperty("SelectionNegativeActionKey"),
                        new GUIContent("Selection Negative Action Key :", "Input key for pieces selection."));
                }

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ValidateActionKey"),
                    new GUIContent("Validate Action Key :", "Input key to validate the current action."));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CancelActionKey"),
                    new GUIContent("Cancel Action Key:", "Input key to cancel the current action."));
#endif
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

#endregion
    }
#endif
}