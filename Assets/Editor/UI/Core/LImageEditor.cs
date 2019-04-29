using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(LImage), true)]
    [CanEditMultipleObjects]
    public class LImageEditor : ImageEditor
    {
        // SerializedProperty m_FillMethod;
        // SerializedProperty m_FillOrigin;
        // SerializedProperty m_FillAmount;
        // SerializedProperty m_FillClockwise;
        protected SerializedProperty m_Type;
        // SerializedProperty m_FillCenter;
        protected SerializedProperty m_Sprite;
        protected SerializedProperty m_PreserveAspect;
        // GUIContent m_SpriteContent;
        // GUIContent m_SpriteTypeContent;
        // GUIContent m_ClockwiseContent;
        // AnimBool m_ShowSlicedOrTiled;
        // AnimBool m_ShowSliced;
        // AnimBool m_ShowTiled;
        // AnimBool m_ShowFilled;
        protected AnimBool m_ShowType_LImage;
        #region Shape属性
        SerializedProperty m_ShapeType;
        GUIContent m_ShapeTypeContent;
        SerializedProperty m_ShapeAnchors;
        SerializedProperty m_ShapeAnchorsOffSet;
        SerializedProperty m_ShapeAnchorsCalPadding;
        SerializedProperty m_ShapeScale;
        SerializedProperty m_CircleShape_Segements;
        SerializedProperty m_CircleShape_FillPercent;
        protected AnimBool m_ShowShapeType;
        AnimBool m_ShowCommonShapeType;
        AnimBool m_ShowSquareShape;
        AnimBool m_ShowCircleShape;
        #endregion
        #region PolyImage属性
        protected SerializedProperty m_UsePolyMesh;
        SerializedProperty m_UsePolyRaycastTarget;
        protected AnimBool m_ShowUsePolyMesh;
        AnimBool m_ShowUsePolyRaycastTarget;
        #endregion
        protected override void OnEnable()
        {
            base.OnEnable();

            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Type = serializedObject.FindProperty("m_Type");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");

            m_ShowType_LImage = new AnimBool(m_Sprite.objectReferenceValue != null);
            m_ShowType_LImage.valueChanged.AddListener(Repaint);

            #region Shape
            m_ShapeTypeContent = new GUIContent("Shape Type");

            m_ShapeType = serializedObject.FindProperty("m_ShapeType");
            m_ShapeAnchors = serializedObject.FindProperty("m_ShapeAnchors");
            m_ShapeAnchorsOffSet = serializedObject.FindProperty("m_ShapeAnchorsOffSet");
            m_ShapeAnchorsCalPadding = serializedObject.FindProperty("m_ShapeAnchorsCalPadding");
            m_ShapeScale = serializedObject.FindProperty("m_ShapeScale");
            m_CircleShape_Segements = serializedObject.FindProperty("m_CircleShape_Segements");
            m_CircleShape_FillPercent = serializedObject.FindProperty("m_CircleShape_FillPercent");

            var shapeTypeEnum = (LImage.ShapeType)m_ShapeType.enumValueIndex;
            m_ShowShapeType = new AnimBool(m_Sprite.objectReferenceValue != null && (Image.Type)m_Type.enumValueIndex == Image.Type.Simple);
            m_ShowShapeType.valueChanged.AddListener(Repaint);
            m_ShowCommonShapeType = new AnimBool(shapeTypeEnum == LImage.ShapeType.Square || shapeTypeEnum == LImage.ShapeType.Circle);
            m_ShowCommonShapeType.valueChanged.AddListener(Repaint);
            m_ShowSquareShape = new AnimBool(shapeTypeEnum == LImage.ShapeType.Square);
            m_ShowSquareShape.valueChanged.AddListener(Repaint);
            m_ShowCircleShape = new AnimBool(shapeTypeEnum == LImage.ShapeType.Circle);
            m_ShowCircleShape.valueChanged.AddListener(Repaint);
            #endregion
            #region PolyImage
            m_UsePolyMesh = serializedObject.FindProperty("m_UsePolyMesh");
            m_UsePolyRaycastTarget = serializedObject.FindProperty("m_UsePolyRaycastTarget");
            m_ShowUsePolyMesh = new AnimBool();
            m_ShowUsePolyMesh.valueChanged.AddListener(Repaint);
            m_ShowUsePolyRaycastTarget = new AnimBool();
            m_ShowUsePolyRaycastTarget.valueChanged.AddListener(Repaint);
            #endregion
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            m_ShowType_LImage.valueChanged.RemoveListener(Repaint);
            #region Shape
            m_ShowShapeType.valueChanged.RemoveListener(Repaint);
            m_ShowCommonShapeType.valueChanged.RemoveListener(Repaint);
            m_ShowSquareShape.valueChanged.RemoveListener(Repaint);
            m_ShowCircleShape.valueChanged.RemoveListener(Repaint);
            #endregion
            #region PolyImage
            m_ShowUsePolyMesh.valueChanged.RemoveListener(Repaint);
            m_ShowUsePolyRaycastTarget.valueChanged.RemoveListener(Repaint);
            #endregion
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SpriteGUI();
            AppearanceControlsGUI();
            RaycastControlsGUI();

            m_ShowType_LImage.target = m_Sprite.objectReferenceValue != null;
            if (EditorGUILayout.BeginFadeGroup(m_ShowType_LImage.faded))
                TypeGUI();
            EditorGUILayout.EndFadeGroup();

            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            #region PolyImage
            m_ShowUsePolyMesh.target = m_Sprite.objectReferenceValue != null && (Image.Type)m_Type.enumValueIndex != Image.Type.Filled && (Image.Type)m_Type.enumValueIndex != Image.Type.Tiled;
            if (EditorGUILayout.BeginFadeGroup(m_ShowUsePolyMesh.faded))
            {
                PolyImageGUI();
            }
            EditorGUILayout.EndFadeGroup();
            #endregion

            #region Shape
            m_ShowShapeType.target = m_Sprite.objectReferenceValue != null && !m_UsePolyMesh.boolValue && (Image.Type)m_Type.enumValueIndex == Image.Type.Simple;
            if (EditorGUILayout.BeginFadeGroup(m_ShowShapeType.faded))
                ShapeTypeGUI();
            EditorGUILayout.EndFadeGroup();
            #endregion

            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }
        protected void SetShowNativeSize(bool instant)
        {
            Image.Type type = (Image.Type)m_Type.enumValueIndex;
            bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) && m_Sprite.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }
        #region Shape
        protected void ShapeTypeGUI()
        {
            EditorGUILayout.PropertyField(m_ShapeType, m_ShapeTypeContent);

            LImage.ShapeType typeEnum = (LImage.ShapeType)m_ShapeType.enumValueIndex;
            // LImage image = target as LImage;

            ++EditorGUI.indentLevel;
            {
                m_ShowCommonShapeType.target = typeEnum == LImage.ShapeType.Square || typeEnum == LImage.ShapeType.Circle;
                m_ShowSquareShape.target = (!m_ShapeType.hasMultipleDifferentValues && typeEnum == LImage.ShapeType.Square);
                m_ShowCircleShape.target = (!m_ShapeType.hasMultipleDifferentValues && typeEnum == LImage.ShapeType.Circle);

                if (EditorGUILayout.BeginFadeGroup(m_ShowCommonShapeType.faded))
                {
                    EditorGUILayout.PropertyField(m_ShapeAnchors, new GUIContent("Anchors"));
                    EditorGUILayout.PropertyField(m_ShapeAnchorsOffSet, new GUIContent("OffSet"));
                    EditorGUILayout.PropertyField(m_ShapeAnchorsCalPadding, new GUIContent("IncludePadding"));
                    if (m_ShapeAnchorsCalPadding.boolValue && ((Sprite)m_Sprite.objectReferenceValue).packed)
                    {
                        EditorGUILayout.HelpBox("计算Padding可能会截取到图集中其他像素，", MessageType.Warning);
                    }
                    EditorGUILayout.PropertyField(m_ShapeScale);
                }
                EditorGUILayout.EndFadeGroup();

                if (EditorGUILayout.BeginFadeGroup(m_ShowCircleShape.faded))
                {
                    EditorGUILayout.PropertyField(m_CircleShape_FillPercent);
                    EditorGUILayout.PropertyField(m_CircleShape_Segements);
                }
                EditorGUILayout.EndFadeGroup();
            }
            --EditorGUI.indentLevel;
        }
        #endregion
        
        #region PolyImage
        protected void PolyImageGUI()
        {
            EditorGUI.indentLevel++;
            {
                EditorGUILayout.PropertyField(m_UsePolyMesh);
                m_ShowUsePolyRaycastTarget.target = m_UsePolyMesh.boolValue;
                if (EditorGUILayout.BeginFadeGroup(m_ShowUsePolyRaycastTarget.faded))
                {
                    EditorGUILayout.PropertyField(m_UsePolyRaycastTarget);
                }
                EditorGUILayout.EndFadeGroup();
            }
            EditorGUI.indentLevel--;
        }
        #endregion
    }
}
