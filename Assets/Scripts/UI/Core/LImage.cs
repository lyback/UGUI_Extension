using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UnityEngine.UI
{
    public class LImage : Image
    {
        #region shape属性
        public enum ShapeType
        {
            None,
            Square,
            Circle,
        }
        public enum ShapeAnchors
        {
            Center,
            Top,
            Bottom,
            Left,
            Right,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
        }
        [SerializeField]
        private ShapeType m_ShapeType = ShapeType.None;
        public ShapeType shapeType { get { return m_ShapeType; } set { if (SetPropertyUtility.SetStruct(ref m_ShapeType, value)) SetVerticesDirty(); } }
        [SerializeField]
        private ShapeAnchors m_ShapeAnchors = ShapeAnchors.Center; // 裁剪锚点
        public ShapeAnchors shapeAnchorsType { get { return m_ShapeAnchors; } set { if (SetPropertyUtility.SetStruct(ref m_ShapeAnchors, value)) SetVerticesDirty(); } }
        [VectorRange(-1f, 1f, -1f, 1f, true), SerializeField]
        private Vector2 m_ShapeAnchorsOffSet = Vector2.zero; // 锚点偏移
        public Vector2 shapeAnchorsOffSet { get { return m_ShapeAnchorsOffSet; } set { if (SetPropertyUtility.SetStruct(ref m_ShapeAnchorsOffSet, value)) SetVerticesDirty(); } }
        [SerializeField]
        private bool m_ShapeAnchorsCalPadding = false; // 裁剪锚点是否计算padding
        [Range(0f, 1f), SerializeField]
        private float m_ShapeScale = 1f; // 裁剪缩放
        public float shapeScale { get { return m_ShapeScale; } set { if (SetPropertyUtility.SetStruct(ref m_ShapeScale, value)) SetVerticesDirty(); } }
        [Range(1f, 50f), SerializeField]
        int m_CircleShape_Segements = 25; // 圆形等分面数
        public int circleShape_Segements { get { return m_CircleShape_Segements; } set { if (SetPropertyUtility.SetStruct(ref m_CircleShape_Segements, value)) SetVerticesDirty(); } }
        [Range(0f, 1f), SerializeField]
        float m_CircleShape_FillPercent = 1f; // 显示比例
        public float circleShape_FillPercent { get { return m_CircleShape_FillPercent; } set { if (SetPropertyUtility.SetStruct(ref m_CircleShape_FillPercent, value)) SetVerticesDirty(); } }
        #endregion
        #region PolyImage属性
        private Vector3[] m_PolyMeshVertices;
        [SerializeField]
        private bool m_UsePolyRaycastTarget = false;
        public bool usePolyRaycastTarget { get { return m_UsePolyRaycastTarget; } set { if (SetPropertyUtility.SetStruct(ref m_UsePolyRaycastTarget, value)) SetVerticesDirty(); } }
        [SerializeField]
        private bool m_UsePolyMesh = false;
        public bool usePolyMesh { get { return m_UsePolyMesh; } set { if (SetPropertyUtility.SetStruct(ref m_UsePolyMesh, value)) SetVerticesDirty(); } }
        #endregion
        /// Image's dimensions used for drawing. X = left, Y = bottom, Z = right, W = top.
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            var activeSprite = this.overrideSprite;
            var padding = activeSprite == null ? Vector4.zero : Sprites.DataUtility.GetPadding(activeSprite);
            var size = activeSprite == null ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            Rect r = GetPixelAdjustedRect();
            // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));

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
        /// <summary>
        /// Update the UI renderer mesh.
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            m_PolyMeshVertices = null;
            if (overrideSprite == null)
            {
                base.OnPopulateMesh(toFill);
                return;
            }
            switch (type)
            {
                case Type.Simple:
                    if (m_UsePolyMesh)
                    {
                        GenerateSimplePolySprite(toFill, preserveAspect);
                    }
                    else
                    {
                        if (m_ShapeType == ShapeType.Circle)
                            GenerateSimpleCircleSprite(toFill, preserveAspect);
                        else if (m_ShapeType == ShapeType.Square)
                            GenerateSimpleSquareSprite(toFill, preserveAspect);
                        else
                            GenerateSimpleSprite(toFill, preserveAspect);
                    }
                    break;
                case Type.Sliced:
                    if (m_UsePolyMesh)
                        GenerateSlicedPolySprite(toFill);
                    else
                        GenerateSlicedSprite(toFill);
                    break;
                case Type.Tiled:
                    GenerateTiledSprite(toFill);
                    break;
                case Type.Filled:
                    GenerateFilledSprite(toFill, preserveAspect);
                    break;
            }
        }
        #region PolyImage绘制方法
        void GenerateSimplePolySprite(VertexHelper vh, bool lPreserveAspect)
        {
            vh.Clear();
            Sprite activeSprite = this.overrideSprite;
            Vector4 v = GetDrawingDimensions(lPreserveAspect);
            Vector4 uv = Sprites.DataUtility.GetOuterUV(activeSprite);
            Color32 color32 = color;
            Vector2[] uvs = activeSprite.uv;
            float invU = 1 / (uv.z - uv.x);
            float invV = 1 / (uv.w - uv.y);
            if (m_UsePolyRaycastTarget)
                m_PolyMeshVertices = new Vector3[uvs.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                float u2 = invU * (uvs[i].x - uv.x);
                float v2 = invV * (uvs[i].y - uv.y);
                float x = u2 * (v.z - v.x) + v.x;
                float y = v2 * (v.w - v.y) + v.y;
                Vector3 vert = new Vector3(x, y, 0f);
                vh.AddVert(vert, color32, uvs[i]);
                if (m_UsePolyRaycastTarget)
                    m_PolyMeshVertices[i] = vert;
            }

            ushort[] triangles = activeSprite.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                vh.AddTriangle(triangles[i], triangles[i + 1], triangles[i + 2]);
            }
        }
        static List<ushort> s_Triangles = new List<ushort>();
        static List<Vector2> s_Uvs = new List<Vector2>();
        private void GenerateSlicedPolySprite(VertexHelper vh)
        {
            if (!hasBorder)
            {
                GenerateSimplePolySprite(vh, false);
                return;
            }
            s_Triangles.Clear();
            s_Uvs.Clear();
            Sprite activeSprite = this.overrideSprite;
            Color32 color32 = color;
            Vector4 outer, inner, padding, border;
            bool _fillCenter = this.fillCenter;
            if (activeSprite != null)
            {
                outer = Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = Sprites.DataUtility.GetInnerUV(activeSprite);
                padding = Sprites.DataUtility.GetPadding(activeSprite);
                border = activeSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            float _pixelsPerUnit = pixelsPerUnit;
            Vector4 adjustedBorders = GetAdjustedBorders(border / _pixelsPerUnit, rect);
            padding = padding / _pixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            Vector2 outer_xy = new Vector2(outer.x, outer.y);
            Vector2 outer_zw = new Vector2(outer.z, outer.w);
            Vector2 inner_xy = new Vector2(inner.x, inner.y);
            Vector2 inner_zw = new Vector2(inner.z, inner.w);
            s_UVScratch[0] = outer_xy;
            s_UVScratch[1] = inner_xy;
            s_UVScratch[2] = inner_zw;
            s_UVScratch[3] = outer_zw;

            var arrayTriangels = activeSprite.triangles;
            var arrayUvs = activeSprite.uv;
            //给List扩容，减少GC
            int p = 0;
            if (inner.x != 0) p++;
            if (inner.z != 0) p++;
            if (inner.y != 0) p++;
            if (inner.w != 0) p++;
            int triCapacity = arrayTriangels.Length * (int)Mathf.Pow(2f, p);
            s_Triangles.Capacity = triCapacity;
            s_Uvs.Capacity = arrayUvs.Length + triCapacity / 3 * 2;
            s_Triangles.AddRange(arrayTriangels);
            s_Uvs.AddRange(arrayUvs);

            // Debug.Log("before，triangles:"+"Capacity:"+s_Triangles.Capacity + ",Count:"+ s_Triangles.Count);
            // Debug.Log("before，uv:"+"Capacity:"+s_Uvs.Capacity + ",Count:"+ s_Uvs.Count);

            if (inner.x != 0)
                MathUtility.NewLineCut(ref s_Uvs, ref s_Triangles, inner_xy, Mathf.PI / 2);
            if (inner.z != 0)
                MathUtility.NewLineCut(ref s_Uvs, ref s_Triangles, inner_zw, Mathf.PI / 2);
            if (inner.y != 0)
                MathUtility.NewLineCut(ref s_Uvs, ref s_Triangles, inner_xy, 0);
            if (inner.w != 0)
                MathUtility.NewLineCut(ref s_Uvs, ref s_Triangles, inner_zw, 0);

            // if (inner.x != 0)
            //     s_Triangles = MathUtility.LineCut(s_Uvs, s_Triangles, inner_xy, Mathf.PI / 2);
            // if (inner.z != 0)
            //     s_Triangles = MathUtility.LineCut(s_Uvs, s_Triangles, inner_zw, Mathf.PI / 2);
            // if (inner.y != 0)
            //     s_Triangles = MathUtility.LineCut(s_Uvs, s_Triangles, inner_xy, 0);
            // if (inner.w != 0)
            //     s_Triangles = MathUtility.LineCut(s_Uvs, s_Triangles, inner_zw, 0);

            // Debug.Log("after，triangles:"+"Capacity:"+s_Triangles.Capacity + ",Count:"+ s_Triangles.Count);
            // Debug.Log("after，uv:"+"Capacity:"+s_Uvs.Capacity + ",Count:"+ s_Uvs.Count);

            vh.Clear();
            int uvsCount = s_Uvs.Count;
            Vector3 pos = new Vector3();
            if (m_UsePolyRaycastTarget)
                m_PolyMeshVertices = new Vector3[uvsCount];
            for (int i = 0; i < uvsCount; i++)
            {
                Vector2 _uv = s_Uvs[i];
                int x = XSlot(_uv.x);
                int y = YSlot(_uv.y);

                Vector2 s_UVScratch_x = s_UVScratch[x];
                Vector2 s_UVScratch_y = s_UVScratch[y];

                float kX = (_uv.x - s_UVScratch_x.x) / (s_UVScratch[x + 1].x - s_UVScratch_x.x);
                float kY = (_uv.y - s_UVScratch_y.y) / (s_UVScratch[y + 1].y - s_UVScratch_y.y);

                Vector2 s_VertScratch_x = s_VertScratch[x];
                Vector2 s_VertScratch_y = s_VertScratch[y];

                pos.x = kX * (s_VertScratch[x + 1].x - s_VertScratch_x.x) + s_VertScratch_x.x;
                pos.y = kY * (s_VertScratch[y + 1].y - s_VertScratch_y.y) + s_VertScratch_y.y;

                vh.AddVert(pos, color32, _uv);
                if (m_UsePolyRaycastTarget)
                    m_PolyMeshVertices[i] = pos;
            }
            int trianglesCount = s_Triangles.Count;
            for (int i = 0; i < trianglesCount; i += 3)
            {
                Vector2 _uv = s_Uvs[s_Triangles[i + 0]];
                int x = XSlot(_uv.x);
                int y = YSlot(_uv.y);
                if (x == 1 && y == 1 && !_fillCenter) continue;
                vh.AddTriangle(s_Triangles[i + 0], s_Triangles[i + 1], s_Triangles[i + 2]);
            }
        }
        private static int XSlot(float x)
        {
            for (int idx = 0; idx < 3; idx++)
            {
                if (s_UVScratch[idx].x < s_UVScratch[idx + 1].x && x <= s_UVScratch[idx + 1].x)
                    return idx;
            }
            return 2;
        }
        private static int YSlot(float y)
        {
            for (int idx = 0; idx < 3; idx++)
            {
                if (s_UVScratch[idx].y < s_UVScratch[idx + 1].y && y <= s_UVScratch[idx + 1].y)
                    return idx;
            }
            return 2;
        }
        #endregion
        #region shape绘制方法
        /// <summary>
        /// shape锚点设置.
        /// </summary>
        Vector4 SetShapeAnchors(Vector4 uv)
        {
            Sprite activeSprite = this.overrideSprite;
            if (activeSprite == null)
            {
                return uv;
            }
            float tw = activeSprite.texture.width;
            float th = activeSprite.texture.height;
            float s = tw / th;
            float dx = (uv.z - uv.x) * s;
            float dy = (uv.w - uv.y);
            float d = dx - dy;//d>0 宽大于高，d<0 高大于宽
            if (d < 0)
            {
                if (shapeAnchorsType == ShapeAnchors.Top || shapeAnchorsType == ShapeAnchors.TopLeft || shapeAnchorsType == ShapeAnchors.TopRight)
                {
                    uv.y -= d;
                }
                else if (shapeAnchorsType == ShapeAnchors.Center || shapeAnchorsType == ShapeAnchors.Left || shapeAnchorsType == ShapeAnchors.Right)
                {
                    uv.y -= d / 2;
                    uv.w += d / 2;
                }
                else if (shapeAnchorsType == ShapeAnchors.Bottom || shapeAnchorsType == ShapeAnchors.BottomLeft || shapeAnchorsType == ShapeAnchors.BottomRight)
                {
                    uv.w += d;
                }
            }
            else
            {
                if (shapeAnchorsType == ShapeAnchors.Center || shapeAnchorsType == ShapeAnchors.Top || shapeAnchorsType == ShapeAnchors.Bottom)
                {
                    uv.x += d / 2;
                    uv.z -= d / 2;
                }
                else if (shapeAnchorsType == ShapeAnchors.Left || shapeAnchorsType == ShapeAnchors.TopLeft || shapeAnchorsType == ShapeAnchors.BottomLeft)
                {
                    uv.z -= d;
                }
                else if (shapeAnchorsType == ShapeAnchors.Right || shapeAnchorsType == ShapeAnchors.TopRight || shapeAnchorsType == ShapeAnchors.BottomRight)
                {
                    uv.x += d;
                }
            }
            return uv;
        }
        Vector4 SetShapeAnchorsOffset(Vector4 uv)
        {
            var dw = uv.z - uv.x;
            var dh = uv.w - uv.y;
            uv.x += dw * m_ShapeAnchorsOffSet.x;
            uv.z += dw * m_ShapeAnchorsOffSet.x;
            uv.y += dh * m_ShapeAnchorsOffSet.y;
            uv.w += dh * m_ShapeAnchorsOffSet.y;
            return uv;
        }
        Vector4 SetShapeScale(Vector4 uv)
        {
            Sprite activeSprite = this.overrideSprite;
            if (activeSprite == null)
            {
                return uv;
            }
            float scale = 1 - m_ShapeScale;
            float dx = (uv.z - uv.x) * scale;
            float dy = (uv.w - uv.y) * scale;
            if (shapeAnchorsType == ShapeAnchors.Center || shapeAnchorsType == ShapeAnchors.Top || shapeAnchorsType == ShapeAnchors.Bottom)
            {
                uv.x += dx * 0.5f;
                uv.z -= dx * 0.5f;
            }
            if (shapeAnchorsType == ShapeAnchors.Center || shapeAnchorsType == ShapeAnchors.Left || shapeAnchorsType == ShapeAnchors.Right)
            {
                uv.y += dy * 0.5f;
                uv.w -= dy * 0.5f;
            }
            if (shapeAnchorsType == ShapeAnchors.Bottom || shapeAnchorsType == ShapeAnchors.BottomLeft || shapeAnchorsType == ShapeAnchors.BottomRight)
            {
                uv.w -= dy;
            }
            if (shapeAnchorsType == ShapeAnchors.Top || shapeAnchorsType == ShapeAnchors.TopLeft || shapeAnchorsType == ShapeAnchors.TopRight)
            {
                uv.y += dy;
            }
            if (shapeAnchorsType == ShapeAnchors.Left || shapeAnchorsType == ShapeAnchors.TopLeft || shapeAnchorsType == ShapeAnchors.BottomLeft)
            {
                uv.z -= dx;
            }
            if (shapeAnchorsType == ShapeAnchors.Right || shapeAnchorsType == ShapeAnchors.TopRight || shapeAnchorsType == ShapeAnchors.BottomRight)
            {
                uv.x += dx;
            }
            return uv;
        }
        Vector4 IncludePaddingUV(Vector4 uv)
        {
            Sprite activeSprite = this.overrideSprite;
            if (activeSprite == null)
            {
                return uv;
            }
            var old_uv = Sprites.DataUtility.GetOuterUV(activeSprite);
            var padding = Sprites.DataUtility.GetPadding(activeSprite);
            float width = activeSprite.rect.width;
            float height = activeSprite.rect.height;
            float odx = old_uv.z - old_uv.x;
            float ody = old_uv.w - old_uv.y;
            float px = padding.x / width * odx;
            float pz = padding.z / width * odx;
            float pw = padding.w / height * ody;
            float py = padding.y / height * ody;
            if (shapeAnchorsType == ShapeAnchors.Center || shapeAnchorsType == ShapeAnchors.Top || shapeAnchorsType == ShapeAnchors.Bottom)
            {
                uv.x += (pz - px) / 2;
                uv.z += (pz - px) / 2;
            }
            if (shapeAnchorsType == ShapeAnchors.Center || shapeAnchorsType == ShapeAnchors.Left || shapeAnchorsType == ShapeAnchors.Right)
            {
                uv.y += (py - pw) / 2;
                uv.w += (py - pw) / 2;
            }
            if (shapeAnchorsType == ShapeAnchors.Bottom || shapeAnchorsType == ShapeAnchors.BottomLeft || shapeAnchorsType == ShapeAnchors.BottomRight)
            {
                uv.y -= pw;
                uv.w -= pw;
            }
            if (shapeAnchorsType == ShapeAnchors.Top || shapeAnchorsType == ShapeAnchors.TopLeft || shapeAnchorsType == ShapeAnchors.TopRight)
            {
                uv.y += py;
                uv.w += py;
            }
            if (shapeAnchorsType == ShapeAnchors.Left || shapeAnchorsType == ShapeAnchors.TopLeft || shapeAnchorsType == ShapeAnchors.BottomLeft)
            {
                uv.z -= px;
                uv.x -= px;
            }
            if (shapeAnchorsType == ShapeAnchors.Right || shapeAnchorsType == ShapeAnchors.TopRight || shapeAnchorsType == ShapeAnchors.BottomRight)
            {
                uv.z += pz;
                uv.x += pz;
            }
            return uv;
        }
        /// <summary>
        /// 绘制圆形图片
        /// </summary>
        void GenerateSimpleCircleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Sprite activeSprite = this.overrideSprite;
            Color32 color = this.color;
            vh.Clear();
            var uv = (activeSprite != null) ? Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            uv = SetShapeAnchors(uv);
            uv = SetShapeScale(uv);
            uv = SetShapeAnchorsOffset(uv);
            if (m_ShapeAnchorsCalPadding)
            {
                uv = IncludePaddingUV(uv);
            }

            float tw = rectTransform.rect.width;
            float th = rectTransform.rect.height;
            float outerRadius = tw > th ? th / 2f : tw / 2f;
            float uvCenterX = (uv.x + uv.z) * 0.5f;
            float uvCenterY = (uv.y + uv.w) * 0.5f;
            float uvScaleX = (uv.z - uv.x) / tw;
            float uvScaleY = (uv.w - uv.y) / th;
            float degreeDelta = 2 * Mathf.PI / circleShape_Segements;
            int curSegements = (int)(circleShape_Segements * circleShape_FillPercent);

            float curDegree = 0f;
            int verticeCount = curSegements + 1;
            //圆心
            Rect r = GetPixelAdjustedRect();
            Vector3 centerVertice = new Vector3(r.x + r.width / 2f, r.y + r.height / 2f);
            vh.AddVert(centerVertice, color, new Vector2(uvCenterX, uvCenterY));
            Vector3 curVertice = new Vector3();
            for (int i = 1; i < verticeCount; i++)
            {
                float cosA = Mathf.Cos(curDegree);
                float sinA = Mathf.Sin(curDegree);
                float posX = cosA * outerRadius;
                float posY = sinA * outerRadius;
                curVertice.x = posX + centerVertice.x;
                curVertice.y = posY + centerVertice.y;
                vh.AddVert(curVertice, color, new Vector2(uvCenterX + posX * uvScaleX, uvCenterY + posY * uvScaleY));
                curDegree += degreeDelta;
            }
            int triangleCount = curSegements * 3;
            for (int i = 0, vIdx = 1; i < triangleCount - 3; i += 3, vIdx++)
            {
                vh.AddTriangle(vIdx, 0, vIdx + 1);
            }
            if (circleShape_FillPercent == 1)
            {
                //首尾顶点相连
                vh.AddTriangle(verticeCount - 1, 0, 1);
            }
        }
        /// <summary>
        /// 绘制方形图片
        /// </summary>
        void GenerateSimpleSquareSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Sprite activeSprite = this.overrideSprite;
            Color32 color = this.color;
            vh.Clear();
            var uv = (activeSprite != null) ? Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            uv = SetShapeAnchors(uv);
            uv = SetShapeScale(uv);
            uv = SetShapeAnchorsOffset(uv);
            if (m_ShapeAnchorsCalPadding)
            {
                uv = IncludePaddingUV(uv);
            }
            //不计算padding
            Rect r = GetPixelAdjustedRect();
            float v_y = r.y;
            float v_w = r.y;
            float v_x = r.x;
            float v_z = r.x;
            float d = r.height - r.width;
            if (d > 0) // 高大于宽
            {
                float dy = d * 0.5f;
                v_y += dy;
                v_w += r.width + dy;
                v_z += r.width;
            }
            else
            {
                float dx = -d * 0.5f;
                v_x += dx;
                v_z += r.height + dx;
                v_w += r.height;
            }

            vh.AddVert(new Vector3(v_x, v_y), color, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v_x, v_w), color, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v_z, v_w), color, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v_z, v_y), color, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
        #endregion
        #region Image原版绘制图片方法
        /// <summary>
        /// Generate vertices for a simple Image.
        /// </summary>
        void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            Sprite activeSprite = this.overrideSprite;
            Color32 color = this.color;//优化：避免每次都要Color转为Color32
            vh.Clear();

            var uv = (activeSprite != null) ? Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            Vector4 v = GetDrawingDimensions(lPreserveAspect);

            vh.AddVert(new Vector3(v.x, v.y), color, new Vector2(uv.x, uv.y));
            vh.AddVert(new Vector3(v.x, v.w), color, new Vector2(uv.x, uv.w));
            vh.AddVert(new Vector3(v.z, v.w), color, new Vector2(uv.z, uv.w));
            vh.AddVert(new Vector3(v.z, v.y), color, new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        /// <summary>
        /// Generate vertices for a 9-sliced Image.
        /// </summary>
        static readonly Vector2[] s_VertScratch = new Vector2[4];
        static readonly Vector2[] s_UVScratch = new Vector2[4];
        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            if (!hasBorder)
            {
                GenerateSimpleSprite(toFill, false);
                return;
            }
            Color32 color = this.color;//优化：避免每次都要Color转为Color32
            Sprite activeSprite = this.overrideSprite;
            Vector4 outer, inner, padding, border;

            if (activeSprite != null)
            {
                outer = Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = Sprites.DataUtility.GetInnerUV(activeSprite);
                padding = Sprites.DataUtility.GetPadding(activeSprite);
                border = activeSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / pixelsPerUnit, rect);
            padding = padding / pixelsPerUnit;

            s_VertScratch[0] = new Vector2(padding.x, padding.y);
            s_VertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            s_VertScratch[1].x = adjustedBorders.x;
            s_VertScratch[1].y = adjustedBorders.y;

            s_VertScratch[2].x = rect.width - adjustedBorders.z;
            s_VertScratch[2].y = rect.height - adjustedBorders.w;

            for (int i = 0; i < 4; ++i)
            {
                s_VertScratch[i].x += rect.x;
                s_VertScratch[i].y += rect.y;
            }

            s_UVScratch[0] = new Vector2(outer.x, outer.y);
            s_UVScratch[1] = new Vector2(inner.x, inner.y);
            s_UVScratch[2] = new Vector2(inner.z, inner.w);
            s_UVScratch[3] = new Vector2(outer.z, outer.w);

            toFill.Clear();

            for (int x = 0; x < 3; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < 3; ++y)
                {
                    if (!fillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;


                    AddQuad(toFill,
                        new Vector2(s_VertScratch[x].x, s_VertScratch[y].y),
                        new Vector2(s_VertScratch[x2].x, s_VertScratch[y2].y),
                        color,
                        new Vector2(s_UVScratch[x].x, s_UVScratch[y].y),
                        new Vector2(s_UVScratch[x2].x, s_UVScratch[y2].y));
                }
            }
        }

        /// <summary>
        /// Generate vertices for a tiled Image.
        /// </summary>

        void GenerateTiledSprite(VertexHelper toFill)
        {
            Vector4 outer, inner, border;
            Vector2 spriteSize;
            Sprite activeSprite = this.overrideSprite;
            Color32 color = this.color;//优化：避免每次都要Color转为Color32
            if (activeSprite != null)
            {
                outer = Sprites.DataUtility.GetOuterUV(activeSprite);
                inner = Sprites.DataUtility.GetInnerUV(activeSprite);
                border = activeSprite.border;
                spriteSize = activeSprite.rect.size;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                border = Vector4.zero;
                spriteSize = Vector2.one * 100;
            }

            Rect rect = GetPixelAdjustedRect();
            float tileWidth = (spriteSize.x - border.x - border.z) / pixelsPerUnit;
            float tileHeight = (spriteSize.y - border.y - border.w) / pixelsPerUnit;
            border = GetAdjustedBorders(border / pixelsPerUnit, rect);

            var uvMin = new Vector2(inner.x, inner.y);
            var uvMax = new Vector2(inner.z, inner.w);

            // Min to max max range for tiled region in coordinates relative to lower left corner.
            float xMin = border.x;
            float xMax = rect.width - border.z;
            float yMin = border.y;
            float yMax = rect.height - border.w;

            toFill.Clear();
            var clipped = uvMax;

            // if either width is zero we cant tile so just assume it was the full width.
            if (tileWidth <= 0)
                tileWidth = xMax - xMin;

            if (tileHeight <= 0)
                tileHeight = yMax - yMin;

            if (activeSprite != null && (hasBorder || activeSprite.packed || activeSprite.texture.wrapMode != TextureWrapMode.Repeat))
            {
                // Sprite has border, or is not in repeat mode, or cannot be repeated because of packing.
                // We cannot use texture tiling so we will generate a mesh of quads to tile the texture.

                // Evaluate how many vertices we will generate. Limit this number to something sane,
                // especially since meshes can not have more than 65000 vertices.

                long nTilesW = 0;
                long nTilesH = 0;
                if (fillCenter)
                {
                    nTilesW = (long)Math.Ceiling((xMax - xMin) / tileWidth);
                    nTilesH = (long)Math.Ceiling((yMax - yMin) / tileHeight);

                    double nVertices = 0;
                    if (hasBorder)
                    {
                        nVertices = (nTilesW + 2.0) * (nTilesH + 2.0) * 4.0; // 4 vertices per tile
                    }
                    else
                    {
                        nVertices = nTilesW * nTilesH * 4.0; // 4 vertices per tile
                    }

                    if (nVertices > 65000.0)
                    {
                        Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.", this);

                        double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                        double imageRatio;
                        if (hasBorder)
                        {
                            imageRatio = (nTilesW + 2.0) / (nTilesH + 2.0);
                        }
                        else
                        {
                            imageRatio = (double)nTilesW / nTilesH;
                        }

                        double targetTilesW = Math.Sqrt(maxTiles / imageRatio);
                        double targetTilesH = targetTilesW * imageRatio;
                        if (hasBorder)
                        {
                            targetTilesW -= 2;
                            targetTilesH -= 2;
                        }

                        nTilesW = (long)Math.Floor(targetTilesW);
                        nTilesH = (long)Math.Floor(targetTilesH);
                        tileWidth = (xMax - xMin) / nTilesW;
                        tileHeight = (yMax - yMin) / nTilesH;
                    }
                }
                else
                {
                    if (hasBorder)
                    {
                        // Texture on the border is repeated only in one direction.
                        nTilesW = (long)Math.Ceiling((xMax - xMin) / tileWidth);
                        nTilesH = (long)Math.Ceiling((yMax - yMin) / tileHeight);
                        double nVertices = (nTilesH + nTilesW + 2.0 /*corners*/) * 2.0 /*sides*/ * 4.0 /*vertices per tile*/;
                        if (nVertices > 65000.0)
                        {
                            Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, convert the Sprite to an Advanced texture, remove the borders, clear the Packing tag and set the Wrap mode to Repeat.", this);

                            double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                            double imageRatio = (double)nTilesW / nTilesH;
                            double targetTilesW = (maxTiles - 4 /*corners*/) / (2 * (1.0 + imageRatio));
                            double targetTilesH = targetTilesW * imageRatio;

                            nTilesW = (long)Math.Floor(targetTilesW);
                            nTilesH = (long)Math.Floor(targetTilesH);
                            tileWidth = (xMax - xMin) / nTilesW;
                            tileHeight = (yMax - yMin) / nTilesH;
                        }
                    }
                    else
                    {
                        nTilesH = nTilesW = 0;
                    }
                }

                if (fillCenter)
                {
                    // TODO: we could share vertices between quads. If vertex sharing is implemented. update the computation for the number of vertices accordingly.
                    for (long j = 0; j < nTilesH; j++)
                    {
                        float y1 = yMin + j * tileHeight;
                        float y2 = yMin + (j + 1) * tileHeight;
                        if (y2 > yMax)
                        {
                            clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                            y2 = yMax;
                        }
                        clipped.x = uvMax.x;
                        for (long i = 0; i < nTilesW; i++)
                        {
                            float x1 = xMin + i * tileWidth;
                            float x2 = xMin + (i + 1) * tileWidth;
                            if (x2 > xMax)
                            {
                                clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                                x2 = xMax;
                            }
                            AddQuad(toFill, new Vector2(x1, y1) + rect.position, new Vector2(x2, y2) + rect.position, color, uvMin, clipped);
                        }
                    }
                }
                if (hasBorder)
                {
                    clipped = uvMax;
                    // Left and right tiled border
                    for (long j = 0; j < nTilesH; j++)
                    {
                        float y1 = yMin + j * tileHeight;
                        float y2 = yMin + (j + 1) * tileHeight;
                        if (y2 > yMax)
                        {
                            clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                            y2 = yMax;
                        }
                        AddQuad(toFill,
                            new Vector2(0, y1) + rect.position,
                            new Vector2(xMin, y2) + rect.position,
                            color,
                            new Vector2(outer.x, uvMin.y),
                            new Vector2(uvMin.x, clipped.y));
                        AddQuad(toFill,
                            new Vector2(xMax, y1) + rect.position,
                            new Vector2(rect.width, y2) + rect.position,
                            color,
                            new Vector2(uvMax.x, uvMin.y),
                            new Vector2(outer.z, clipped.y));
                    }

                    clipped = uvMax;
                    // Bottom and top tiled border
                    for (long i = 0; i < nTilesW; i++)
                    {
                        float x1 = xMin + i * tileWidth;
                        float x2 = xMin + (i + 1) * tileWidth;
                        if (x2 > xMax)
                        {
                            clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                            x2 = xMax;
                        }
                        AddQuad(toFill,
                            new Vector2(x1, 0) + rect.position,
                            new Vector2(x2, yMin) + rect.position,
                            color,
                            new Vector2(uvMin.x, outer.y),
                            new Vector2(clipped.x, uvMin.y));
                        AddQuad(toFill,
                            new Vector2(x1, yMax) + rect.position,
                            new Vector2(x2, rect.height) + rect.position,
                            color,
                            new Vector2(uvMin.x, uvMax.y),
                            new Vector2(clipped.x, outer.w));
                    }

                    // Corners
                    AddQuad(toFill,
                        new Vector2(0, 0) + rect.position,
                        new Vector2(xMin, yMin) + rect.position,
                        color,
                        new Vector2(outer.x, outer.y),
                        new Vector2(uvMin.x, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(xMax, 0) + rect.position,
                        new Vector2(rect.width, yMin) + rect.position,
                        color,
                        new Vector2(uvMax.x, outer.y),
                        new Vector2(outer.z, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(0, yMax) + rect.position,
                        new Vector2(xMin, rect.height) + rect.position,
                        color,
                        new Vector2(outer.x, uvMax.y),
                        new Vector2(uvMin.x, outer.w));
                    AddQuad(toFill,
                        new Vector2(xMax, yMax) + rect.position,
                        new Vector2(rect.width, rect.height) + rect.position,
                        color,
                        new Vector2(uvMax.x, uvMax.y),
                        new Vector2(outer.z, outer.w));
                }
            }
            else
            {
                // Texture has no border, is in repeat mode and not packed. Use texture tiling.
                Vector2 uvScale = new Vector2((xMax - xMin) / tileWidth, (yMax - yMin) / tileHeight);

                if (fillCenter)
                {
                    AddQuad(toFill, new Vector2(xMin, yMin) + rect.position, new Vector2(xMax, yMax) + rect.position, color, Vector2.Scale(uvMin, uvScale), Vector2.Scale(uvMax, uvScale));
                }
            }
        }

        static void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Color32 color, Vector3[] quadUVs)
        {
            int startIndex = vertexHelper.currentVertCount;

            for (int i = 0; i < 4; ++i)
                vertexHelper.AddVert(quadPositions[i], color, quadUVs[i]);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        static void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0), color, new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = rectTransform.rect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                // The adjusted rect (adjusted for pixel correctness)
                // may be slightly larger than the original rect.
                // Adjust the border to match the adjustedRect to avoid
                // small gaps between borders (case 833201).
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }

        /// <summary>
        /// Generate vertices for a filled Image.
        /// </summary>

        static readonly Vector3[] s_Xy = new Vector3[4];
        static readonly Vector3[] s_Uv = new Vector3[4];
        void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
        {
            toFill.Clear();

            if (fillAmount < 0.001f)
                return;

            Color32 color = this.color;//优化：避免每次都要Color转为Color32
            Sprite activeSprite = this.overrideSprite;
            Vector4 v = GetDrawingDimensions(preserveAspect);
            Vector4 outer = activeSprite != null ? Sprites.DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            UIVertex uiv = UIVertex.simpleVert;
            uiv.color = color;

            float tx0 = outer.x;
            float ty0 = outer.y;
            float tx1 = outer.z;
            float ty1 = outer.w;

            // Horizontal and vertical filled sprites are simple -- just end the Image prematurely
            if (fillMethod == FillMethod.Horizontal || fillMethod == FillMethod.Vertical)
            {
                if (fillMethod == FillMethod.Horizontal)
                {
                    float fill = (tx1 - tx0) * fillAmount;

                    if (fillAmount == 1)
                    {
                        v.x = v.z - (v.z - v.x) * fillAmount;
                        tx0 = tx1 - fill;
                    }
                    else
                    {
                        v.z = v.x + (v.z - v.x) * fillAmount;
                        tx1 = tx0 + fill;
                    }
                }
                else if (fillMethod == FillMethod.Vertical)
                {
                    float fill = (ty1 - ty0) * fillAmount;

                    if (fillAmount == 1)
                    {
                        v.y = v.w - (v.w - v.y) * fillAmount;
                        ty0 = ty1 - fill;
                    }
                    else
                    {
                        v.w = v.y + (v.w - v.y) * fillAmount;
                        ty1 = ty0 + fill;
                    }
                }
            }

            s_Xy[0] = new Vector2(v.x, v.y);
            s_Xy[1] = new Vector2(v.x, v.w);
            s_Xy[2] = new Vector2(v.z, v.w);
            s_Xy[3] = new Vector2(v.z, v.y);

            s_Uv[0] = new Vector2(tx0, ty0);
            s_Uv[1] = new Vector2(tx0, ty1);
            s_Uv[2] = new Vector2(tx1, ty1);
            s_Uv[3] = new Vector2(tx1, ty0);

            {
                if (fillAmount < 1f && fillMethod != FillMethod.Horizontal && fillMethod != FillMethod.Vertical)
                {
                    if (fillMethod == FillMethod.Radial90)
                    {
                        if (RadialCut(s_Xy, s_Uv, fillAmount, fillClockwise, fillOrigin))
                            AddQuad(toFill, s_Xy, color, s_Uv);
                    }
                    else if (fillMethod == FillMethod.Radial180)
                    {
                        for (int side = 0; side < 2; ++side)
                        {
                            float fx0, fx1, fy0, fy1;
                            int even = fillOrigin > 1 ? 1 : 0;

                            if (fillOrigin == 0 || fillOrigin == 2)
                            {
                                fy0 = 0f;
                                fy1 = 1f;
                                if (side == even)
                                {
                                    fx0 = 0f;
                                    fx1 = 0.5f;
                                }
                                else
                                {
                                    fx0 = 0.5f;
                                    fx1 = 1f;
                                }
                            }
                            else
                            {
                                fx0 = 0f;
                                fx1 = 1f;
                                if (side == even)
                                {
                                    fy0 = 0.5f;
                                    fy1 = 1f;
                                }
                                else
                                {
                                    fy0 = 0f;
                                    fy1 = 0.5f;
                                }
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = fillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((side + fillOrigin + 3) % 4)))
                            {
                                AddQuad(toFill, s_Xy, color, s_Uv);
                            }
                        }
                    }
                    else if (fillMethod == FillMethod.Radial360)
                    {
                        for (int corner = 0; corner < 4; ++corner)
                        {
                            float fx0, fx1, fy0, fy1;

                            if (corner < 2)
                            {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else
                            {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }

                            if (corner == 0 || corner == 3)
                            {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                            else
                            {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }

                            s_Xy[0].x = Mathf.Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Mathf.Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Mathf.Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Mathf.Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Mathf.Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Mathf.Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Mathf.Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Mathf.Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = fillClockwise ?
                                fillAmount * 4f - ((corner + fillOrigin) % 4) :
                                fillAmount * 4f - (3 - ((corner + fillOrigin) % 4));

                            if (RadialCut(s_Xy, s_Uv, Mathf.Clamp01(val), fillClockwise, ((corner + 2) % 4)))
                                AddQuad(toFill, s_Xy, color, s_Uv);
                        }
                    }
                }
                else
                {
                    AddQuad(toFill, s_Xy, color, s_Uv);
                }
            }
        }
        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
        {
            // Nothing to fill
            if (fill < 0.001f) return false;

            // Even corners invert the fill direction
            if ((corner & 1) == 1) invert = !invert;

            // Nothing to adjust
            if (!invert && fill > 0.999f) return true;

            // Convert 0-1 value into 0 to 90 degrees angle in radians
            float angle = Mathf.Clamp01(fill);
            if (invert) angle = 1f - angle;
            angle *= 90f * Mathf.Deg2Rad;

            // Calculate the effective X and Y factors
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            RadialCut(xy, cos, sin, invert, corner);
            RadialCut(uv, cos, sin, invert, corner);
            return true;
        }
        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>
        static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
        {
            int i0 = corner;
            int i1 = ((corner + 1) % 4);
            int i2 = ((corner + 2) % 4);
            int i3 = ((corner + 3) % 4);

            if ((corner & 1) == 1)
            {
                if (sin > cos)
                {
                    cos /= sin;
                    sin = 1f;

                    if (invert)
                    {
                        xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i2].x = xy[i1].x;
                    }
                }
                else if (cos > sin)
                {
                    sin /= cos;
                    cos = 1f;

                    if (!invert)
                    {
                        xy[i2].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i3].y = xy[i2].y;
                    }
                }
                else
                {
                    cos = 1f;
                    sin = 1f;
                }

                if (!invert) xy[i3].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                else xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
            }
            else
            {
                if (cos > sin)
                {
                    sin /= cos;
                    cos = 1f;

                    if (!invert)
                    {
                        xy[i1].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i2].y = xy[i1].y;
                    }
                }
                else if (sin > cos)
                {
                    cos /= sin;
                    sin = 1f;

                    if (invert)
                    {
                        xy[i2].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i3].x = xy[i2].x;
                    }
                }
                else
                {
                    cos = 1f;
                    sin = 1f;
                }

                if (invert) xy[i3].y = Mathf.Lerp(xy[i0].y, xy[i2].y, sin);
                else xy[i1].x = Mathf.Lerp(xy[i0].x, xy[i2].x, cos);
            }
        }
        #endregion
        #region 多边形碰撞检测

        public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
        {
            if (!m_UsePolyMesh || !m_UsePolyRaycastTarget)
            {
                return base.IsRaycastLocationValid(screenPoint, eventCamera);
            }

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, eventCamera, out local);

            var triangles = overrideSprite.triangles;
            for (var i = 0; i < triangles.Length; i += 3)
            {
                if (IsInTriangle(m_PolyMeshVertices[triangles[i]],
                    m_PolyMeshVertices[triangles[i + 1]],
                    m_PolyMeshVertices[triangles[i + 2]],
                    local))
                {
                    return true;
                }
            }

            return false;
        }

        // see more info at: http://oldking.wang/20ae1da0-d6d5-11e8-b3e6-29fe0b430026/
        private static bool IsInTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
        {
            var v0 = C - A;
            var v1 = B - A;
            var v2 = P - A;

            var dot00 = Vector2.Dot(v0, v0);
            var dot01 = Vector2.Dot(v0, v1);
            var dot02 = Vector2.Dot(v0, v2);
            var dot11 = Vector2.Dot(v1, v1);
            var dot12 = Vector2.Dot(v1, v2);

            var inverDeno = 1 / (dot00 * dot11 - dot01 * dot01);

            var u = (dot11 * dot02 - dot01 * dot12) * inverDeno;
            if (u < 0 || u > 1)
            {
                return false;
            }

            var v = (dot00 * dot12 - dot01 * dot02) * inverDeno;
            if (v < 0 || v > 1)
            {
                return false;
            }

            return u + v <= 1;
        }
        #endregion
    }
}
