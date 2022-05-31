namespace RasterLib.Core;

public static class Util
{
    /// <summary>
    ///     Performs perspective division over a triangle and its vertex attributes
    /// </summary>
    public static void PerspectiveDivide<VertexData>(ref Triangle<VertexData> tri) where VertexData : IPerspectiveDivisable
    {
        float w1 = tri.VertexA.Position.w,
              w2 = tri.VertexB.Position.w,
              w3 = tri.VertexC.Position.w;

        tri.VertexA.Position /= w1;
        tri.VertexB.Position /= w2;
        tri.VertexC.Position /= w3;

        tri.VertexA.Position.w = 1.0f / w1;
        tri.VertexB.Position.w = 1.0f / w2;
        tri.VertexC.Position.w = 1.0f / w3;

        tri.VertexA.Attribs.PerspectiveDivide(w1);
        tri.VertexB.Attribs.PerspectiveDivide(w2);
        tri.VertexC.Attribs.PerspectiveDivide(w3);
    }
}
