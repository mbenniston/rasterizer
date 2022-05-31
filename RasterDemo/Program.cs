using System;
using SFML.Graphics;
using SFML.Window;
using SFML.System;
using RasterLib.Core;
using RasterLib.Scene;
using RasterLib.Shaders;
using RasterLib.Renderers;
using RasterLib.LinearMath;

namespace RasterDemo
{
    class Program
    {
        private const int FrameBufferWidth = 1920 / 4;
        private const int FrameBufferHeight = 1080 / 4;
        private const float Fov = 80.0f;
        private const float NearPlaneDist = 0.1f;
        private const float FarPlaneDist = 100.0f;

        static void Main(string[] args)
        {
            RenderWindow window = new(new VideoMode(1920, 1080), "Software Rasterizer");

            // Setup mouse capture
            window.SetVerticalSyncEnabled(true);
            window.SetMouseCursorGrabbed(true);
            window.SetMouseCursorVisible(false);

            // Close the window when the X is pressed
            window.Closed += (sender, args) =>
            {
                window.Close();
            };

            // Setup framebuffer and depthbuffer
            Texture texture = new(FrameBufferWidth, FrameBufferHeight)
            {
                Smooth = false
            };

            Sprite sprite = new(texture);

            FrameBuffer buffer = new(FrameBufferWidth, FrameBufferHeight);
            DepthBuffer depthBuffer = new(FrameBufferWidth, FrameBufferHeight);

            // Load mesh and textures
            Mesh mesh = ObjLoader.Load("./RasterDemo/Resources/horse_statue_01_4k.obj");

            EnvironmentSampler environmentSampler = new(new Image("./RasterDemo/Resources/env.jpg"));
            ImageSampler textureSampler = new(new Image("./RasterDemo/Resources/horse_statue_01_diff_4k.bmp"));

            FlyThroughCamera camera = new();

            // Callback for fps mouse input 
            window.MouseMoved += (sender, args) =>
            {
                Vector2 mousePosition = new(args.X, args.Y);

                Vector2 halfWindowSize = new((float)window.Size.X / 2, (float)window.Size.Y / 2);
                Vector2 d = (mousePosition - halfWindowSize) / halfWindowSize;

                camera.Rotate(-d.x, d.y);
                Mouse.SetPosition(new Vector2i((int)window.Size.X / 2, (int)window.Size.Y / 2), window);
            };

            // Callback for toggling winding normals view with the 'T' key
            bool showNormals = false;
            window.KeyPressed += (sender, args) =>
            {
                if (args.Code == Keyboard.Key.Escape)
                {
                    window.Close();
                }
                else if (args.Code == Keyboard.Key.T)
                {
                    showNormals = !showNormals;
                }
            };

            Matrix4 projMat = Matrix4.Perspective(Fov * MathF.PI / 180.0f, FrameBufferWidth / (float)FrameBufferHeight, NearPlaneDist, FarPlaneDist);

            // Construct Renderers and shaders
            ReflectionShader reflectionShader = new ReflectionShader(environmentSampler, depthBuffer);
            LightingShader lightingShader = new LightingShader(textureSampler, depthBuffer);
            SkyboxRenderer skyboxRenderer = new SkyboxRenderer(environmentSampler, projMat);

            Clock deltaClock = new Clock();
            Clock clock = new Clock();

            while (window.IsOpen)
            {
                // Calculate last frames delta time
                float delta = deltaClock.ElapsedTime.AsSeconds();
                deltaClock.Restart();
                int trisDrawn = 0;

                // Update camera controller input 
                Vector3 MoveDir = new();
                MoveDir.x += Keyboard.IsKeyPressed(Keyboard.Key.A) ? -1 : 0;
                MoveDir.x += Keyboard.IsKeyPressed(Keyboard.Key.D) ? 1 : 0;
                MoveDir.y += Keyboard.IsKeyPressed(Keyboard.Key.LShift) ? 1 : 0;
                MoveDir.y += Keyboard.IsKeyPressed(Keyboard.Key.Space) ? -1 : 0;
                MoveDir.z += Keyboard.IsKeyPressed(Keyboard.Key.S) ? 1 : 0;
                MoveDir.z += Keyboard.IsKeyPressed(Keyboard.Key.W) ? -1 : 0;
                camera.Move(MoveDir * delta);

                // Start rendering 
                buffer.ClearWithColour(55, 55, 55, 255);
                depthBuffer.Clear();

                // Calculate this frames view and model matricies
                Matrix4 viewMat = camera.getViewMatrix();
                Matrix4 modelMat = Matrix4.RotationY(clock.ElapsedTime.AsSeconds()) * Matrix4.Scale(new Vector3(10, 10, 10));
                Matrix4 normalMat = Matrix4.Invserse(Matrix4.Transpose(modelMat));

                // Draw skybox
                skyboxRenderer.SetViewMatrix(viewMat);
                // skyboxRenderer.Draw(buffer);

                // Setup shader
                lightingShader.ProjViewMatrix = projMat * viewMat;
                lightingShader.ModelMatrix = modelMat;
                lightingShader.NormalMatrix = normalMat;
                lightingShader.CameraPosition = camera.GetPosition();

                Pipeline.PipeLineOptions options = new();

                trisDrawn += mesh.Draw(lightingShader, buffer, options);

                // Draw winding normals if requested
                if (showNormals)
                {
                    float lineLength = 0.005f;
                    Matrix4 mvp = projMat * viewMat * modelMat;

                    for (int i = 0; i < mesh.Vertices.Length; i += 3)
                    {
                        Vector3 center = (mesh.Vertices[i].Position + mesh.Vertices[i + 1].Position + mesh.Vertices[i + 2].Position) / 3;
                        Vector3 left = (mesh.Vertices[i + 1].Position - mesh.Vertices[i].Position).Normalized;
                        Vector3 right = (mesh.Vertices[i + 2].Position - mesh.Vertices[i].Position).Normalized;
                        Vector3 normal = Vector3.Cross(left, right).Normalized;

                        LineRenderer.DrawTranformedLine(mvp, center, center + normal * lineLength, new Vector3(1, 0, 0), buffer, depthBuffer);
                    }
                }

                // Update window
                texture.Update(buffer.Data);
                window.Clear();

                // Scale internal buffer to window size
                window.SetView(new View(
                    new Vector2f(window.Size.X * 0.5f, window.Size.Y * 0.5f),
                    new Vector2f(window.Size.X, window.Size.Y)));

                float xS = window.Size.X / (float)sprite.Texture.Size.X;
                float yS = window.Size.Y / (float)sprite.Texture.Size.Y;

                float scale = MathF.Min(xS, yS);

                // Center using aspect ratio
                float xLeftOver = window.Size.X - (scale * sprite.Texture.Size.X);
                float yLeftOver = window.Size.Y - (scale * sprite.Texture.Size.Y);

                sprite.Scale = new Vector2f(scale, scale);
                sprite.Position = new Vector2f(xLeftOver / 2.0f, yLeftOver / 2.0f);

                window.Draw(sprite);
                window.Display();
                window.DispatchEvents();

                window.SetTitle($"{deltaClock.ElapsedTime.AsMilliseconds()}MS Tris: {trisDrawn}");
            }
        }

        struct ImageSampler : ISampler2D
        {
            public Image image;

            public Vector3 Sample(Vector2 texCoord)
            {
                int pixelCoordX = (int)MathF.Round(texCoord.x * image.Size.X);
                int pixelCoordY = (int)MathF.Round((1.0f - texCoord.y) * image.Size.Y);

                // Texture clamping
                pixelCoordX = MathUtil.Clamp(pixelCoordX, 0, (int)image.Size.X - 1);
                pixelCoordY = MathUtil.Clamp(pixelCoordY, 0, (int)image.Size.Y - 1);

                Color col = image.GetPixel((uint)pixelCoordX, (uint)pixelCoordY);
                return new Vector3(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f);
            }

            public ImageSampler(Image image)
            {
                this.image = image;
            }
        }

        struct EnvironmentSampler : ISampler3D
        {
            public Image image;

            public Vector3 Sample(Vector3 direction)
            {
                float alpha = MathF.Asin(direction.y);
                float theta = MathF.Atan2(direction.z, direction.x);

                float xCoord = (((theta / MathF.PI) + 1.0f) * 0.5f);
                float yCoord = (((alpha / (0.5f * MathF.PI)) + 1.0f) * 0.5f);

                int pixelCoordX = (int)MathF.Round(xCoord * image.Size.X);
                int pixelCoordY = (int)MathF.Round(yCoord * image.Size.Y);

                // Repeat texture clamping
                pixelCoordX %= (int)image.Size.X - 1;
                pixelCoordY %= (int)image.Size.Y - 1;

                if (pixelCoordX < 0)
                {
                    pixelCoordX += (int)image.Size.X - 1;
                }
                if (pixelCoordY < 0)
                {
                    pixelCoordY += (int)image.Size.Y - 1;
                }

                pixelCoordY = ((int)image.Size.Y - 1) - pixelCoordY;


                Color col = image.GetPixel((uint)pixelCoordX, (uint)pixelCoordY);
                return new Vector3(col.R / 255.0f, col.G / 255.0f, col.B / 255.0f);
            }

            public EnvironmentSampler(Image image)
            {
                this.image = image;
            }
        }
    }
}
