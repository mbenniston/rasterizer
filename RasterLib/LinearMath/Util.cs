using System;

namespace RasterLib.LinearMath;

public class MathUtil
{
    public static float Lerp(float a, float b, float f)
    {
        return a * (1.0f - f) + b * f;
    }

    public static float Clamp(float x, float minVal, float maxVal)
    {
        return MathF.Min(MathF.Max(minVal, x), maxVal);
    }

    public static int Clamp(int x, int minVal, int maxVal)
    {
        return Math.Min(Math.Max(minVal, x), maxVal);
    }

    public static float Radians(float degrees)
    {
        return degrees * MathF.PI / 180.0f;
    }

    /// <summary>
    ///     Get the closest distance from a point to a given 2D plane 
    /// </summary>
    public static double DistanceFromPointToLine(double lineStartX, double lineStartY, double lineEndX, double lineEndY, double pointX, double pointY)
    {
        return (lineEndY - lineStartY) * pointX + (lineStartX - lineEndX) * pointY + lineEndX * lineStartY - lineStartX * lineEndY;
    }

    /// <summary>
    ///     Calculates the barycentric coordinates of a point given a triangle.
    /// </summary>
    public static (double, double, double)? Barycentric(Vector2 A, Vector2 B, Vector2 C, Vector2 point)
    {
        double a = DistanceFromPointToLine(B.x, B.y, C.x, C.y, point.x, point.y) / DistanceFromPointToLine(B.x, B.y, C.x, C.y, A.x, A.y);
        double b = DistanceFromPointToLine(C.x, C.y, A.x, A.y, point.x, point.y) / DistanceFromPointToLine(C.x, C.y, A.x, A.y, B.x, B.y);
        double c = DistanceFromPointToLine(A.x, A.y, B.x, B.y, point.x, point.y) / DistanceFromPointToLine(A.x, A.y, B.x, B.y, C.x, C.y);

        if (a >= 0.0 && a < 1.0 && b >= 0.0 && b < 1.0 && c >= 0.0 && c < 1.0)
        {
            return (a, b, c);
        }

        return null;
    }

    /// <summary>
    ///     Mixes three variables with three given factors
    /// </summary>
    public static float TriMix(float a, float b, float c, float alpha, float beta, float gamma)
    {
        return a * alpha + b * beta + c * gamma;
    }

    /// <summary>
    ///     Mixes three variables with three given factors
    /// </summary>
    public static Vector3 TriMix(Vector3 a, Vector3 b, Vector3 c, float alpha, float beta, float gamma)
    {
        return a * alpha + b * beta + c * gamma;
    }

    /// <summary>
    ///     Mixes three variables with three given factors
    /// </summary>
    public static Vector2 TriMix(Vector2 a, Vector2 b, Vector2 c, float alpha, float beta, float gamma)
    {
        return a * alpha + b * beta + c * gamma;
    }

    public static float DistancePointToPlane(Vector3 planeNormal, float planeD, Vector3 point)
    {
        return Vector3.Dot(planeNormal, point) - planeD;
    }

    public static (Vector3, float) GetPointOnPlaneFromLine(Vector3 planeNormal, float planeD, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 dir = (lineEnd - lineStart).Normalized;

        float t = (planeD - Vector3.Dot(planeNormal, lineStart)) / Vector3.Dot(planeNormal, dir);

        Vector3 point = lineStart + dir * t;

        return (point, (point - lineStart).Length / (lineEnd - lineStart).Length);
    }
}