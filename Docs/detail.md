# Project Overview

To complete this project I split it into two parts, the Rasterization library ([RasterLib](../RasterLib/)) and the Demo program ([RasterDemo](../RasterDemo/)).
This RasterLib contains all the classes and functions required to perform rasterization, execute shaders and load obj models. 
RasterDemo contained a demo scene of a horse statue rendered with the Blinn-Phong shader. 

## RasterLib

This library contains all functions required to perform rasterization:
- Vertex shading
- Triangle attribute Interpolation
- Clipping
- Pixel Shading

It also contains other helpful primitives for rendering, such as framebuffers, depthbuffers and some basic shaders.
However it doesn't contain any specifics of what should be done with the output of the rasterizer.

## RasterDemo

Contains a demonstation usage of the RasterLib by rendering a collection of Obj Models and displaing the output to the screen using the SFML library. 

## Hightlighed Files & Folders

- [RasterLib](../RasterLib/)
    - [Core](../RasterLib/Core/): basic rastization primitives
        - [Triangle](../RasterLib/Core/Triangle.cs): used to specify a triangle with 3 vertices that can be interpolated over
        - [Sampler](../RasterLib/Core/Sampler.cs): describe an interface that can be sampled at a certain position for textures
        - [FrameBuffer](../RasterLib/Core/FrameBuffer.cs), [DepthBuffer](../RasterLib/Core/DepthBuffer.cs.cs) (used to store colour and depth images respectively
        - [Clipping](../RasterLib/Core/Clipping.cs): used to cut triangles into 1 or more other triangles using a plane
        - [Pipeline](../RasterLib/Core/Pipeline.cs): contains triangle rasterization routines which also applies shaders to the triangle
    - [LinearMath](../RasterLib/LinearMath/): contains linear algebra constructs such as Vectors adn Matrices
    - [Renderers](../RasterLib/Renderers/)
        - [LineRenderer](../RasterLib/Renderers/LineRenderer.cs): contains functions for drawing lines in 3D
        - [SkyboxRenderer](../RasterLib/Renderers/SkyboxRenderer.cs): used to draw a skybox using an equirectangular image
    - [Shaders](../RasterLib/Shaders/)
        - [IShader](../RasterLib/Shaders/IShader.cs): defines the interface for a shader to follow
        - [LightingShader](../RasterLib/Shaders/LightingShader.cs)
        - [ReflectionShader](../RasterLib/Shaders/ReflectionShader.cs)
        - [SkyboxShader](../RasterLib/Shaders/SkyboxShader.cs)
    - [Scene](../RasterLib/Scene/)
        - [Camera](../RasterLib/Scene/Camera.cs): defines the interface for a camera in 3D space
        - [Flythrough camera](../RasterLib/Scene/FlyThroughCamera.cs): implements a camera which can be oriented and moved
        - [ObjLoader](../RasterLib/Scene/ObjLoader.cs): implements a camera which can be oriented and moved
- [RasterDemo](../RasterDemo/)
    - [Program](../RasterDemo/Program.cs): contains the code for a demostration of RasterLib