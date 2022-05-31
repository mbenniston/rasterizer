using RasterLib.LinearMath;

namespace RasterLib.Scene;

public interface Camera
{
    Matrix4 getViewMatrix();
}