using RasterLib.LinearMath;
using RasterLib.Shaders;
using System;

namespace RasterLib.Core;

public class Pipeline
{
    public struct PipeLineOptions
    {
        public bool CullBackfaces = true;
        public bool ClipViewport = true;

        public PipeLineOptions()
        {
        }
    }

    /// <summary>
    ///     Draws a triangle with the given shading functions
    /// </summary>
    public static int DrawTriangle<VertexData, VSout, PSin>(
       VertexShader<VertexData, VSout> vertexShader,
       Interpolator<VSout, PSin> interpolator,
       PixelShader<PSin> pixelShader,
       VertexData v1, VertexData v2, VertexData v3,
       FrameBuffer buffer,
       PipeLineOptions options) where VSout : IInterpolatable<VSout>, IPerspectiveDivisable
    {
        int trisDrawn = 0;

        // Apply vertex shader to vertices
        Triangle<VSout> triangle = new(
            vertexShader(v1),
            vertexShader(v2),
            vertexShader(v3)
        );

        // Calculate signed area of triangle
        Vector2 vA = triangle.VertexA.Position.xy / triangle.VertexA.Position.w;
        Vector2 vB = triangle.VertexB.Position.xy / triangle.VertexB.Position.w;
        Vector2 vC = triangle.VertexC.Position.xy / triangle.VertexC.Position.w;

        float det = (vA.x - vB.x) * -(vA.y - vC.y) + (vC.x - vA.x) * (vB.y - vA.y);

        det = triangle.VertexA.Position.w >= 0 ? -det : det;
        det = triangle.VertexB.Position.w >= 0 ? -det : det;
        det = triangle.VertexC.Position.w >= 0 ? -det : det;

        // Backface cull 
        if (det <= 0 && options.CullBackfaces)
        {
            return 0;
        }

        if (options.ClipViewport)
        {
            // Clip against all 6 viewport planes and then rasterize triangle to the framebuffer
            Clipping.ClipHomogenousAllMap(triangle,
                    clippedTri =>
                    {
                        Util.PerspectiveDivide(ref clippedTri);
                        clippedTri.ConvertFromNDC(buffer.Width, buffer.Height);
                        RasterizeTriangle(clippedTri, interpolator, pixelShader, buffer);
                        trisDrawn++;
                    });
        }
        else
        {
            Util.PerspectiveDivide(ref triangle);

            triangle.ConvertFromNDC(buffer.Width, buffer.Height);
            RasterizeTriangle(triangle, interpolator, pixelShader, buffer);
            trisDrawn++;
        }

        return trisDrawn;
    }

    /// <summary>
    ///     Draw a triangle to a framebuffer using the pixel shader to get the colour 
    ///     of each pixel to be drawn.
    /// </summary>
    public static void RasterizeTriangle<VSOut, PSIn>(
        Triangle<VSOut> triangle,
        Interpolator<VSOut, PSIn> interpolator, PixelShader<PSIn> pixelShader,
        FrameBuffer buffer) where VSOut : IInterpolatable<VSOut>
    {
        (Vector2 minBound, Vector2 maxBound) = triangle.CalcXYBounds();

        // Clamp bounds to viewport
        int minBoundX = Math.Max((int)minBound.x, 0);
        int minBoundY = Math.Max((int)minBound.y, 0);
        int maxBoundX = Math.Min((int)maxBound.x, buffer.Width - 1);
        int maxBoundY = Math.Min((int)maxBound.y, buffer.Height - 1);

        // Iterate over bound
        for (int j = minBoundY; j <= maxBoundY; j++)
        {
            for (int i = minBoundX; i <= maxBoundX; i++)
            {
                var bCoords = MathUtil.Barycentric(
                    triangle.VertexA.Position.xy,
                    triangle.VertexB.Position.xy,
                    triangle.VertexC.Position.xy,
                    new Vector2(i + 0.5f, j + 0.5f));

                if (bCoords != null)
                {
                    (double alpha, double beta, double gamma) = bCoords.Value;

                    float w = triangle.VertexA.Position.w * (float)alpha +
                        triangle.VertexB.Position.w * (float)beta +
                        triangle.VertexC.Position.w * (float)gamma;

                    InterpolatorContext interpolatorContext = new()
                    {
                        W = w,
                        Alpha = (float)alpha,
                        Beta = (float)beta,
                        Gamma = (float)gamma
                    };

                    // Interpolate vertex attributes for this point
                    var interpolated = interpolator(interpolatorContext, triangle);

                    PixelShaderContext pixelShaderContext = new()
                    {
                        bufferX = i,
                        bufferY = j
                    };

                    // Shade the pixel at this point
                    Vector4? psOut = pixelShader(pixelShaderContext, interpolated);

                    if (psOut != null)
                    {
                        buffer.SetPixel(i, j, psOut.Value.xyz);
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Uses a IShader implementation to draw a triangle 
    /// </summary>
    public static int DrawTriangle<VertexData, VSout, PSin>(
        IShader<VertexData, VSout, PSin> shader,
        VertexData v1, VertexData v2, VertexData v3,
        FrameBuffer buffer,
        PipeLineOptions options) where VSout : IInterpolatable<VSout>, IPerspectiveDivisable
    {
        return DrawTriangle(shader.ShadeVertex, shader.Interpolate, shader.ShadePixel, v1, v2, v3, buffer, options);
    }
}
