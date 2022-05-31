using RasterLib.Core;
using RasterLib.LinearMath;
using System;
using System.Diagnostics;

namespace RasterLib.Renderers;

public static class LineRenderer
{
    /// <summary>
    ///     Use DDA algorithm to draw a solid coloured line with depth testing
    /// </summary>
    /// <param name="start">The position of the start of the line</param>
    /// <param name="end">The position of the end of the line</param>
    /// <param name="colour">The colour of the line</param>
    /// <param name="buffer">The colour buffer the line should be drawn to</param>
    /// <param name="depthBuffer">The depthbuffer that should be used for Z testing</param>
    public static void DrawLine(Vector4 start, Vector4 end, Vector3 colour, FrameBuffer buffer, DepthBuffer depthBuffer)
    {
        var clippedLine = Clipping.ClipLine(start, end);

        if (clippedLine == null)
        {
            return;
        }

        (start, end) = clippedLine.Value;

        // Perpsective division
        start /= start.w;
        end /= end.w;

        // NDC
        start.x = (start.x + 1.0f) * 0.5f * buffer.Width;
        start.y = (-start.y + 1.0f) * 0.5f * buffer.Height;

        end.x = (end.x + 1.0f) * 0.5f * buffer.Width;
        end.y = (-end.y + 1.0f) * 0.5f * buffer.Height;

        Vector3 current = start.xyz;
        Vector3 delta = end.xyz - start.xyz;

        float step = MathF.Abs(MathF.Abs(delta.x) > MathF.Abs(delta.y) ? delta.x : delta.y);

        if (step == 0)
        {
            return;
        }

        delta /= step;

        if (float.IsNaN(current.x) || float.IsNaN(current.y))
        {
            Debugger.Break();
        }

        int i = 1;
        while (i <= step)
        {
            if (current.x >= 0 && current.x < buffer.Width &&
                current.y >= 0 && current.y < buffer.Height)
            {
                if (current.z < depthBuffer.GetDepth((int)current.x, (int)current.y))
                {
                    buffer.SetPixel((int)current.x, (int)current.y, colour);
                    depthBuffer.SetDepth((int)current.x, (int)current.y, current.z);
                }
            }

            current += delta;
            i++;
        }
    }

    public static void DrawTranformedLine(Matrix4 mvpMatrix, Vector3 start, Vector3 end, Vector3 colour, FrameBuffer buffer, DepthBuffer depthBuffer)
    {
        Vector4 transformedStart = mvpMatrix * new Vector4(start, 1.0f);
        Vector4 transformedEnd = mvpMatrix * new Vector4(end, 1.0f);

        DrawLine(transformedStart, transformedEnd, colour, buffer, depthBuffer);
    }
}
