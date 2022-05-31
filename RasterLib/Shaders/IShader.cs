using RasterLib.Core;
using RasterLib.LinearMath;

namespace RasterLib.Shaders;

/// <summary>
///     Describes an interface that can Shade vertices and Pixels as well as provide an 
///     interpolation function of a triangle. 
/// </summary>
/// <typeparam name="VSIn">The input to the vertex shader.</typeparam>
/// <typeparam name="VSout">The output from the vertex shader. Must be interpolatable.</typeparam>
/// <typeparam name="InterpolatorOutput">The output of the interpolation function.</typeparam>
public interface IShader<VSIn, VSout, InterpolatorOutput> where VSout : IInterpolatable<VSout>
{
    (Vector4, VSout) ShadeVertex(VSIn inVertex);
    InterpolatorOutput Interpolate(InterpolatorContext context, Triangle<VSout> triangle);
    Vector4? ShadePixel(PixelShaderContext context, InterpolatorOutput interpolatedAttribs);
}
