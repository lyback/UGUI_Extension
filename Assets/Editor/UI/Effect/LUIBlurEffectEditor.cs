using UnityEditor;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    public class LUIBlurEffectEditor : Editor
    {
        //================
        // Blur setting.
        //================
        public static bool DrawBlurProperties(SerializedObject serializedObject)
        {
            bool isBlurModeChanger = false;
            var spBlurMode = serializedObject.FindProperty("m_BlurMode");
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(spBlurMode);
            isBlurModeChanger = EditorGUI.EndChangeCheck();

            // When blur is enable, show parameters.
            if (spBlurMode.intValue != (int)BlurMode.None)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_BlurFactor"));

                var spAdvancedBlur = serializedObject.FindProperty("m_AdvancedBlur");
                if (spAdvancedBlur != null)
                {
                    EditorGUILayout.PropertyField(spAdvancedBlur);
                }
                EditorGUI.indentLevel--;
            }
            return isBlurModeChanger;
        }
    }
}