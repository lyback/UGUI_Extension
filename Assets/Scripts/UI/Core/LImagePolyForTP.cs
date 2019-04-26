using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class LImagePolyForTP : LImagePoly
    {
        [SerializeField]
        private bool m_UseTPAtlas = true;

        public string m_SpriteName = "";
        protected override Vector4 GetPadding(Sprite spr)
        {
            if (m_UseTPAtlas)
            {
                return AtlasManager.Instance.GetPadding(spr.name);
            }
            else
            {
                return Sprites.DataUtility.GetPadding(spr);
            }
        }
        public override void SetNativeSize()
        {
            var overrideSprite = this.overrideSprite;
            if (overrideSprite != null)
            {
                float w;
                float h;
                if (m_UseTPAtlas)
                {
                    var padding = GetPadding(overrideSprite);
                    w = (overrideSprite.rect.width + padding.x + padding.z) / pixelsPerUnit;
                    h = (overrideSprite.rect.height + padding.y + padding.w) / pixelsPerUnit;
                }
                else
                {
                    w = overrideSprite.rect.width / pixelsPerUnit;
                    h = overrideSprite.rect.height / pixelsPerUnit;
                }
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
                SetAllDirty();
            }
        }
    }
}