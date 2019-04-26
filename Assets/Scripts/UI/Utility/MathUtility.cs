using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtility
{

    public static float Cross(Vector2 axis, Vector2 point)
    {
        return axis.x * point.y - axis.y * point.x;
    }
    public static float Cross(Vector2 axis, Vector2 point, Vector2 offset)
    {
        return axis.x * (point.y - offset.y) - axis.y * (point.x - offset.x);
    }
    // 直线切割mesh获得交点
    static ushort[] splitTriRightCache = new ushort[5];
    static int splitTriRight_Count = 0;
    static ushort[] splitTriLeftCache = new ushort[5];
    static int splitTriLeft_Count = 0;
    public static List<ushort> LineCut(
        List<Vector2> uvs, List<ushort> triangles,
        Vector2 start, float angle, Func<Vector2, bool> predict = null)
    {
        List<ushort> splitTriangles = new List<ushort>(triangles.Count * 2);
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        for (int i = 0; i < triangles.Count; i += 3)
        {
            splitTriRight_Count = 0;
            splitTriLeft_Count = 0;
            for (int j = 0; j < 3; j++)
            {
                ushort tri1 = triangles[i + j], tri2 = triangles[i + (j + 1) % 3];
                Vector2 uv1 = uvs[tri1], uv2 = uvs[tri2];

                float sign1 = Cross(offset, uv1 - start), sign2 = Cross(offset, uv2 - start);

                if (sign1 <= 0)
                {
                    splitTriRightCache[splitTriRight_Count] = tri1;
                    splitTriRight_Count++;
                }
                if (sign1 >= 0)
                {
                    splitTriLeftCache[splitTriLeft_Count] = tri1;
                    splitTriLeft_Count++;
                }

                if (sign1 * sign2 < 0)
                {
                    // Line Intersects!
                    var diff = uv2 - uv1;
                    float t1 = -sign1 / Cross(offset, diff);
                    var p = uv1 + diff * t1;

                    ushort idx = (ushort)uvs.Count;
                    uvs.Add(p);

                    splitTriRightCache[splitTriRight_Count] = idx;
                    splitTriLeftCache[splitTriLeft_Count] = idx;
                    splitTriRight_Count++;
                    splitTriLeft_Count++;
                }
            }

            if (splitTriRight_Count >= 3)
            {
                for (int j = 2; j < splitTriRight_Count; j++)
                {
                    if (predict != null)
                    {
                        Vector2 center = (uvs[splitTriRightCache[0]] + uvs[splitTriRightCache[j - 1]] + uvs[splitTriRightCache[j]]) / 3;
                        if (!predict(center)) continue;
                    }
                    splitTriangles.Add(splitTriRightCache[0]);
                    splitTriangles.Add(splitTriRightCache[j - 1]);
                    splitTriangles.Add(splitTriRightCache[j]);
                }
            }
            if (splitTriLeft_Count >= 3)
            {
                for (int j = 2; j < splitTriLeft_Count; j++)
                {
                    if (predict != null)
                    {
                        Vector2 center = (uvs[splitTriLeftCache[0]] + uvs[splitTriLeftCache[j - 1]] + uvs[splitTriLeftCache[j]]) / 3;
                        if (!predict(center)) continue;
                    }
                    splitTriangles.Add(splitTriLeftCache[0]);
                    splitTriangles.Add(splitTriLeftCache[j - 1]);
                    splitTriangles.Add(splitTriLeftCache[j]);
                }
            }
        }

        return splitTriangles;
    }

    static ushort[] tri = new ushort[3];
    static Vector2[] uv = new Vector2[3];
    static float[] sign = new float[3];
    public static void NewLineCut(ref List<Vector2> uvs, ref List<ushort> triangles, Vector2 linePoint, float angle)
    {
        Vector2 vecDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); //射线方向向量
        // Vector2 vecLine = vecDir + linePoint; //切割的射线
        int triangleCount = triangles.Count;
        ushort uvCount = (ushort)uvs.Count;
        for (int i = 0; i < triangleCount; i += 3)
        {
            ushort idCount = 0;

            tri[0] = triangles[i];
            tri[1] = triangles[i + 1];
            tri[2] = triangles[i + 2];
            uv[0] = uvs[tri[0]];
            uv[1] = uvs[tri[1]];
            uv[2] = uvs[tri[2]];
            sign[0] = Cross(vecDir, uv[0], linePoint);//计算uv1点在射线左边还是右边(大于0左边，小于0右边)
            sign[1] = Cross(vecDir, uv[1], linePoint);//计算uv2点在射线左边还是右边(大于0左边，小于0右边)
            sign[2] = Cross(vecDir, uv[2], linePoint);//计算uv3点在射线左边还是右边(大于0左边，小于0右边)
            splitTriRight_Count = 0;
            splitTriLeft_Count = 0;
            for (int j = 0; j < 3; j++)
            {

                Vector2 uv_cur = uv[j];
                Vector2 uv_next = uv[(j + 1) % 3];
                float sign_cur = sign[j];
                float sign_next = sign[(j + 1) % 3];

                if (sign_cur <= 0)
                {
                    splitTriRightCache[splitTriRight_Count] = tri[j];
                    splitTriRight_Count++;
                }
                if (sign_cur >= 0)
                {
                    splitTriLeftCache[splitTriLeft_Count] = tri[j];
                    splitTriLeft_Count++;
                }

                //异号说明uv1和uv2的线段和射线相交
                if (sign_cur * sign_next < 0)
                {
                    //计算交点
                    var diff = uv_next - uv_cur;
                    float t1 = -sign_cur / Cross(vecDir, diff);
                    var p = uv_cur + diff * t1;
                    //记录交点信息
                    splitTriRightCache[splitTriRight_Count] = uvCount;
                    splitTriRight_Count++;
                    splitTriLeftCache[splitTriLeft_Count] = uvCount;
                    splitTriLeft_Count++;
                    uvCount++;
                    idCount++;
                    uvs.Add(p);
                }
            }
            if (idCount == 1)//一个交点
            {
                triangles[i] = splitTriRightCache[0];
                triangles[i + 1] = splitTriRightCache[1];
                triangles[i + 2] = splitTriRightCache[2];
                triangles.Add(splitTriLeftCache[0]);
                triangles.Add(splitTriLeftCache[1]);
                triangles.Add(splitTriLeftCache[2]);
            }
            else if (idCount == 2)//两个交点
            {
                if (splitTriRight_Count > splitTriLeft_Count)
                {
                    triangles[i] = splitTriLeftCache[0];
                    triangles[i + 1] = splitTriLeftCache[1];
                    triangles[i + 2] = splitTriLeftCache[2];
                    for (int j = 2; j < splitTriRight_Count; j++)
                    {
                        triangles.Add(splitTriRightCache[0]);
                        triangles.Add(splitTriRightCache[j - 1]);
                        triangles.Add(splitTriRightCache[j]);
                    }
                }
                else
                {
                    triangles[i] = splitTriRightCache[0];
                    triangles[i + 1] = splitTriRightCache[1];
                    triangles[i + 2] = splitTriRightCache[2];
                    for (int j = 2; j < splitTriLeft_Count; j++)
                    {
                        triangles.Add(splitTriLeftCache[0]);
                        triangles.Add(splitTriLeftCache[j - 1]);
                        triangles.Add(splitTriLeftCache[j]);
                    }
                }
            }
        }
    }
}
