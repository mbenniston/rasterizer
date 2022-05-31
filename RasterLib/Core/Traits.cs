using RasterLib.LinearMath;

namespace RasterLib.Core;

public interface IInterpolatable<T>
{
    T Interpolate(T interpolateWith, float t);
}

public interface IPerspectiveDivisable
{
    void PerspectiveDivide(float z);
}

public struct InterpolatorContext
{
    public float Alpha, Beta, Gamma, W;
};

public struct PixelShaderContext
{
    public int bufferX, bufferY;
}

/// <summary>
///     Describes a shader that outputs a homogenous coordinate and some custom output 
///     given a vertex input.
/// </summary>
public delegate (Vector4, VSout) VertexShader<VSIn, VSout>(VSIn inVertex);

/// <summary>
///     Describes an interpolation function over a triangle
/// </summary>
public delegate InterpolatorOutput Interpolator<VertexAttrib, InterpolatorOutput>(
    InterpolatorContext context, Triangle<VertexAttrib> triangle) where VertexAttrib : IInterpolatable<VertexAttrib>;

/// <summary>
///     Describes a shader that has an input and may or may not output a colour
/// </summary>
public delegate Vector4? PixelShader<InterpolatorOutput>(PixelShaderContext context, InterpolatorOutput interpolatedAttribs);