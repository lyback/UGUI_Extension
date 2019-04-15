using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(LImagePoly), true)]
    [CanEditMultipleObjects]
    public class LImagePolyEditor : LImageEditor
    {
        #region PolyImage属性
        SerializedProperty m_UsePolyMesh;
        SerializedProperty m_UsePolyRaycastTarget;
        AnimBool m_ShowUsePolyMesh;
        AnimBool m_ShowUsePolyRaycastTarget;
        #endregion
        protected override void OnEnable()
        {
            base.OnEnable();
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
            base.OnEnable();
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