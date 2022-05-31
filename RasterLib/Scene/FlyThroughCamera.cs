using RasterLib.LinearMath;
using System;

namespace RasterLib.Scene;

/// <summary>
///     Implementation of a Camera that can be moved and oriented in 3D 
/// </summary>
public class FlyThroughCamera : Camera
{
    private static readonly Vector3 WorldUp = new(0, 1, 0);
    private static readonly float MaxPitch = MathUtil.Radians(89.9f);
    private static readonly float MinPitch = -MaxPitch;
    private static readonly float MovementSpeed = 2;
    private static readonly float Sensitivity = 0.25f;

    private Vector3 m_position = new();
    private float m_yaw = 0.0f;
    private float m_pitch = 0.0f;

    private Vector3 m_front = new();
    private Vector3 m_right = new();
    private Vector3 m_up = new();

    public Vector3 GetPosition()
    {
        return m_position;
    }

    public FlyThroughCamera()
    {
        UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        m_front = new Vector3(
            MathF.Sin(m_yaw) * MathF.Cos(m_pitch),
            MathF.Sin(m_pitch),
            MathF.Cos(m_yaw) * MathF.Cos(m_pitch)
        );

        m_right = Vector3.Cross(WorldUp, m_front).Normalized;
        m_up = Vector3.Cross(m_right, m_front);
    }

    public Matrix4 getViewMatrix()
    {
        return Matrix4.LookAt(m_position, m_position + m_front, WorldUp);
    }

    public void Move(Vector3 moveDir)
    {
        m_position += m_right * moveDir.x * MovementSpeed;
        m_position += m_up * moveDir.y * MovementSpeed;
        m_position += m_front * moveDir.z * MovementSpeed;
    }

    public void Rotate(float yawChange, float pitchChange)
    {
        m_yaw += yawChange * Sensitivity;
        m_pitch += pitchChange * Sensitivity;
        m_pitch = MathUtil.Clamp(m_pitch, MinPitch, MaxPitch);

        UpdateOrientation();
    }
}