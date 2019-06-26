using UnityEditor;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
	/// UIEffect editor.
	/// </summary>
	[CustomEditor(typeof(LUIDissolve))]
	[CanEditMultipleObjects]
	public class LUIDissolveEditor : Editor
	{
        //################################
		// Public/Protected Members.
		//################################
		/// <summary>
		/// This function is called when the object becomes enabled and active.
		/// </summary>
		protected void OnEnable()
		{
			_spMaterial = serializedObject.FindProperty("m_EffectMaterial");
			_spEffectFactor = serializedObject.FindProperty("m_EffectFactor");
			_spEffectArea = serializedObject.FindProperty("m_EffectArea");
			_spKeepAspectRatio = serializedObject.FindProperty("m_KeepAspectRatio");
			_spWidth = serializedObject.FindProperty("m_Width");
			_spColor = serializedObject.FindProperty("m_Color");
			_spSoftness = serializedObject.FindProperty("m_Softness");
			_spColorMode = serializedObject.FindProperty("m_ColorMode");
			_spNoiseTexture = serializedObject.FindProperty("m_NoiseTexture");
			_spKeepAspectRatio = serializedObject.FindProperty("m_KeepAspectRatio");

			_shader = Shader.Find ("TextMeshPro/Distance Field (UIDissolve)");
			_mobileShader = Shader.Find ("TextMeshPro/Mobile/Distance Field (UIDissolve)");
			_spriteShader = Shader.Find ("TextMeshPro/Sprite (UIDissolve)");
		}

		/// <summary>
		/// Implement this function to make a custom inspector.
		/// </summary>
		public override void OnInspectorGUI()
		{
            bool isNeedBuildMat = false;

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
			EditorGUILayout.PropertyField(_spSoftness);
			EditorGUILayout.PropertyField(_spColor);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField (_spColorMode);
            EditorGUILayout.PropertyField (_spNoiseTexture);
            isNeedBuildMat = EditorGUI.EndChangeCheck();

			//================
			// Advanced option.
			//================
			EditorGUILayout.PropertyField(_spEffectArea);
			EditorGUILayout.PropertyField(_spKeepAspectRatio);

            if ((isNeedBuildMat || _spMaterial.objectReferenceValue == null) &&_spNoiseTexture.objectReferenceValue != null)
            {
                var obj = target as LUIDissolve;
                if (obj.targetGraphic is LImageForTP)
                {
                    Shader shader = Shader.Find("UI/Hidden/UI-Effect-Dissolve(RGB+A)");
                    _spMaterial.objectReferenceValue = MaterialUtility.GetOrGenerateMaterialVariant(shader, (ColorMode)(_spColorMode.intValue));
                    serializedObject.ApplyModifiedProperties();
                }
                else{
                    Shader shader = Shader.Find("UI/Hidden/UI-Effect-Dissolve");
                    _spMaterial.objectReferenceValue = MaterialUtility.GetOrGenerateMaterialVariant(shader, (ColorMode)(_spColorMode.intValue));
                    serializedObject.ApplyModifiedProperties();
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
		SerializedProperty _spColor;
		SerializedProperty _spSoftness;
		SerializedProperty _spColorMode;
		SerializedProperty _spNoiseTexture;
		SerializedProperty _spEffectArea;
		SerializedProperty _spKeepAspectRatio;
		Shader _shader;
		Shader _mobileShader;
		Shader _spriteShader;
    }
}