using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
namespace UnityEngine.UI
{
    /// <summary>
	/// Dissolve effect for uGUI.
	/// </summary>
	[AddComponentMenu("UI/UIEffect/LUIDissolve", 3)]
    public class LUIDissolve : LUIEffectBase
    {
        //################################
        // Constant or Static Members.
        //################################
        public const string shaderName = "UI/Hidden/UI-Effect-Dissolve";
        static readonly ParameterTexture _ptex = new ParameterTexture(8, 128, "_ParamTex");


        //################################
        // Serialize Members.
        //################################
        [Tooltip("Current location[0-1] for dissolve effect. 0 is not dissolved, 1 is completely dissolved.")]
        [FormerlySerializedAs("m_Location")]
        [SerializeField] [Range(0, 1)] float m_EffectFactor = 0.5f;

        [Tooltip("Edge width.")]
        [SerializeField] [Range(0, 1)] float m_Width = 0.5f;

        [Tooltip("Edge softness.")]
        [SerializeField] [Range(0, 1)] float m_Softness = 0.5f;

        [Tooltip("Edge color.")]
        [SerializeField] [ColorUsage(false)] Color m_Color = new Color(0.0f, 0.25f, 1.0f);

        [Tooltip("Edge color effect mode.")]
        [SerializeField] ColorMode m_ColorMode = ColorMode.Add;

        [Tooltip("Noise texture for dissolving (single channel texture).")]

        [SerializeField] Texture m_NoiseTexture;

        [Header("Advanced Option")]
        [Tooltip("The area for effect.")]
        [SerializeField] protected EffectArea m_EffectArea;

        [Tooltip("Keep effect aspect ratio.")]
        [SerializeField] bool m_KeepAspectRatio;


        //################################
        // Public Members.
        //################################

        /// <summary>
        /// Effect factor between 0(start) and 1(end).
        /// </summary>
        [System.Obsolete("Use effectFactor instead (UnityUpgradable) -> effectFactor")]
        public float location
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
        /// Edge width.
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
        /// Edge softness.
        /// </summary>
        public float softness
        {
            get { return m_Softness; }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (!Mathf.Approximately(m_Softness, value))
                {
                    m_Softness = value;
                    SetDirty();
                }
            }
        }

        /// <summary>
        /// Edge color.
        /// </summary>
        public Color color
        {
            get { return m_Color; }
            set
            {
                if (m_Color != value)
                {
                    m_Color = value;
                    SetDirty();
                }
            }
        }
#if UNITY_EDITOR
        /// <summary>
        /// Noise texture.
        /// </summary>
        public Texture noiseTexture
        {
            get { return m_NoiseTexture; }
            set
            {
                if (m_NoiseTexture != value)
                {
                    m_NoiseTexture = value;
                    if (graphic)
                    {
                        ModifyMaterial();
                    }
                }
            }
        }
#endif
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
        /// Keep aspect ratio.
        /// </summary>
        public bool keepAspectRatio
        {
            get { return m_KeepAspectRatio; }
            set
            {
                if (m_KeepAspectRatio != value)
                {
                    m_KeepAspectRatio = value;
                    SetVerticesDirty();
                }
            }
        }

        /// <summary>
        /// Color effect mode.
        /// </summary>
        public ColorMode colorMode { get { return m_ColorMode; } }

        /// <summary>
        /// graphic material.
        /// </summary>
        public virtual Material material { get { return graphic.material; } set { graphic.material = value; } }

        /// <summary>
        /// Gets the parameter texture.
        /// </summary>
        public override ParameterTexture ptex { get { return _ptex; } }

        //################################
        // Private Members.
        //################################
        MaterialCache _materialCache = null;

        /// <summary>
		/// Modifies the material.
		/// </summary>
		public override void ModifyMaterial()
        {
            bool isTpImage = false;
            Texture _AlphaTex = null;
            if (graphic is LImageForTP)
            {
                var image = graphic as LImageForTP;
                isTpImage = true;
                _AlphaTex = AtlasManager.Instance.GetAtlasInfoBySpriteName(image.m_SpriteName).m_Mat.GetTexture("_AlphaTex");
            }
            string key = "";
            if (isTpImage)
            {
                string noiseKey = (m_NoiseTexture ? m_NoiseTexture.GetInstanceID().ToString() : "null");
                string spriteKey = _AlphaTex.GetInstanceID().ToString();
                key = string.Format("{0}{1}{2}", noiseKey, spriteKey, m_ColorMode);
            }
            else
            {
                key = (m_NoiseTexture ? m_NoiseTexture.GetInstanceID().ToString() : "null");
            }
            if (_materialCache != null && (_materialCache.key != key || !isActiveAndEnabled || !m_EffectMaterial))
            {
                MaterialCache.Unregister(_materialCache);
                _materialCache = null;
            }

            if (!isActiveAndEnabled || !m_EffectMaterial)
            {
                material = null;
            }
            else if (!m_NoiseTexture)
            {
                material = m_EffectMaterial;
            }
            else if (_materialCache != null && _materialCache.key == key)
            {
                material = _materialCache.material;
            }
            else
            {
                _materialCache = MaterialCache.Register(key, () =>
                    {
                        var mat = new Material(m_EffectMaterial);
                        mat.name += "_" + m_NoiseTexture.name;
                        if (isTpImage)
                        {
                            mat.SetTexture("_AlphaTex", _AlphaTex);
                        }
                        mat.SetTexture("_NoiseTex", m_NoiseTexture);
                        return mat;
                    });
                material = _materialCache.material;
            }
        }

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
            var tex = m_NoiseTexture;
            var aspectRatio = m_KeepAspectRatio && tex ? ((float)tex.width) / tex.height : -1;
            Rect rect = m_EffectArea.GetEffectArea(vh, ((RectTransform)transform).rect, aspectRatio);

            // Calculate vertex position.
            UIVertex vertex = default(UIVertex);
            float x, y;
            int count = vh.currentVertCount;
            for (int i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                m_EffectArea.GetPositionFactor(i, rect, vertex.position, isText, false, out x, out y);

                vertex.uv0 = new Vector2(
                    Packer.ToFloat(vertex.uv0.x, vertex.uv0.y),
                    Packer.ToFloat(x, y, normalizedIndex)
                );

                vh.SetUIVertex(vertex, i);
            }
        }

        protected override void SetDirty()
        {

            ptex.RegisterMaterial(material);

            ptex.SetData(this, 0, m_EffectFactor);  // param1.x : location
            ptex.SetData(this, 1, m_Width);     // param1.y : width
            ptex.SetData(this, 2, m_Softness);  // param1.z : softness
            ptex.SetData(this, 4, m_Color.r);   // param2.x : red
            ptex.SetData(this, 5, m_Color.g);   // param2.y : green
            ptex.SetData(this, 6, m_Color.b);   // param2.z : blue
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            MaterialCache.Unregister(_materialCache);
            _materialCache = null;
        }

        // #if UNITY_EDITOR
        // 		/// <summary>
        // 		/// Gets the material.
        // 		/// </summary>
        // 		/// <returns>The material.</returns>
        // 		protected override Material GetMaterial()
        // 		{
        // 			return MaterialUtility.GetOrGenerateMaterialVariant(Shader.Find(shaderName), m_ColorMode);
        // 		}
        // #endif
    }
}