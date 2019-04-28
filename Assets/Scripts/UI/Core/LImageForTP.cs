using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace UnityEngine.UI
{
    public class LImageForTP : LImage
    {
        [SerializeField]
        public bool m_UseTPAtlas = true;
        [SerializeField]
        public string m_SpriteName = "";

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            var overrideSprite = this.overrideSprite;
            if (overrideSprite != null)
            {
                m_UsePolyMesh = AtlasManager.Instance.GetIsPolyAtlas(overrideSprite.name);
            }
            base.OnPopulateMesh(toFill);
        }

        protected override Vector4 GetPadding(Sprite spr)
        {
            #region LImageForTP
            if (m_UseTPAtlas)
            {
                return AtlasManager.Instance.GetPadding(spr.name);
            }
            #endregion
            return base.GetPadding(spr);
        }
        protected override Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            var activeSprite = this.overrideSprite;
            var padding = activeSprite == null ? Vector4.zero : GetPadding(activeSprite);
            //需要加上padding才是原图大小
            var size = activeSprite == null ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            Rect r = GetPixelAdjustedRect();
            // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

            #region LImageForTP
            if (m_UseTPAtlas)
            {
                size.x += padding.x + padding.z;
                size.y += padding.y + padding.w;
            }
            #endregion

            int spriteW = Mathf.RoundToInt(size.x);
            int spriteH = Mathf.RoundToInt(size.y);

            var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                var spriteRatio = size.x / size.y;
                var rectRatio = r.width / r.height;

                if (spriteRatio > rectRatio)
                {
                    var oldHeight = r.height;
                    r.height = r.width * (1.0f / spriteRatio);
                    r.y += (oldHeight - r.height) * rectTransform.pivot.y;
                }
                else
                {
                    var oldWidth = r.width;
                    r.width = r.height * spriteRatio;
                    r.x += (oldWidth - r.width) * rectTransform.pivot.x;
                }
            }

            v = new Vector4(
                r.x + r.width * v.x,
                r.y + r.height * v.y,
                r.x + r.width * v.z,
                r.y + r.height * v.w
            );

            return v;
        }
        public override void SetNativeSize()
        {
            var overrideSprite = this.overrideSprite;
            if (overrideSprite != null)
            {
                float w;
                float h;
                #region LImageForTP
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
                #endregion
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(w, h);
                SetAllDirty();
            }
        }
    }
}