using EasyBuildSystem.Features.Scripts.Core.Base.Builder;
using EasyBuildSystem.Features.Scripts.Core.Base.Builder.Enums;
using EasyBuildSystem.Features.Scripts.Core.Base.Manager;
#if UNITY_EDITOR
using EasyBuildSystem.Features.Scripts.Core.Inspectors;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

#if EBS_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

#if EBS_XR
using UnityEngine.XR;
#endif

using UnityEngine.UI;

namespace EasyBuildSystem.Addons.CircularMenu.Scripts
{
    public class CircularMenu : MonoBehaviour
    {
        #region Fields

        public static CircularMenu Instance;

        [Serializable]
        public class UICustomCategory
        {
            public string Name;
            public GameObject Content;
            public List<CircularButtonData> Buttons = new List<CircularButtonData>();
            public List<CircularButton> InstancedButtons = new List<CircularButton>();
        }

        public enum ControllerType
        {
            KeyboardAndMouse,
            Android,
            Gamepad,
            XR
        }

        public ControllerType Controller = ControllerType.KeyboardAndMouse;

        public int DefaultCategoryIndex;

        public List<UICustomCategory> Categories = new List<UICustomCategory>();

        public MonoBehaviour[] DisableComponentsWhenShown;

        public Image Selection;
        public Image SelectionIcon;
        public bool SelectionIconPreserveAspect = true;
        public Text SelectionText;
        public Text SelectionDescription;
        public Color ButtonNormalColor;
        public Color ButtonHoverColor;
        public Color ButtonPressedColor;

        public CircularButton CircularButton;
        public float ButtonSpacing = 160f;
        public bool ButtonImagePreserveAspect = true;

        public Animator Animator;
        public string ShowStateName;
        public string HideStateName;

        [HideInInspector]
        public GameObject SelectedButton;

        [HideInInspector]
        public UICustomCategory CurrentCategory;

        public bool IsActive = false;

        private readonly List<float> ButtonsRotation = new List<float>();
        private int Elements;
        private float Fill;
        private float CurrentRotation;

        #endregion

        #region Methods

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            for (int i = 0; i < Categories.Count; i++)
            {
                if (Categories[i].Content != null)
                {
                    Categories[i].Buttons = Categories[i].Buttons.OrderBy(o => o.Order).ToList();

                    for (int x = 0; x < Categories[i].Buttons.Count; x++)
                    {
                        CircularButton Button = Instantiate(CircularButton, Categories[i].Content.transform);

                        if (Button.Icon != null)
                            Button.Icon.preserveAspect = ButtonImagePreserveAspect;

                        Button.Init(Categories[i].Buttons[x].Text, Categories[i].Buttons[x].Description, Categories[i].Buttons[x].Icon, Categories[i].Buttons[x].Action);
                        Categories[i].InstancedButtons.Add(Button);
                    }

                    Categories[i].InstancedButtons = Categories[i].Content.GetComponentsInChildren<CircularButton>(true).ToList();
                }
            }

            ChangeCategory(Categories[0].Name);

#if EBS_NEW_INPUT_SYSTEM
            BuilderInput.Instance.userInteraface.CircularMenu.performed += ctx => { Show(); };
            BuilderInput.Instance.userInteraface.CircularMenu.canceled += ctx => { Hide(); };
#endif
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

#if EBS_NEW_INPUT_SYSTEM
            if (BuilderInput.Instance.userInteraface.Cancel.triggered)
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
#else

            if (Input.GetKeyDown(KeyCode.Tab))
            { 
                if (!IsActive) 
                    Show();
                else 
                    Hide();
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
                BuilderBehaviour.Instance.ChangeMode(BuildMode.None);
#endif

            if (!IsActive)
                return;

            Selection.fillAmount = Mathf.Lerp(Selection.fillAmount, Fill, .2f);

#if EBS_NEW_INPUT_SYSTEM
            if (Controller == ControllerType.Gamepad)
            {
                Vector2 InputAxis = BuilderInput.Instance.userInteraface.Select.ReadValue<Vector2>();

                if (Mathf.Abs(InputAxis.x) > 0.25f || Mathf.Abs(InputAxis.y) > 0.25f)
                    CurrentRotation = Mathf.Atan2(InputAxis.x, InputAxis.y) * 57.29578f;
            }
            else if (Controller == ControllerType.XR)
            {
#if EBS_XR
                UnityEngine.XR.InputDevice Device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
                Device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 InputAxis);

                if (Mathf.Abs(InputAxis.x) > 0.25f || Mathf.Abs(InputAxis.y) > 0.25f)
                    CurrentRotation = Mathf.Atan2(InputAxis.x, InputAxis.y) * 57.29578f;
#endif
            }
            else if (Controller == ControllerType.KeyboardAndMouse || Controller == ControllerType.Android)
            {
                Vector3 BoundsScreen = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
                Vector3 RelativeBounds = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0f) - BoundsScreen;
                CurrentRotation = Mathf.Atan2(RelativeBounds.x, RelativeBounds.y) * 57.29578f;
            }
#else
            Vector3 BoundsScreen = new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, 0f);
            Vector3 RelativeBounds = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f) - BoundsScreen;
            CurrentRotation = Mathf.Atan2(RelativeBounds.x, RelativeBounds.y) * 57.29578f;
            #endif

            if (CurrentRotation < 0f)
                CurrentRotation += 360f;

            _ = -(CurrentRotation - Selection.fillAmount * 360f / 2f);

            float Average = 9999;

            GameObject Nearest = null;

            for (int i = 0; i < Elements; i++)
            {
                GameObject InstancedButton = CurrentCategory.InstancedButtons[i].gameObject;
                InstancedButton.transform.localScale = Vector3.one;
                float Rotation = Convert.ToSingle(InstancedButton.name);

                if (Mathf.Abs(Rotation - CurrentRotation) < Average)
                {
                    Nearest = InstancedButton;
                    Average = Mathf.Abs(Rotation - CurrentRotation);
                }
            }

            SelectedButton = Nearest;
            float CursorRotation = -(Convert.ToSingle(SelectedButton.name) - Selection.fillAmount * 360f / 2f);
            Selection.transform.localRotation = Quaternion.Slerp(Selection.transform.localRotation, Quaternion.Euler(0, 0, CursorRotation), 15f * Time.deltaTime);

            for (int i = 0; i < Elements; i++)
            {
                CircularButton Button = CurrentCategory.InstancedButtons[i].GetComponent<CircularButton>();

                if (Button.gameObject != SelectedButton)
                    Button.Icon.color = Color.Lerp(Button.Icon.color, ButtonNormalColor, 15f * Time.deltaTime);
                else
                    Button.Icon.color = Color.Lerp(Button.Icon.color, ButtonHoverColor, 15f * Time.deltaTime);
            }

            SelectionIcon.sprite = SelectedButton.GetComponent<CircularButton>().Icon.sprite;
            SelectionIcon.preserveAspect = SelectionIconPreserveAspect;
            SelectionText.text = SelectedButton.GetComponent<CircularButton>().Text;
            SelectionDescription.text = SelectedButton.GetComponent<CircularButton>().Description;

#if EBS_NEW_INPUT_SYSTEM
            if (BuilderInput.Instance.userInteraface.Validate.triggered)
            {
                if (SelectedButton.GetComponent<CircularButton>().GetComponent<Animator>() != null)
                    SelectedButton.GetComponent<CircularButton>().GetComponent<Animator>().Play("Button Press");

                SelectedButton.GetComponent<CircularButton>().Action.Invoke();
            }
#else
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                if (SelectedButton.GetComponent<CircularButton>().GetComponent<Animator>() != null)
                    SelectedButton.GetComponent<CircularButton>().GetComponent<Animator>().Play("Button Press");

                SelectedButton.GetComponent<CircularButton>().Action.Invoke();
            }
#endif
        }

        private void RefreshButtons()
        {
            Elements = CurrentCategory.InstancedButtons.Count;

            if (Elements > 0)
            {
                Fill = 1f / (float)Elements;

                float FillRadius = Fill * 360f;
                float LastRotation = 0;

                for (int i = 0; i < Elements; i++)
                {
                    GameObject Temp = CurrentCategory.InstancedButtons[i].gameObject;

                    float Rotate = LastRotation + FillRadius / 2;
                    LastRotation = Rotate + FillRadius / 2;

                    Temp.transform.localPosition = new Vector2(ButtonSpacing * Mathf.Cos((Rotate - 90) * Mathf.Deg2Rad), -ButtonSpacing * Mathf.Sin((Rotate - 90) * Mathf.Deg2Rad));
                    Temp.transform.localScale = Vector3.one;

                    if (Rotate > 360)
                        Rotate -= 360;

                    Temp.name = Rotate.ToString();

                    ButtonsRotation.Add(Rotate);
                }
            }
        }

        /// <summary>
        /// This method allows to change of category by name.
        /// </summary>
        public void ChangeCategory(string name)
        {
            DefaultCategoryIndex = Categories.ToList().FindIndex(entry => entry.Content.name == name);

            if (DefaultCategoryIndex == -1)
                return;

            CurrentCategory = Categories[DefaultCategoryIndex];

            for (int i = 0; i < Categories.Count; i++)
            {
                if (Categories[i].Content != null)
                {
                    if (i != DefaultCategoryIndex)
                        Categories[i].Content.SetActive(false);
                    else
                        Categories[i].Content.SetActive(true);
                }
            }

            RefreshButtons();
        }

        /// <summary>
        /// This method allows to change mode.
        /// </summary>
        public void ChangeMode(string modeName)
        {
            Hide();

            BuilderBehaviour.Instance.ChangeMode(modeName);
        }

        /// <summary>
        /// This method allows to pass in placement mode with a piece name.
        /// </summary>
        public void ChangePiece(string name)
        {
            Hide();

            BuilderBehaviour.Instance.ChangeMode(BuildMode.None);

            BuilderBehaviour.Instance.SelectPrefab(BuildManager.Instance.GetPieceByName(name));
            BuilderBehaviour.Instance.ChangeMode(BuildMode.Placement);
        }

        /// <summary>
        /// This method allows to show the circular menu.
        /// </summary>
        protected void Show()
        {
            Animator.CrossFade(ShowStateName, 0.1f);

            for (int i = 0; i < DisableComponentsWhenShown.Length; i++)
                DisableComponentsWhenShown[i].enabled = false;

            if (Controller != ControllerType.XR)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            IsActive = true;
        }

        /// <summary>
        /// This method allows to close the circular menu.
        /// </summary>
        protected void Hide()
        {
            Animator.CrossFade(HideStateName, 0.1f);

            for (int i = 0; i < DisableComponentsWhenShown.Length; i++)
                DisableComponentsWhenShown[i].enabled = true;

            if (Controller != ControllerType.XR)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            IsActive = false;
        }

#endregion
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(CircularMenu))]
    public class CircularMenuInspector : Editor
    {
        #region Fields

        private CircularMenu Target { get { return (CircularMenu)target; } }
        private static bool[] FoldoutArray = new bool[4];

        private string CategoryName;

        #endregion Fields

        #region Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InspectorStyles.DrawSectionLabel("Circular Menu - Add-On");

            GUILayout.Label("UI circular menu allows the selection of modes and pieces via UI.", EditorStyles.miniLabel);

            #region General

            FoldoutArray[0] = EditorGUILayout.Foldout(FoldoutArray[0], "General Settings", true);

            if (FoldoutArray[0])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Controller"), 
                    new GUIContent("Input Controller Type :", "Input type to control the circular menu."));

                GUILayout.BeginHorizontal();
                GUILayout.Space(13);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DisableComponentsWhenShown"),
                    new GUIContent("Disable Components When Shown :", "Disable defined components when the circular menu is shown, useful to disable some awkward stuff."), true);
                GUILayout.EndHorizontal();
            }

            #endregion

            #region UI

            FoldoutArray[1] = EditorGUILayout.Foldout(FoldoutArray[1], "UI References Settings", true);

            if (FoldoutArray[1])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SelectionText"), new GUIContent("Selection Name :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SelectionDescription"), new GUIContent("Selection Description :", ""));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("Selection"), new GUIContent("Selection Bar :", ""));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("SelectionIcon"), new GUIContent("Selection Icon :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("SelectionIconPreserveAspect"), new GUIContent("Selection Icon Preserve Aspect :", ""));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonNormalColor"), new GUIContent("Selection Icon Normal Color :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonHoverColor"), new GUIContent("Selection Icon Hover Color :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonPressedColor"), new GUIContent("Selection Icon Pressed Color :", ""));

                EditorGUILayout.PropertyField(serializedObject.FindProperty("CircularButton"), new GUIContent("Button Prefab :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonImagePreserveAspect"), new GUIContent("Button Image Preserve Aspect :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ButtonSpacing"), new GUIContent("Button Spacing :", ""));
            }

            #endregion

            #region Categories

            FoldoutArray[2] = EditorGUILayout.Foldout(FoldoutArray[2], "Categories Settings", true);

            if (FoldoutArray[2])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("DefaultCategoryIndex"), new GUIContent("Default Category Index :", ""));

                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                for (int i = 0; i < Target.Categories.Count; i++)
                {
                    GUI.color = Color.black / 4f;
                    GUILayout.BeginVertical("helpBox");
                    GUI.color = Color.white;

                    Target.Categories[i].Name = EditorGUILayout.TextField("Category Name :", Target.Categories[i].Name);
                    Target.Categories[i].Content = (GameObject)EditorGUILayout.ObjectField("Category Content :", Target.Categories[i].Content, typeof(GameObject), true);

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(13);
                    SerializedProperty ButtonsProperty = serializedObject.FindProperty("Categories.Array.data[" + i + "].Buttons");

                    EditorGUILayout.PropertyField(ButtonsProperty, true);
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Convert All Pieces To Buttons..."))
                    {
                        if (BuildManager.Instance != null)
                        {
                            if (BuildManager.Instance.Pieces != null)
                            {
                                for (int x = 0; x < BuildManager.Instance.Pieces.Count; x++)
                                {
                                    Target.Categories[i].Buttons.Add(new CircularButtonData()
                                    {
                                        Icon = BuildManager.Instance.Pieces[x].Icon,
                                        Order = x,
                                        Text = BuildManager.Instance.Pieces[x].Name,
                                        Description = BuildManager.Instance.Pieces[x].Description,
                                        Action = new UnityEvent()
                                    });
                                }

                                EditorUtility.SetDirty(target);
                            }
                        }
                    }

                    if (GUILayout.Button("Remove Category"))
                    {
                        if (Target.transform.Find(Target.Categories[i].Name) != null)
                            DestroyImmediate(Target.transform.Find(Target.Categories[i].Name).gameObject);

                        Target.Categories.RemoveAt(i);
                    }

                    GUILayout.EndVertical();
                }

                GUILayout.EndVertical();

                GUI.color = Color.black / 4f;
                GUILayout.BeginVertical("helpBox");
                GUI.color = Color.white;

                CategoryName = EditorGUILayout.TextField("Category Name :", CategoryName);

                if (GUILayout.Button("Add New Category"))
                {
                    GameObject NewContent = new GameObject(CategoryName);
                    NewContent.transform.SetParent(Target.transform, false);
                    NewContent.transform.localPosition = Vector3.zero;
                    Target.Categories.Add(new CircularMenu.UICustomCategory() { Name = CategoryName, Content = NewContent });
                    CategoryName = string.Empty;
                }

                GUILayout.EndVertical();
            }

            #endregion

            #region Animator

            FoldoutArray[3] = EditorGUILayout.Foldout(FoldoutArray[3], "Animator Settings", true);

            if (FoldoutArray[3])
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Animator"), new GUIContent("Circular Animator :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ShowStateName"), new GUIContent("Circular Show State Name :", ""));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HideStateName"), new GUIContent("Circular Hide State Name :", ""));
            }

            #endregion

            serializedObject.ApplyModifiedProperties();
        }

        #endregion Methods
    }

#endif
}