namespace RasterLib.LinearMath;

public struct Plane
{
    public Vector3 direction;
    public float distance;

    public Plane(Vector3 direction, float distance)
    {
        this.direction = direction;
        this.distance = distance;
    }

    public Plane(Vector3 direction, Vector3 pointOnPlane)
    {
        distance = Vector3.Dot(direction, pointOnPlane);
        this.direction = direction;
    }

    public float DistanceToPoint(Vector3 point)
    {
        return Vector3.Dot(direction, point) - distance;
    }

    public (Vector3, float) GetPointOnPlaneFromLine(Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 lineDir = (lineEnd - lineStart).Normalized;

        float t = (distance - Vector3.Dot(direction, lineStart)) / Vector3.Dot(direction, lineDir);

        Vector3 point = lineStart + lineDir * t;

        return (point, (point - lineStart).Length / (lineEnd - lineStart).Length);
    }
}