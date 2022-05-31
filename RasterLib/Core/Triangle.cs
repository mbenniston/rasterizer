using RasterLib.LinearMath;
using System;

namespace RasterLib.Core;

/// <summary>
///     Represents a triangle specifed with 3 coordinates and 3 customizable vertex attributes
/// </summary>
public struct Triangle<VertexData>
{
    public struct Vertex
    {
        public Vector4 Position;
        public VertexData Attribs;

        public Vertex(Vector4 position, VertexData attribs)
        {
            Position = position;
            Attribs = attribs;
        }
    }

    public Vertex VertexA;
    public Vertex VertexB;
    public Vertex VertexC;

    public Vertex this[int index] => index switch
    {
        0 => VertexA,
        1 => VertexB,
        _ => VertexC,
    };
    
    public Triangle(Vertex v1, Vertex v2, Vertex v3)
    {
        VertexA = v1;
        VertexB = v2;
        VertexC = v3;
    }

    public Triangle((Vector4, VertexData) v1, (Vector4, VertexData) v2, (Vector4, VertexData) v3)
    {
        VertexA = new Vertex(v1.Item1, v1.Item2);
        VertexB = new Vertex(v2.Item1, v2.Item2);
        VertexC = new Vertex(v3.Item1, v3.Item2);
    }

    /// <summary>
    ///     Convert the triangles vertices into screen space coordinates
    /// </summary>
    public void ConvertFromNDC(float width, float height)
    {
        VertexA.Position.x = (VertexA.Position.x + 1.0f) * 0.5f * width;
        VertexA.Position.y = (-VertexA.Position.y + 1.0f) * 0.5f * height;

        VertexB.Position.x = (VertexB.Position.x + 1.0f) * 0.5f * width;
        VertexB.Position.y = (-VertexB.Position.y + 1.0f) * 0.5f * height;

        VertexC.Position.x = (VertexC.Position.x + 1.0f) * 0.5f * width;
        VertexC.Position.y = (-VertexC.Position.y + 1.0f) * 0.5f * height;
    }

    /// <summary>
    ///     Calculates the normal described by the winding order of the vertices.
    ///     Assumes vertices are provided in a clockwise winding order.
    /// </summary>
    public Vector3 CalcWindingNormal()
    {
        return Vector3.Cross(
            (VertexB.Position.xyz - VertexA.Position.xyz).Normalized,
            (VertexC.Position.xyz - VertexA.Position.xyz).Normalized);
    }

    public (Vector2, Vector2) CalcXYBounds()
    {
        return (
            new Vector2(
                MathF.Min(MathF.Min(VertexA.Position.x, VertexB.Position.x), VertexC.Position.x),
                MathF.Min(MathF.Min(VertexA.Position.y, VertexB.Position.y), VertexC.Position.y)),
            new Vector2(
                (MathF.Max(MathF.Max(VertexA.Position.x, VertexB.Position.x), VertexC.Position.x)),
                (MathF.Max(MathF.Max(VertexA.Position.y, VertexB.Position.y), VertexC.Position.y))));
    }
}