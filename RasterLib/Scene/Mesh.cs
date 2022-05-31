using RasterLib.Core;
using RasterLib.LinearMath;
using RasterLib.Shaders;

namespace RasterLib.Scene;

/// <summary>
///     Contains a collection of vertices that can be drawn using a shader
/// </summary>
public class Mesh
{
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoord;

        public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            Position = position;
            Normal = normal;
            TexCoord = texCoord;
        }
    }

    public Vertex[] Vertices { get; private set; }

    /// <summary>
    ///     Draws the vertices within the mesh as triangles with a given shader
    /// </summary>
    /// <returns>The number of triangles drawn</returns>
    public int Draw<VSout, InterpolatorOutput>(
        IShader<Vertex, VSout, InterpolatorOutput> shader,
        FrameBuffer buffer, Pipeline.PipeLineOptions options)
            where VSout : IInterpolatable<VSout>, IPerspectiveDivisable
    {
        int trisDrawn = 0;

        for (int i = 0; i < Vertices.Length; i += 3)
        {
            trisDrawn += Pipeline.DrawTriangle(shader, Vertices[i], Vertices[i + 1], Vertices[i + 2], buffer, options);
        }

        return trisDrawn;
    }

    public Mesh(Vertex[] vertices)
    {
        Vertices = vertices;
    }
}