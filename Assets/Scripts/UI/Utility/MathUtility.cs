using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtility
{
    public static float Cross(Vector2 lhs, Vector2 rhs)
    {
        return lhs.x * rhs.y - lhs.y * rhs.x;
    }
    // 直线切割mesh获得交点
	static ushort[] splitTriCache1 = new ushort[5];
	static int splitTriCache1_Count = 0;
	static ushort[] splitTriCache2 = new ushort[5];
	static int splitTriCache2_Count = 0;
    public static List<ushort> LineCut(
        List<Vector2> uvs, List<ushort> triangles,
        Vector2 start, float angle, Func<Vector2, bool> predict = null)
    {
        List<ushort> splitTriangles = new List<ushort>(triangles.Count*2);
        var offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        for (int i = 0; i < triangles.Count; i += 3)
        {
			splitTriCache1_Count = 0;
			splitTriCache2_Count = 0;
            for (int j = 0; j < 3; j++)
            {
                ushort tri1 = triangles[i + j], tri2 = triangles[i + (j + 1) % 3];
                Vector2 uv1 = uvs[tri1], uv2 = uvs[tri2];

                float sign1 = Cross(offset, uv1 - start), sign2 = Cross(offset, uv2 - start);

                if (sign1 <= 0){
                    splitTriCache1[splitTriCache1_Count] = tri1;
					splitTriCache1_Count++;
				}
                if (sign1 >= 0){
                    splitTriCache2[splitTriCache2_Count] = tri1;
					splitTriCache2_Count++;
				}

                if (sign1 * sign2 < 0)
                {
                    // Line Intersects!
                    var diff = uv2 - uv1;
                    float t1 = -sign1 / Cross(offset, diff);
                    var p = uv1 + diff * t1;

                    ushort idx = (ushort)uvs.Count;
                    uvs.Add(p);

                    splitTriCache1[splitTriCache1_Count] = idx;
                    splitTriCache2[splitTriCache2_Count] = idx;
                    splitTriCache1_Count++;
                    splitTriCache2_Count++;
                }
            }

            if (splitTriCache1_Count >= 3)
            {
                for (int j = 2; j < splitTriCache1_Count; j++)
                {
                    if (predict != null)
                    {
                        Vector2 center = (uvs[splitTriCache1[0]] + uvs[splitTriCache1[j - 1]] + uvs[splitTriCache1[j]]) / 3;
                        if (!predict(center)) continue;
                    }
                    splitTriangles.Add(splitTriCache1[0]);
                    splitTriangles.Add(splitTriCache1[j - 1]);
                    splitTriangles.Add(splitTriCache1[j]);
                }
            }
            if (splitTriCache2_Count >= 3)
            {
                for (int j = 2; j < splitTriCache2_Count; j++)
                {
                    if (predict != null)
                    {
                        Vector2 center = (uvs[splitTriCache2[0]] + uvs[splitTriCache2[j - 1]] + uvs[splitTriCache2[j]]) / 3;
                        if (!predict(center)) continue;
                    }
                    splitTriangles.Add(splitTriCache2[0]);
                    splitTriangles.Add(splitTriCache2[j - 1]);
                    splitTriangles.Add(splitTriCache2[j]);
                }
            }
        }

        return splitTriangles;
    }
}
