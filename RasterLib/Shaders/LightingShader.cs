using RasterLib.Core;
using RasterLib.LinearMath;
using RasterLib.Scene;
using System;
using System.Collections.Generic;

namespace RasterLib.Shaders;

public class PointLight
{
    public Vector3 Position = new Vector3();
    public Vector3 Colour = new Vector3();
    public Vector3 Attenuation = new Vector3(1, 0, 0);

    public PointLight()
    {
    }
}

public struct Material
{
    public Vector3 Ambient = new Vector3();
    public float SpecularPower = 128.0f;
    public float SpecularStrength = 1.0f;

    public Material()
    {
    }
}

public struct LightingShaderVarying : IInterpolatable<LightingShaderVarying>, IPerspectiveDivisable
{
    public Vector3 WorldSpacePosition;
    public Vector3 ToCamera;
    public Vector3 Normal;
    public Vector2 TexCoord;

    public LightingShaderVarying(Vector3 worldSpacePosition, Vector3 toCamera, Vector3 normal, Vector2 texCoord)
    {
        ToCamera = toCamera;
        Normal = normal;
        TexCoord = texCoord;
        WorldSpacePosition = worldSpacePosition;
    }

    public LightingShaderVarying Interpolate(LightingShaderVarying interpolateWith, float t)
    {
        return new LightingShaderVarying(
            Vector3.Lerp(WorldSpacePosition, interpolateWith.WorldSpacePosition, t),
            Vector3.Lerp(ToCamera, interpolateWith.ToCamera, t),
            Vector3.Lerp(Normal, interpolateWith.Normal, t),
            Vector2.Lerp(TexCoord, interpolateWith.TexCoord, t));
    }

    public void PerspectiveDivide(float z)
    {
        ToCamera /= z;
        Normal /= z;
        TexCoord /= z;
        WorldSpacePosition /= z;
    }
}

public struct LightingShaderInterpolatorOutput
{
    public Vector3 WorldSpacePosition;
    public Vector3 ToCamera, Normal;
    public Vector2 TexCoord;
    public float Depth;
}

/// <summary>
///     Shader than performs BlinnPhong lighting to a given triangle
/// </summary>
public class LightingShader : IShader<Mesh.Vertex, LightingShaderVarying, LightingShaderInterpolatorOutput>
{
    public ISampler2D Texture;
    public Matrix4 ProjViewMatrix = Matrix4.Identity();
    public Matrix4 ModelMatrix = Matrix4.Identity();
    public Matrix4 NormalMatrix = Matrix4.Identity();
    public Vector3 CameraPosition;
    public List<PointLight> PointLights = new();
    public Material Material = new();
    public DepthBuffer DepthBuffer;

    public (Vector4, LightingShaderVarying) ShadeVertex(Mesh.Vertex inVertex)
    {
        Vector4 WorldSpacePosition = ModelMatrix * new Vector4(inVertex.Position);
        return (ProjViewMatrix * WorldSpacePosition, new LightingShaderVarying(
            WorldSpacePosition.xyz,
            CameraPosition - WorldSpacePosition.xyz,
            (NormalMatrix * new Vector4(inVertex.Normal, 0.0f)).xyz,
            inVertex.TexCoord));
    }

    public LightingShaderInterpolatorOutput Interpolate(InterpolatorContext context, Triangle<LightingShaderVarying> triangle)
    {
        // Interpolate vertex attributes for a triangle for a point on the triangle
        LightingShaderInterpolatorOutput output = new();

        output.WorldSpacePosition = MathUtil.TriMix(
         triangle[0].Attribs.WorldSpacePosition,
         triangle[1].Attribs.WorldSpacePosition,
         triangle[2].Attribs.WorldSpacePosition,
         context.Alpha, context.Beta, context.Gamma);
        output.WorldSpacePosition /= context.W;

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

        output.TexCoord = MathUtil.TriMix(
            triangle[0].Attribs.TexCoord,
            triangle[1].Attribs.TexCoord,
            triangle[2].Attribs.TexCoord,
            context.Alpha, context.Beta, context.Gamma);
        output.TexCoord /= context.W;

        return output;
    }

    public Vector4? ShadePixel(PixelShaderContext context, LightingShaderInterpolatorOutput interpolatedAttribs)
    {
        float depth = interpolatedAttribs.Depth;

        // Depth test
        if (depth < DepthBuffer.GetDepth(context.bufferX, context.bufferY))
        {
            DepthBuffer.SetDepth(context.bufferX, context.bufferY, depth);

            // Perform blinn-phong lighting 
            Vector3 toCamera = interpolatedAttribs.ToCamera.Normalized;
            Vector3 normal = interpolatedAttribs.Normal.Normalized;
            Vector3 albedo = Texture.Sample(interpolatedAttribs.TexCoord);

            Vector3 totalLight = new Vector3();

            foreach (PointLight light in PointLights)
            {
                Vector3 toLight = (light.Position - interpolatedAttribs.WorldSpacePosition).Normalized;

                float diffuse = MathF.Max(Vector3.Dot(toLight, normal), 0.0f);

                Vector3 halfVector = (toLight + toCamera).Normalized;
                float specular = MathF.Pow(MathF.Max(Vector3.Dot(halfVector, normal), 0.0f), Material.SpecularPower) * Material.SpecularStrength;

                totalLight += light.Colour * (diffuse + specular);
            }

            // Tone map output to SDR
            Vector3 hdrOut = albedo * totalLight;

            return new Vector4(hdrOut / (hdrOut + new Vector3(1.0f, 1.0f, 1.0f)), 1.0f);
        }

        return null;
    }

    public LightingShader(ISampler2D sampler, DepthBuffer depthBuffer)
    {
        Texture = sampler;
        DepthBuffer = depthBuffer;
        CameraPosition = new();

        PointLight light = new()
        {
            Position = new Vector3(-10, 10, 3),
            Colour = new Vector3(1.9f, 1.8f, 1.9f)
        };
        PointLights.Add(light);
    }
}
