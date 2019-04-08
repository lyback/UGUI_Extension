using UnityEngine;
/*
USAGE
[VectorRange(minX, maxX, minY, maxY, clamped)]
public Vector2 yourVector;
*/
public class VectorRangeAttribute : PropertyAttribute
{
    public readonly float fMinX, fMaxX, fMinY, fMaxY;
    public readonly bool bClamp;
    public VectorRangeAttribute(float fMinX, float fMaxX, float fMinY, float fMaxY, bool bClamp)
    {
        this.fMinX = fMinX;
        this.fMaxX = fMaxX;
        this.fMinY = fMinY;
        this.fMaxY = fMaxY;
        this.bClamp = bClamp;
    }
}
