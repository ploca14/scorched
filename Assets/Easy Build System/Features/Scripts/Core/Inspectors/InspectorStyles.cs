#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Features.Scripts.Core.Inspectors
{
    public class InspectorStyles
    {
        #region Fields

        public static Color TextColor = new Color(0f, 1f, 1f);

        #endregion

        #region Methods

        public static void DrawSectionLabel(string text)
        {
            GUI.color = TextColor * 1.25f;
            GUILayout.Label(text, EditorStyles.largeLabel);
            GUILayout.Space(-2f);
            GUI.color = Color.white;
        }

        public static bool LinkLabel(GUIContent label, params GUILayoutOption[] options)
        {
            GUIStyle m_LinkStyle;

            m_LinkStyle = new GUIStyle(EditorStyles.largeLabel);
            m_LinkStyle.wordWrap = false;
            m_LinkStyle.normal.textColor = TextColor / 1.25f;
            m_LinkStyle.stretchWidth = false;

            var position = GUILayoutUtility.GetRect(label, m_LinkStyle, options);

            Handles.BeginGUI();
            Handles.color = m_LinkStyle.normal.textColor;
            Handles.DrawLine(new Vector3(position.xMin + 2f, position.yMax), new Vector3(position.xMax - 2f, position.yMax));
            Handles.color = Color.white;
            Handles.EndGUI();

            EditorGUIUtility.AddCursorRect(position, MouseCursor.Link);

            return GUI.Button(position, label, m_LinkStyle);
        }

        #endregion
    }
}
#endif