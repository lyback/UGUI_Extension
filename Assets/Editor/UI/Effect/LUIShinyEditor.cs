using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
	/// UIEffect editor.
	/// </summary>
	[CustomEditor(typeof(LUIShiny))]
    [CanEditMultipleObjects]
    public class UIShinyEditor : Editor
    {
        void OnEnable()
        {

            _spMaterial = serializedObject.FindProperty("m_EffectMaterial");
            _spEffectFactor = serializedObject.FindProperty("m_EffectFactor");
            _spEffectArea = serializedObject.FindProperty("m_EffectArea");
            _spWidth = serializedObject.FindProperty("m_Width");
            _spRotation = serializedObject.FindProperty("m_Rotation");
            _spSoftness = serializedObject.FindProperty("m_Softness");
            _spBrightness = serializedObject.FindProperty("m_Brightness");
            _spGloss = serializedObject.FindProperty("m_Gloss");

        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //================
            // Effect material.
            //================
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(_spMaterial);
            EditorGUI.EndDisabledGroup();

            //================
            // Effect setting.
            //================
            EditorGUILayout.PropertyField(_spEffectFactor);
            EditorGUILayout.PropertyField(_spWidth);
            EditorGUILayout.PropertyField(_spRotation);
            EditorGUILayout.PropertyField(_spSoftness);
            EditorGUILayout.PropertyField(_spBrightness);
            EditorGUILayout.PropertyField(_spGloss);

            //================
            // Advanced option.
            //================
            EditorGUILayout.PropertyField(_spEffectArea);

            //================
            // Set Mat.
            //================
            var obj = target as LUIShiny;
            if (_spMaterial.objectReferenceValue == null || _lastGraphic != obj.targetGraphic)
            {
                if (obj.targetGraphic is LImageForTP)
                {
                    Shader shader = Shader.Find("UI/Hidden/UI-Effect-Shiny(RGB+A)");
                    _spMaterial.objectReferenceValue = MaterialUtility.GetOrGenerateMaterialVariant(shader);
                }
                else
                {
                    Shader shader = Shader.Find("UI/Hidden/UI-Effect-Shiny");
                    _spMaterial.objectReferenceValue = MaterialUtility.GetOrGenerateMaterialVariant(shader);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
        //################################
        // Private Members.
        //################################
        SerializedProperty _spMaterial;
        SerializedProperty _spEffectFactor;
        SerializedProperty _spWidth;
        SerializedProperty _spRotation;
        SerializedProperty _spSoftness;
        SerializedProperty _spBrightness;
        SerializedProperty _spGloss;
        SerializedProperty _spEffectArea;
		Graphic _lastGraphic;

    }
}