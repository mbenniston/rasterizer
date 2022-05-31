using RasterLib.LinearMath;
using RasterLib.Core;
using RasterLib.Scene;

namespace RasterLib.Shaders;

public struct ReflectionShaderVarying : IInterpolatable<ReflectionShaderVarying>, IPerspectiveDivisable
{
    public Vector3 ToCamera;
    public Vector3 Normal;

    public ReflectionShaderVarying(Vector3 toCamera, Vector3 normal)
    {
        ToCamera = toCamera;
        Normal = normal;
    }

    public ReflectionShaderVarying Interpolate(ReflectionShaderVarying interpolateWith, float t)
    {
        return new ReflectionShaderVarying(
            Vector3.Lerp(ToCamera, interpolateWith.ToCamera, t),
            Vector3.Lerp(Normal, interpolateWith.Normal, t));
    }

    public void PerspectiveDivide(float z)
    {
        ToCamera /= z;
        Normal /= z;
    }
}

public struct ReflectionShaderInterpolatorOutput
{
    public Vector3 ToCamera, Normal;
    public float Depth;
}

/// <summary>
///     Shader that uses a skybox to get the colour of the object depending on the viewing angle
/// </summary>
public class ReflectionShader : IShader<Mesh.Vertex, ReflectionShaderVarying, ReflectionShaderInterpolatorOutput>
{
    public ISampler3D EnvironmentTexture;
    public DepthBuffer DepthBuffer;
    public Matrix4 ProjViewMatrix = Matrix4.Identity();
    public Matrix4 ModelMatrix = Matrix4.Identity();
    public Matrix4 NormalMatrix = Matrix4.Identity();
    public Vector3 CameraPosition = new();
    public float IDR = 1.044f;
    public float RefractAmount = 0.7f;

    public (Vector4, ReflectionShaderVarying) ShadeVertex(Mesh.Vertex inVertex)
    {
        Vector4 WorldSpacePosition = ModelMatrix * new Vector4(inVertex.Position);

        return (
            ProjViewMatrix * WorldSpacePosition, 
            new ReflectionShaderVarying(
                CameraPosition - WorldSpacePosition.xyz,
                (NormalMatrix * new Vector4(inVertex.Normal, 0.0f)).xyz)
            );
    }

    public ReflectionShaderInterpolatorOutput Interpolate(InterpolatorContext context, Triangle<ReflectionShaderVarying> triangle)
    {
        ReflectionShaderInterpolatorOutput output = new();

        output.Depth = MathUtil.TriMix(
          triangle[0].Position.z,
          triangle[1].Position.z,
          triangle[2].Position.z,
          context.Alpha, context.Beta, context.Gamma);

        output.ToCamera = MathUtil.TriMix(
            triangle[0].Attribs.ToCamera,
            triangle[1].Attribs.ToCamera,
            triangle[2].Attribs.ToCamera,
            context.Alpha, context.Beta, context.Gamma);
        output.ToCamera /= context.W;

        output.Normal = MathUtil.TriMix(
            triangle[0].Attribs.Normal,
            triangle[1].Attribs.Normal,
            triangle[2].Attribs.Normal,
            context.Alpha, context.Beta, context.Gamma);
        output.Normal /= context.W;

        return output;
    }

    public Vector4? ShadePixel(PixelShaderContext context, ReflectionShaderInterpolatorOutput interpolatedAttribs)
    {
        float depth = interpolatedAttribs.Depth;
        Vector3 fromCamera = -interpolatedAttribs.ToCamera.Normalized;
        Vector3 normal = interpolatedAttribs.Normal.Normalized;

        if (depth < DepthBuffer.GetDepth(context.bufferX, context.bufferY))
        {
            DepthBuffer.SetDepth(context.bufferX, context.bufferY, depth);

            Vector3 reflectDir = Vector3.Reflect(fromCamera, normal);
            Vector3 refractDir = Vector3.Refract(fromCamera, normal, 1.0f / IDR);

            return new Vector4(Vector3.Lerp(
                EnvironmentTexture.Sample(reflectDir.Normalized),
                EnvironmentTexture.Sample(refractDir.Normalized), RefractAmount), 1.0f);
        }

        return null;

    }

    public ReflectionShader(ISampler3D sampler, DepthBuffer depthBuffer)
    {
        EnvironmentTexture = sampler;
        DepthBuffer = depthBuffer;
    }
}
