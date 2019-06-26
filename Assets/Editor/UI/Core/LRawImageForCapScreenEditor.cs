using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    /// <summary>
    /// UIEffectCapturedImage editor.
    /// </summary>
    [CustomEditor(typeof(LRawImageForCapScreen))]
    [CanEditMultipleObjects]
    public class LRawImageForCapScreenEditor : RawImageEditor
    {
        //################################
        // Private Members.
        //################################
        SerializedProperty _spTexture;
        SerializedProperty _spColor;
        SerializedProperty _spRaycastTarget;
        SerializedProperty _spDesamplingRate;
        SerializedProperty _spReductionRate;
        SerializedProperty _spFilterMode;
        SerializedProperty _spBlurMode;
        SerializedProperty _spIterations;

        //################################
        // Public/Protected Members.
        //################################
        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            _spTexture = serializedObject.FindProperty("m_Texture");
            _spColor = serializedObject.FindProperty("m_Color");
            _spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
            _spDesamplingRate = serializedObject.FindProperty("m_DesamplingRate");
            _spReductionRate = serializedObject.FindProperty("m_ReductionRate");
            _spFilterMode = serializedObject.FindProperty("m_FilterMode");
            _spIterations = serializedObject.FindProperty("m_BlurIterations");
            _spBlurMode = serializedObject.FindProperty("m_BlurMode");

        }
        /// <summary>
        /// Implement this function to make a custom inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            bool isNeedBuildMat = false;

            var graphic = (target as LRawImageForCapScreen);
            serializedObject.Update();

            //================
            // Basic properties.
            //================
            EditorGUILayout.PropertyField(_spTexture);
            EditorGUILayout.PropertyField(_spColor);
            EditorGUILayout.PropertyField(_spRaycastTarget);
            //================
            // Effect material.
            //================
            var spMaterial = serializedObject.FindProperty("m_EffectMaterial");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(spMaterial);
            EditorGUI.EndDisabledGroup();

            //================
            // Blur effect.
            //================
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Blur Effect", EditorStyles.boldLabel);
            isNeedBuildMat = LUIBlurEffectEditor.DrawBlurProperties(serializedObject);

            //================
            // Advanced option.
            //================
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Advanced Option", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            if (_spBlurMode.intValue != 0)
            {
                EditorGUILayout.PropertyField(_spIterations);// Iterations.
            }
            DrawDesamplingRate(_spReductionRate);// Reduction rate.

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Result Texture Setting", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_spFilterMode);// Filter Mode.
            DrawDesamplingRate(_spDesamplingRate);// Desampling rate.

            serializedObject.ApplyModifiedProperties();

            // Debug.
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label("Debug");

                if (GUILayout.Button("Capture", "ButtonLeft"))
                {
                    graphic.Release();
                    EditorApplication.delayCall += graphic.Capture;
                }

                EditorGUI.BeginDisabledGroup(!(target as LRawImageForCapScreen).capturedTexture);
                if (GUILayout.Button("Release", "ButtonRight"))
                {
                    graphic.Release();
                }
                EditorGUI.EndDisabledGroup();
            }

            if (isNeedBuildMat || (spMaterial.objectReferenceValue == null && _spBlurMode.intValue != 0))
            {
                Shader shader = Shader.Find("UI/Hidden/UI-EffectCapture-Blur");
                spMaterial.objectReferenceValue = MaterialUtility.GetOrGenerateMaterialVariant(shader, (BlurMode)_spBlurMode.intValue);
                serializedObject.ApplyModifiedProperties();
            }
        }
        /// <summary>
        /// Draws the desampling rate.
        /// </summary>
        void DrawDesamplingRate(SerializedProperty sp)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(sp);
                int w, h;
                (target as LRawImageForCapScreen).GetDesamplingSize((DesamplingRate)sp.intValue, out w, out h);
                GUILayout.Label(string.Format("{0}x{1}", w, h), EditorStyles.miniLabel);
            }
        }
    }
}