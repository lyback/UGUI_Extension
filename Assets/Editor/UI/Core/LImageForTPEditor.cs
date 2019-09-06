using System.Linq;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(LImageForTP), true)]
    [CanEditMultipleObjects]
    public class LImageForTPEditor : LImageEditor
    {
        protected SerializedProperty m_UseTPAtlas;
        protected SerializedProperty m_SpriteName;
        GUIContent m_UseTPAtlasContent;
        GUIContent m_SpriteNameContent;
        protected AnimBool m_ShowTPAtlasProperty;
        protected override void OnEnable()
        {
            base.OnEnable();
            #region LimageForTP
            m_UseTPAtlas = serializedObject.FindProperty("m_UseTPAtlas");
            m_SpriteName = serializedObject.FindProperty("m_SpriteName");
            m_UseTPAtlasContent = new GUIContent("Is Use TP");
            m_SpriteNameContent = new GUIContent("Sprite Name");
            m_ShowTPAtlasProperty = new AnimBool(m_Sprite.objectReferenceValue != null && m_UseTPAtlas.boolValue);
            m_ShowTPAtlasProperty.valueChanged.AddListener(Repaint);
            #endregion
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            #region LimageForTP
            m_ShowTPAtlasProperty.valueChanged.RemoveListener(Repaint);
            #endregion
        }
        public override void OnInspectorGUI()
        {
            
            #region LimageForTP
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_UseTPAtlas, m_UseTPAtlasContent);
            m_ShowTPAtlasProperty.target = m_UseTPAtlas.boolValue;
            if (EditorGUILayout.BeginFadeGroup(m_ShowTPAtlasProperty.faded))
                EditorGUILayout.PropertyField(m_SpriteName, m_SpriteNameContent);
            EditorGUILayout.EndFadeGroup();
            #endregion
            
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

            #region LimageForTP
            SetTPMat();
            #endregion

            NativeSizeButtonGUI();
            serializedObject.ApplyModifiedProperties();
        }

        private void SetTPMat()
        {
            LImageForTP image = target as LImageForTP;
            if (!m_UseTPAtlas.boolValue)
            {
                if (m_UseTPAtlas.boolValue != image.m_UseTPAtlas)
                {
                    m_Material.objectReferenceValue = null;
                    m_Sprite.objectReferenceValue = null;
                }
            }
            else
            {
                if (m_UseTPAtlas.boolValue != image.m_UseTPAtlas)
                {
                    string sprName = m_Sprite.objectReferenceValue ? m_Sprite.objectReferenceValue.name : "";
                    Debug.Log("m_UseTPAtlas change:"+sprName);
                    ResetSpriteByName(sprName);
                }
                else
                {
                    if (m_Sprite.objectReferenceValue != image.sprite)
                    {
                        image.m_SpriteName = m_Sprite.objectReferenceValue ? m_Sprite.objectReferenceValue.name : "";
                        m_SpriteName.stringValue = image.m_SpriteName;
                        ResetSpriteByName(image.m_SpriteName);
                    }

                    if (!string.IsNullOrEmpty(image.m_SpriteName) && !image.m_SpriteName.Equals(m_SpriteName.stringValue))
                    {
                        ResetSpriteByName(m_SpriteName.stringValue);
                    }
                    
                    if (m_Material.objectReferenceValue == null)
                    {
                        string sprName = m_SpriteName.stringValue;
                        ResetSpriteByName(sprName);
                    }
                }
            }
        }
        private void ResetSpriteByName(string sprName){
            Sprite spr;
            Material mat;
            AtlasManager.Instance.GetSpriteAndMat(sprName,out spr,out mat);
            m_Sprite.objectReferenceValue = spr;
            var uieffect = ((LImageForTP)target).GetComponent<LUIEffectBase>();
            if (uieffect == null || !uieffect.isActiveAndEnabled)
            {
                m_Material.objectReferenceValue = mat;
            }
            else if (uieffect)
            {
                EditorApplication.delayCall += uieffect.ModifyMaterial;
            }
            m_SpriteName.stringValue = sprName;
        }
    }
}