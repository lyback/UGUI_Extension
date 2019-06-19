using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using System;

public class LBlurEffectEditor : Editor
{
    //================
    // Blur setting.
    //================
    public static void DrawBlurProperties(SerializedObject serializedObject)
    {

        var spBlurMode = serializedObject.FindProperty("m_BlurMode");
        EditorGUILayout.PropertyField(spBlurMode);

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
    }
}