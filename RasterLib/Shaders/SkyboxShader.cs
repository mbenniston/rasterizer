using RasterLib.Core;
using RasterLib.LinearMath;
using RasterLib.Scene;

namespace RasterLib.Shaders;

public struct SkyboxShaderVarying : IInterpolatable<SkyboxShaderVarying>, IPerspectiveDivisable
{
    public Vector3 Position;

    public SkyboxShaderVarying(Vector3 position)
    {
        Position = position;
    }

    public SkyboxShaderVarying Interpolate(SkyboxShaderVarying interpolateWith, float t)
    {
        return new SkyboxShaderVarying(
            Vector3.Lerp(Position, interpolateWith.Position, t));
    }

    public void PerspectiveDivide(float z)
    {
        Position /= z;
    }
}

public struct SkyboxShaderInterpolatorOutput
{
    public Vector3 Position;
}

public class SkyboxShader : IShader<Mesh.Vertex, SkyboxShaderVarying, SkyboxShaderInterpolatorOutput>
{
    private ISampler3D m_environmentTexture;
    public Matrix4 ProjMatrix;
    public Matrix4 ViewMatrix = Matrix4.Identity();

    public (Vector4, SkyboxShaderVarying) ShadeVertex(Mesh.Vertex inVertex)
    {
        return (ProjMatrix * ViewMatrix * new Vector4(inVertex.Position), new SkyboxShaderVarying(inVertex.Position));
    }

    public SkyboxShaderInterpolatorOutput Interpolate(InterpolatorContext context, Triangle<SkyboxShaderVarying> triangle)
    {
        SkyboxShaderInterpolatorOutput output = new();

        output.Position = MathUtil.TriMix(
            triangle[0].Attribs.Position,
            triangle[1].Attribs.Position,
            triangle[2].Attribs.Position,
            context.Alpha, context.Beta, context.Gamma);
        output.Position /= context.W;

        return output;
    }

    public Vector4? ShadePixel(PixelShaderContext context, SkyboxShaderInterpolatorOutput interpolatedAttribs)
    {
        Vector3 direction = interpolatedAttribs.Position.Normalized;
        return new Vector4(m_environmentTexture.Sample(direction), 1.0f);
    }

    public SkyboxShader(ISampler3D sampler, Matrix4 projMatrix)
    {
        m_environmentTexture = sampler;
        ProjMatrix = projMatrix;
    }
}
