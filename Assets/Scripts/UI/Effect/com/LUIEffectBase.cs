using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public abstract class LUIEffectBase : BaseMeshEffect, IParameterTexture
    {
        protected static readonly Vector2[] splitedCharacterPosition = { Vector2.up, Vector2.one, Vector2.right, Vector2.zero };
        protected static readonly List<UIVertex> tempVerts = new List<UIVertex>();
        [SerializeField] protected Material m_EffectMaterial;

        /// <summary>
        /// Gets or sets the parameter index.
        /// </summary>
        public int parameterIndex { get; set; }

        /// <summary>
        /// Gets the parameter texture.
        /// </summary>
        public virtual ParameterTexture ptex { get { return null; } }

        /// <summary>
        /// Gets target graphic for effect.
        /// </summary>
        public Graphic targetGraphic { get { return graphic; } }

        /// <summary>
        /// Gets material for effect.
        /// </summary>
        public Material effectMaterial { get { return m_EffectMaterial; } }

        /// <summary>
        /// graphic material.
        /// </summary>
        public virtual Material material { get { return graphic.material; } set { graphic.material = value; } }

        /// <summary>
        /// Modifies the material.
        /// </summary>
        public virtual void ModifyMaterial()
        {
            targetGraphic.material = isActiveAndEnabled ? m_EffectMaterial : null;
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            if (ptex != null)
            {
                ptex.Register(this);
            }
            ModifyMaterial();
            SetVerticesDirty();
            SetDirty();
        }

        /// <summary>
        /// This function is called when the behaviour becomes disabled () or inactive.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();

            ModifyMaterial();
            SetVerticesDirty();
            if (ptex != null)
            {
                ptex.Unregister(this);
            }
        }

        /// <summary>
        /// Mark the UIEffect as dirty.
        /// </summary>
        protected virtual void SetDirty()
        {
            SetVerticesDirty();
        }
        protected virtual void SetVerticesDirty()
        {
            if (graphic)
			{
				graphic.SetVerticesDirty ();
			}
        }
        protected override void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }


#if UNITY_EDITOR
		protected override void Reset()
		{
			OnValidate();
		}
		/// <summary>
		/// Raises the validate event.
		/// </summary>
		protected override void OnValidate()
		{
			base.OnValidate ();

			ModifyMaterial();
			SetVerticesDirty ();
			SetDirty ();
		}
        
#endif
    }
}