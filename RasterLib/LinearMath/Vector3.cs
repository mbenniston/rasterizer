using System;

namespace RasterLib.LinearMath;

public struct Vector3 : IEquatable<Vector3>
{
    public float x, y, z;

    public Vector3(float X = 0.0f, float Y = 0.0f, float Z = 0.0f)
    {
        x = X;
        y = Y;
        z = Z;
    }

    public Vector3(Vector2 vec, float Z = 0.0f)
    {
        x = vec.x;
        y = vec.y;
        z = Z;
    }

    public Vector2 xy
    {
        get { return new Vector2(x, y); }
    }

    public Vector3 Abs
    {
        get { return new Vector3(MathF.Abs(x), MathF.Abs(y), MathF.Abs(z)); }
    }

    public static Vector3 Lerp(Vector3 a, Vector3 b, float f)
    {
        return a * (1.0f - f) + b * f;
    }

    public static float Dot(Vector3 left, Vector3 right)
    {
        return left.x * right.x + left.y * right.y + left.z * right.z;
    }

    public static Vector3 Cross(Vector3 left, Vector3 right)
    {
        return new Vector3(
            left.y * right.z - left.z * right.y,
            left.z * right.x - left.x * right.z,
            left.x * right.y - left.y * right.x
        );
    }

    public static Vector3 Reflect(Vector3 incident, Vector3 normal)
    {
        return incident - normal * Dot(normal, incident) * 2.0f;
    }

    public static Vector3 Refract(Vector3 incident, Vector3 normal, float eta)
    {
        float dotValue = Dot(normal, incident);
        float k = 1.0f - eta * eta * (1.0f - dotValue * dotValue);
        return (incident * eta - normal * (eta * dotValue + MathF.Sqrt(k))) * (k >= 0.0f ? 1.0f : 0.0f);
    }

    public static Vector3 operator +(Vector3 left, Vector3 right) => new(left.x + right.x, left.y + right.y, left.z + right.z);

    public static Vector3 operator -(Vector3 left, Vector3 right) => new(left.x - right.x, left.y - right.y, left.z - right.z);
    public static Vector3 operator -(Vector3 vec) => new(-vec.x, -vec.y, -vec.z);

    public static Vector3 operator *(Vector3 left, Vector3 right) => new(left.x * right.x, left.y * right.y, left.z * right.z);
    public static Vector3 operator *(Vector3 left, float right) => new(left.x * right, left.y * right, left.z * right);

    public static Vector3 operator /(Vector3 left, Vector3 right) => new(left.x / right.x, left.y / right.y, left.z / right.z);
    public static Vector3 operator /(Vector3 left, float right) => new(left.x / right, left.y / right, left.z / right);

    public float LengthSquared { get { return Dot(this, this); } }
    public float Length { get { return MathF.Sqrt(LengthSquared); } }
    public Vector3 Normalized { get { return this / Length; } }

    public bool Equals(Vector3 other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public static bool operator ==(Vector3 left, Vector3 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Vector3 left, Vector3 right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
        return Equals((Vector3)obj);
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() + y.GetHashCode() + z.GetHashCode();
    }

    public override string ToString()
    {
        return $"{x} {y} {z}";
    }
}

