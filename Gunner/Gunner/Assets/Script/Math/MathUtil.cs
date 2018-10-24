using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtil
{
    public static float PointToAngle(Vector2 s,Vector2 e)
    {
        Vector2 dif = e - s;
        float radian = Mathf.Atan2(dif.y, dif.x);
        return radian * Mathf.Rad2Deg + 180;
    }

}
