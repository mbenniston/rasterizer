using RasterLib.LinearMath;

namespace RasterLib.Core;

/// <summary>
///     Describes an object that can be sampled at a 2D location
/// </summary>
public interface ISampler2D
{
    Vector3 Sample(Vector2 texCoord);
}

/// <summary>
///     Describes an object that can be sampled at a 3D location (or direction)
/// </summary>
public interface ISampler3D
{
    Vector3 Sample(Vector3 direction);
}

