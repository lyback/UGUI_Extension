using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace UnityEngine.UI
{
    public class LUIEffect_Prop_CapScreen : RawImage
    {
        //################################	
        // Serialize Members.
        //################################
        [Tooltip("FilterMode for capturing.")]
        [SerializeField] FilterMode m_FilterMode = FilterMode.Bilinear;

        [Tooltip("Desampling rate of the generated RenderTexture.")]
        [SerializeField] DesamplingRate m_DesamplingRate = DesamplingRate.x1;

        [Tooltip("Effect material.")]
        [SerializeField] Material m_EffectMaterial = null;

        [Tooltip("Desampling rate of reduction buffer to apply effect.")]
        [SerializeField] DesamplingRate m_ReductionRate = DesamplingRate.x1;

        [Tooltip("Blur effect mode.")]
        [SerializeField] BlurMode m_BlurMode = BlurMode.DetailBlur;

        [Tooltip("Blur iterations.")]
        [SerializeField] [Range(1, 8)] int m_BlurIterations = 3;

        [Tooltip("How far is the blurring from the graphic.")]
        [SerializeField] [Range(0, 1)] float m_BlurFactor = 1;
        //################################
        // Public Members.
        //################################

        /// <summary>
        /// FilterMode for capturing.
        /// </summary>
        public FilterMode filterMode { get { return m_FilterMode; } set { m_FilterMode = value; } }

        /// <summary>
        /// Desampling rate of the generated RenderTexture.
        /// </summary>
        public DesamplingRate desamplingRate { get { return m_DesamplingRate; } set { m_DesamplingRate = value; } }

        /// <summary>
        /// Effect material.
        /// </summary>
        public Material effectMaterial { get { return m_EffectMaterial; } protected set { m_EffectMaterial = value; } }

        /// <summary>
        /// Desampling rate of reduction buffer to apply effect.
        /// </summary>
        public DesamplingRate reductionRate { get { return m_ReductionRate; } set { m_ReductionRate = value; } }

        /// <summary>
        /// Captured texture.
        /// </summary>
        public RenderTexture capturedTexture { get { return _rt; } }

        /// <summary>
        /// Blur effect mode.
        /// </summary>
        public BlurMode blurMode { get { return m_BlurMode; } }

        /// <summary>
        /// Blur iterations.
        /// </summary>
        public int blurIterations { get { return m_BlurIterations; } set { m_BlurIterations = value; } }
        /// <summary>
        /// How far is the blurring from the graphic.
        /// </summary>
        public float blurFactor { get { return m_BlurFactor; } set { m_BlurFactor = Mathf.Clamp(value, 0, 4); } }

        //################################
        // Protected Members.
        //################################
        protected static int s_CopyId;
        protected static int s_EffectId1;
        protected static int s_EffectId2;

        protected static int s_EffectFactorId;
        protected static CommandBuffer s_CommandBuffer;
        protected RenderTexture _rt;
        protected RenderTargetIdentifier _rtId;
    }
}