using System;
using RasterLib.LinearMath;

namespace RasterLib.Core;

public class FrameBuffer
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public byte[] Data { get; private set; }

    public FrameBuffer(int width, int height)
    {
        Width = width;
        Height = height;
        Data = new byte[Width * Height * 4];
    }

    public void SetPixel(int x, int y, byte r, byte g, byte b)
    {
        int index = (y * Width + x) * 4;
        Data[index] = r;
        Data[index + 1] = g;
        Data[index + 2] = b;
        Data[index + 3] = 0xFF;
    }

    public void SetPixel(int x, int y, Vector3 colour)
    {
        SetPixel(x, y, (byte)(colour.x * 255.0), (byte)(colour.y * 255.0), (byte)(colour.z * 255.0));
    }

    public void Clear()
    {
        Array.Fill(Data, (byte)0x0);
    }

    public void ClearWithColour(Vector4 colour)
    {
        ClearWithColour((byte)(colour.x * 255.0f), (byte)(colour.y * 255.0f), (byte)(colour.z * 255.0f), (byte)(colour.w * 255.0f));
    }

    public void ClearWithColour(byte r, byte g, byte b, byte a)
    {
        for (int i = 0; i < Width * Height * 4; i += 4)
        {
            Data[i] = r;
            Data[i + 1] = g;
            Data[i + 2] = b;
            Data[i + 3] = a;
        }
    }
}