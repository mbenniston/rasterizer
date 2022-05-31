using System;

namespace RasterLib.LinearMath;

public struct Vector2 : IEquatable<Vector2>
{
    public float x, y;

    public static Vector2 Lerp(Vector2 a, Vector2 b, float f)
    {
        return a * (1.0f - f) + b * f;
    }

    public Vector2(float X = 0.0f, float Y = 0.0f)
    {
        x = X;
        y = Y;
    }

    public static Vector2 operator +(Vector2 left, Vector2 right) => new(left.x + right.x, left.y + right.y);
    public static Vector2 operator -(Vector2 left, Vector2 right) => new(left.x - right.x, left.y - right.y);
    public static Vector2 operator -(Vector2 vec) => new(-vec.x, -vec.y);
    public static Vector2 operator *(Vector2 left, Vector2 right) => new(left.x * right.x, left.y * right.y);
    public static Vector2 operator /(Vector2 left, Vector2 right) => new(left.x / right.x, left.y / right.y);

    public static Vector2 operator *(Vector2 left, float right) => new(left.x * right, left.y * right);
    public static Vector2 operator /(Vector2 left, float right) => new(left.x / right, left.y / right);

    public static float Dot(Vector2 left, Vector2 right)
    {
        return left.x * right.x + left.y * right.y;
    }

    public float LengthSquared { get { return Dot(this, this); } }
    public float Length { get { return MathF.Sqrt(LengthSquared); } }
    public Vector2 Normalized { get { return this / Length; } }

    public override bool Equals(object obj)
    {
        return Equals((Vector2)obj);
    }

    public static bool operator !=(Vector2 left, Vector2 right)
    {
        return !Equals(left, right);
    }

    public bool Equals(Vector2 other)
    {
        return x == other.x && y == other.y;
    }

    public static bool operator ==(Vector2 left, Vector2 right)
    {
        return Equals(left, right);
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() + y.GetHashCode();
    }

    public override string ToString()
    {
        return $"{x} {y}";
    }
}