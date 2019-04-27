using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UnityEngine.UI
{
    public class LImagePoly : LImage
    {
        #region PolyImage属性
        private Vector3[] m_PolyMeshVertices;
        [SerializeField]
        private bool m_UsePolyRaycastTarget = false;
        public bool usePolyRaycastTarget { get { return m_UsePolyRaycastTarget; } set { if (SetPropertyUtility.SetStruct(ref m_UsePolyRaycastTarget, value)) SetVerticesDirty(); } }
        [SerializeField]
        private bool m_UsePolyMesh = true;
        public bool usePolyMesh { get { return m_UsePolyMesh; } set { if (SetPropertyUtility.SetStruct(ref m_UsePolyMesh, value)) SetVerticesDirty(); } }
        #endregion
        /// <summary>
        /// Update the UI renderer mesh.
        /// </summary>
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            m_PolyMeshVertices = null;
            if (overrideSprite == null || !m_UsePolyMesh)
            {
                base.OnPopulateMesh(toFill);
                return;
            }
            switch (type)
            {
                case Type.Simple:
                    GenerateSimplePolySprite(toFill, preserveAspect);
                    break;
                case Type.Sliced:
                    GenerateSlicedPolySprite(toFill);
                    break;
                default:
                    base.OnPopulateMesh(toFill);
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
            Vector3 vert = new Vector3();
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
                vert.x = x;
                vert.y = y;
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
        static float[] s_VertScratch_Dx = new float[3];
        static float[] s_VertScratch_Dy = new float[3];
        static float[] s_UVScratch_Dx = new float[3];
        static float[] s_UVScratch_Dy = new float[3];
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
                padding = GetPadding(activeSprite);
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
            //减少重复计算量(空间换时间)
            for (int i = 0; i < 3; i++)
            {
                s_VertScratch_Dx[i] = s_VertScratch[i+1].x - s_VertScratch[i].x;
                s_VertScratch_Dy[i] = s_VertScratch[i+1].y - s_VertScratch[i].y;
                s_UVScratch_Dx[i] = s_UVScratch[i+1].x - s_UVScratch[i].x;
                s_UVScratch_Dy[i] = s_UVScratch[i+1].y - s_UVScratch[i].y;
            }
            for (int i = 0; i < uvsCount; i++)
            {
                Vector2 _uv = s_Uvs[i];
                int x = XSlot(_uv.x);
                int y = YSlot(_uv.y);

                float kX = (_uv.x - s_UVScratch[x].x) / s_UVScratch_Dx[x];
                float kY = (_uv.y - s_UVScratch[y].y) / s_UVScratch_Dy[y];

                pos.x = kX * s_VertScratch_Dx[x] + s_VertScratch[x].x;
                pos.y = kY * s_VertScratch_Dy[y] + s_VertScratch[y].y;

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
