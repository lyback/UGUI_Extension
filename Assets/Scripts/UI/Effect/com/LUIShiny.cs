using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.UI
{
    public class LUIShiny : LUIEffectBase
    {
        //################################
        // Constant or Static Members.
        //################################
        public const string shaderName = "UI/Hidden/UI-Effect-Shiny";
        static readonly ParameterTexture _ptex = new ParameterTexture(8, 128, "_ParamTex");

        //################################
        // Serialize Members.
        //################################
        [Tooltip("Location for shiny effect.")]
        [SerializeField] [Range(0, 1)] float m_EffectFactor = 0;

        [Tooltip("Width for shiny effect.")]
        [SerializeField] [Range(0, 1)] float m_Width = 0.25f;

        [Tooltip("Rotation for shiny effect.")]
        [SerializeField] [Range(-180, 180)] float m_Rotation;

        [Tooltip("Softness for shiny effect.")]
        [SerializeField] [Range(0.01f, 1)] float m_Softness = 1f;

        [Tooltip("Brightness for shiny effect.")]
        [SerializeField] [Range(0, 1)] float m_Brightness = 1f;

        [Tooltip("Gloss factor for shiny effect.")]
        [SerializeField] [Range(0, 1)] float m_Gloss = 1;

        [Header("Advanced Option")]
        [Tooltip("The area for effect.")]
        [SerializeField] protected EffectArea m_EffectArea;


        //################################
        // Public Members.
        //################################
        /// <summary>
        /// Effect factor between 0(start) and 1(end).
        /// </summary>
        public float effectFactor
        {
            get { return m_EffectFactor; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_EffectFactor, value))
                {
                    m_EffectFactor = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Width for shiny effect.
        /// </summary>
        public float width
        {
            get { return m_Width; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_Width, value))
                {
                    m_Width = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Softness for shiny effect.
        /// </summary>
        public float softness
        {
            get { return m_Softness; }
            set
            {
                value = Mathf.Clamp(value, 0.01f, 1);
                if (!Mathf.Approximately(m_Softness, value))
                {
                    m_Softness = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Brightness for shiny effect.
        /// </summary>
        public float brightness
        {
            get { return m_Brightness; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_Brightness, value))
                {
                    m_Brightness = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Gloss factor for shiny effect.
        /// </summary>
        public float gloss
        {
            get { return m_Gloss; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_Gloss, value))
                {
                    m_Gloss = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Rotation for shiny effect.
        /// </summary>
        public float rotation
        {
            get { return m_Rotation; }
            set
            {
                if (!Mathf.Approximately(m_Rotation, value))
                {
                    m_Rotation = _lastRotation = value;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// The area for effect.
        /// </summary>
        public EffectArea effectArea
        {
            get { return m_EffectArea; }
            set
            {
                if (m_EffectArea != value)
                {
                    m_EffectArea = value;
                    SetVerticesDirty();
                }
            }
        }
        /// <summary>
		/// Gets the parameter texture.
		/// </summary>
		public override ParameterTexture ptex { get { return _ptex; } }

        //################################
        // Private Members.
        //################################
        float _lastRotation;
        MaterialCache _materialCache = null;

        /// <summary>
        /// Modifies the mesh.
        /// </summary>
        public override void ModifyMesh(VertexHelper vh)
        {
            if (!isActiveAndEnabled)
                return;

            bool isText = graphic is Text;
            float normalizedIndex = ptex.GetNormalizedIndex(this);

            // rect.
            Rect rect = m_EffectArea.GetEffectArea(vh, ((RectTransform)transform).rect);

            // rotation.
            float rad = m_Rotation * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            dir.x *= rect.height / rect.width;
            dir = dir.normalized;

            // Calculate vertex position.
            UIVertex vertex = default(UIVertex);
            Vector2 nomalizedPos;
            Matrix2x3 localMatrix = new Matrix2x3(rect, dir.x, dir.y);  // Get local matrix.
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                m_EffectArea.GetNormalizedFactor(i, localMatrix, vertex.position, isText, out nomalizedPos);

                vertex.uv0 = new Vector2(
                    Packer.ToFloat(vertex.uv0.x, vertex.uv0.y),
                    Packer.ToFloat(nomalizedPos.y, normalizedIndex)
                );

                vh.SetUIVertex(vertex, i);
            }
        }

        /// <summary>
        /// Modifies the material.
        /// </summary>
        public override void ModifyMaterial()
        {
            if (graphic is LImageForTP)
            {
                Texture _AlphaTex = null;
                var image = graphic as LImageForTP;
                _AlphaTex = AtlasManager.Instance.GetAtlasInfoBySpriteName(image.m_SpriteName).m_Mat.GetTexture("_AlphaTex");
                string key = _AlphaTex.GetInstanceID().ToString();
                //清理旧的matCache
                if (_materialCache != null && (_materialCache.material == null || _materialCache.key != key || !isActiveAndEnabled || !m_EffectMaterial))
                {
                    MaterialCache.Unregister(_materialCache);
                    _materialCache = null;
                }
                //创建matCache
                if (_materialCache != null && _materialCache.key == key && _materialCache.material != null)
                {
                    material = _materialCache.material;
                }
                else
                {
                    _materialCache = MaterialCache.Register(key, () =>
                        {
                            var mat = new Material(m_EffectMaterial);
                            mat.name += "_" + _AlphaTex.name;
                            mat.SetTexture("_AlphaTex", _AlphaTex);
                            return mat;
                        });
                    material = _materialCache.material;
                }
            }
            else
            {
                //清理旧的matCache
                if (_materialCache != null)
                {
                    MaterialCache.Unregister(_materialCache);
                    _materialCache = null;
                }
                material = m_EffectMaterial;
            }

            if (!isActiveAndEnabled || !m_EffectMaterial)
            {
                material = null;
            }
        }

        protected override void SetDirty()
        {
            ptex.RegisterMaterial(material);

            ptex.SetData(this, 0, m_EffectFactor);  // param1.x : location
            ptex.SetData(this, 1, m_Width);     // param1.y : width
            ptex.SetData(this, 2, m_Softness);  // param1.z : softness
            ptex.SetData(this, 3, m_Brightness);// param1.w : blightness
            ptex.SetData(this, 4, m_Gloss);     // param2.x : gloss

            if (!Mathf.Approximately(_lastRotation, m_Rotation) && targetGraphic)
            {
                _lastRotation = m_Rotation;
                SetVerticesDirty();
            }
        }
    }
}