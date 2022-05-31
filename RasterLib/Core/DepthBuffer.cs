using System;

namespace RasterLib.Core;

public class DepthBuffer
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float[] Data { get; private set; }

    public DepthBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        Data = new float[Width * Height];
    }

    public void SetDepth(int x, int y, float depth)
    {
        Data[y * Width + x] = depth;
    }

    public float GetDepth(int x, int y)
    {
        return Data[y * Width + x];
    }

    public void Clear()
    {
        Array.Fill(Data, float.PositiveInfinity);
    }
}