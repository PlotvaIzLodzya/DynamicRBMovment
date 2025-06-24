using UnityEngine;

public static class Extensions
{
    public static bool IsSameDirection(this Vector2 v1, Vector2 v2)
    {
        return Vector2.Angle(v1, v2) < 90;
    }

    public static Vector2 GetHorizontal(this Vector2 v)
    {
        return new Vector2(v.x, 0);
    }
}