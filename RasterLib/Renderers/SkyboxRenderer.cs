using RasterLib.Core;
using RasterLib.LinearMath;
using RasterLib.Scene;
using RasterLib.Shaders;

namespace RasterLib.Renderers;

/// <summary>
///     Renders a cube in the scene with a given texture to simulate a skybox
/// </summary>
public class SkyboxRenderer
{
    private ISampler3D m_sampler;
    private SkyboxShader m_shader;
    private Mesh m_cube;

    public void SetProjectionMatrix(Matrix4 proj)
    {
        m_shader.ProjMatrix = proj;
    }

    public void SetViewMatrix(Matrix4 view)
    {
        m_shader.ViewMatrix = view.Clone();
        m_shader.ViewMatrix[3, 0] = 0;
        m_shader.ViewMatrix[3, 1] = 0;
        m_shader.ViewMatrix[3, 2] = 0;
    }

    public void Draw(FrameBuffer drawBuffer)
    {
        Pipeline.PipeLineOptions options = new()
        {
            CullBackfaces = false
        };

        m_cube.Draw(m_shader, drawBuffer, options);
    }

    public SkyboxRenderer(ISampler3D skyboxSampler, Matrix4 proj)
    {
        m_sampler = skyboxSampler;
        m_shader = new SkyboxShader(m_sampler, proj);
        m_cube = ObjLoader.Load("./RasterDemo/Resources/cube.obj");
    }
}
