using System;

namespace RasterLib.LinearMath;

public struct Vector4 : IEquatable<Vector4>
{
    public float x, y, z, w;

    public Vector4(float X = 0.0f, float Y = 0.0f, float Z = 0.0f, float W = 1.0f)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
    }

    public Vector4(Vector3 vec, float W = 1.0f)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
        w = W;
    }

    public float this[int index] => index switch
    {
        0 => x,
        1 => y,
        2 => z,
        _ => w,
    };

    public static Vector4 Lerp(Vector4 a, Vector4 b, float f)
    {
        return a * (1.0f - f) + b * f;
    }

    public static float Dot(Vector4 left, Vector4 right)
    {
        return left.x * right.x + left.y * right.y + left.z * right.z + left.w * right.w;
    }

    public Vector3 xyz { get { return new Vector3(x, y, z); } }
    public Vector2 xy { get { return new Vector2(x, y); } }

    public static Vector4 operator +(Vector4 left, Vector4 right) => new(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);
    public static Vector4 operator -(Vector4 left, Vector4 right) => new(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);
    public static Vector4 operator -(Vector4 vec) => new(-vec.x, -vec.y, -vec.z, -vec.w);
    public static Vector4 operator *(Vector4 left, Vector4 right) => new(left.x * right.x, left.y * right.y, left.z * right.z, left.w * right.w);
    public static Vector4 operator /(Vector4 left, Vector4 right) => new(left.x / right.x, left.y / right.y, left.z / right.z, left.w / right.w);

    public static Vector4 operator *(Vector4 left, float right) => new(left.x * right, left.y * right, left.z * right, left.w * right);
    public static Vector4 operator /(Vector4 left, float right) => new(left.x / right, left.y / right, left.z / right, left.w / right);

    public float LengthSquared { get { return Dot(this, this); } }
    public float Length { get { return MathF.Sqrt(LengthSquared); } }
    public Vector4 Normalized { get { return this / Length; } }

    public bool Equals(Vector4 other)
    {
        return x == other.x && y == other.y && z == other.z && w == other.w;
    }

    public static bool operator ==(Vector4 left, Vector4 right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Vector4 left, Vector4 right)
    {
        return !Equals(left, right);
    }

    public override bool Equals(object obj)
    {
        return Equals((Vector4)obj);
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() + y.GetHashCode();
    }

    public override string ToString()
    {
        return $"{x} {y} {z} {w}";
    }
}

