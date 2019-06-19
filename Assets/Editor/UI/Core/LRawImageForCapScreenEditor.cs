using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

/// <summary>
/// UIEffectCapturedImage editor.
/// </summary>
[CustomEditor(typeof(LRawImageForCapScreen))]
[CanEditMultipleObjects]
public class LRawImageForCapScreenEditor : RawImageEditor
{
    //################################
    // Constant or Static Members.
    //################################
    public enum QualityMode : int
    {
        Fast = (DesamplingRate.x2 << 0) + (DesamplingRate.x2 << 4) + (FilterMode.Bilinear << 8) + (2 << 10),
        Medium = (DesamplingRate.x1 << 0) + (DesamplingRate.x1 << 4) + (FilterMode.Bilinear << 8) + (3 << 10),
        Detail = (DesamplingRate.None << 0) + (DesamplingRate.x1 << 4) + (FilterMode.Bilinear << 8) + (5 << 10),
        Custom = -1,
    }
    //################################
    // Private Members.
    //################################
    const int Bits4 = (1 << 4) - 1;
    const int Bits2 = (1 << 2) - 1;
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
        LBlurEffectEditor.DrawBlurProperties(serializedObject);

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
