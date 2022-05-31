using System;

namespace RasterLib.LinearMath;

public struct Matrix4
{
    private readonly float[] m_mat = new float[16];

    public float this[int index]
    {
        get { return m_mat[index]; }
        set { m_mat[index] = value; }
    }

    public float this[int col, int row]
    {
        get { return m_mat[row * 4 + col]; }
        set { m_mat[row * 4 + col] = value; }
    }

    public Matrix4()
    {
    }

    public Matrix4(float a0, float a1, float a2, float a3,
                   float a4, float a5, float a6, float a7,
                   float a8, float a9, float a10, float a11,
                   float a12, float a13, float a14, float a15)
    {
        m_mat[0] = a0; m_mat[1] = a1; m_mat[2] = a2; m_mat[3] = a3;
        m_mat[4] = a4; m_mat[5] = a5; m_mat[6] = a6; m_mat[7] = a7;
        m_mat[8] = a8; m_mat[9] = a9; m_mat[10] = a10; m_mat[11] = a11;
        m_mat[12] = a12; m_mat[13] = a13; m_mat[14] = a14; m_mat[15] = a15;
    }

    public Matrix4 Clone()
    {
        Matrix4 matrix = new();
        Array.Copy(m_mat, matrix.m_mat, 16);
        return matrix;
    }

    public void SetZeros()
    {
        for (int i = 0; i < 16; i++)
        {
            m_mat[i] = 0.0f;
        }
    }


    public static Vector4 operator *(Matrix4 mat, Vector4 vec)
    {
        return new Vector4(
            vec.x * mat[0] + vec.y * mat[1] + vec.z * mat[2] + vec.w * mat[3],
            vec.x * mat[4] + vec.y * mat[5] + vec.z * mat[6] + vec.w * mat[7],
            vec.x * mat[8] + vec.y * mat[9] + vec.z * mat[10] + vec.w * mat[11],
            vec.x * mat[12] + vec.y * mat[13] + vec.z * mat[14] + vec.w * mat[15]
        );
    }

    public static Matrix4 operator *(Matrix4 left, Matrix4 right)
    {
        Matrix4 outMat = new();
        outMat.SetZeros();

        for (int leftCol = 0; leftCol < 4; leftCol++)
        {
            for (int rightRow = 0; rightRow < 4; rightRow++)
            {
                for (int leftRowRightCol = 0; leftRowRightCol < 4; leftRowRightCol++)
                {
                    outMat[leftCol, rightRow] += right[leftCol, leftRowRightCol] * left[leftRowRightCol, rightRow];
                }
            }
        }

        return outMat;
    }

    public static Matrix4 LookAt(Vector3 eye, Vector3 focus, Vector3 up)
    {
        Matrix4 m = new();
        Vector3 forward = (focus - eye).Normalized;
        Vector3 left = Vector3.Cross(up, forward).Normalized;
        Vector3 newup = Vector3.Cross(forward, left);

        m.m_mat[0] = left.x; m.m_mat[1] = left.y; m.m_mat[2] = left.z; m.m_mat[3] = -Vector3.Dot(left, eye);
        m.m_mat[4] = newup.x; m.m_mat[5] = newup.y; m.m_mat[6] = newup.z; m.m_mat[7] = -Vector3.Dot(newup, eye);
        m.m_mat[8] = forward.x; m.m_mat[9] = forward.y; m.m_mat[10] = forward.z; m.m_mat[11] = -Vector3.Dot(forward, eye);
        m.m_mat[12] = 0.0f; m.m_mat[13] = 0.0f; m.m_mat[14] = 0.0f; m.m_mat[15] = 1.0f;
        return m;
    }

    public static Matrix4 Scale(Vector3 scale)
    {
        Matrix4 m = new();
        m.m_mat[0] = scale.x; m.m_mat[1] = 0.0f; m.m_mat[2] = 0.0f; m.m_mat[3] = 0.0f;
        m.m_mat[4] = 0.0f; m.m_mat[5] = scale.y; m.m_mat[6] = 0.0f; m.m_mat[7] = 0.0f;
        m.m_mat[8] = 0.0f; m.m_mat[9] = 0.0f; m.m_mat[10] = scale.z; m.m_mat[11] = 0.0f;
        m.m_mat[12] = 0.0f; m.m_mat[13] = 0.0f; m.m_mat[14] = 0.0f; m.m_mat[15] = 1.0f;
        return m;
    }

    public static Matrix4 Orthographic(float left, float right, float top, float bottom, float nearZ, float farZ)
    {
        Matrix4 mat = Identity();
        mat[0, 0] = 2.0f / (right - left);
        mat[1, 1] = 2.0f / (top - bottom);
        mat[2, 2] = -2.0f / (farZ - nearZ);

        mat[3, 0] = -(right + left) / (right - left);
        mat[3, 1] = -(top + bottom) / (top - bottom);
        mat[3, 2] = -(farZ + nearZ) / (farZ - nearZ);

        return mat;
    }

    public static Matrix4 Perspective(float fov, float aspectRatio, float nearZ, float farZ)
    {
        float h = 1.0f / MathF.Tan(fov / 2.0f);
        float w = h / aspectRatio;

        Matrix4 mat = Identity();
        mat[0, 0] = w;
        mat[1, 1] = h;
        mat[2, 2] = -(farZ + nearZ) / (farZ - nearZ);
        mat[3, 3] = 0.0f;
        mat[3, 2] = -(2 * farZ * nearZ) / (farZ - nearZ);
        mat[2, 3] = -1.0f;

        return mat;
    }

    public static Matrix4 RotationX(float angle)
    {
        Matrix4 mat = Identity();

        float ca = MathF.Cos(angle);
        float sa = MathF.Sin(angle);
        mat[0, 0] = 1.0f;
        mat[1, 1] = ca;
        mat[2, 2] = ca;

        mat[2, 1] = -sa;
        mat[1, 2] = sa;

        return mat;
    }

    public static Matrix4 RotationY(float angle)
    {
        Matrix4 mat = Identity();

        float ca = MathF.Cos(angle);
        float sa = MathF.Sin(angle);
        mat[0, 0] = ca;
        mat[1, 1] = 1.0f;
        mat[2, 2] = ca;

        mat[2, 0] = sa;
        mat[0, 2] = -sa;

        return mat;
    }

    public static Matrix4 RotationZ(float angle)
    {
        Matrix4 mat = Identity();

        float ca = MathF.Cos(angle);
        float sa = MathF.Sin(angle);
        mat[0, 0] = ca;
        mat[1, 1] = ca;

        mat[1, 0] = -sa;
        mat[0, 1] = sa;

        return mat;
    }

    public static Matrix4 Identity()
    {
        Matrix4 m = new();
        m.m_mat[0] = 1.0f; m.m_mat[1] = 0.0f; m.m_mat[2] = 0.0f; m.m_mat[3] = 0.0f;
        m.m_mat[4] = 0.0f; m.m_mat[5] = 1.0f; m.m_mat[6] = 0.0f; m.m_mat[7] = 0.0f;
        m.m_mat[8] = 0.0f; m.m_mat[9] = 0.0f; m.m_mat[10] = 1.0f; m.m_mat[11] = 0.0f;
        m.m_mat[12] = 0.0f; m.m_mat[13] = 0.0f; m.m_mat[14] = 0.0f; m.m_mat[15] = 1.0f;
        return m;
    }

    public static Matrix4 Translate(Vector3 translation)
    {
        Matrix4 m = new();
        m.m_mat[0] = 1.0f; m.m_mat[1] = 0.0f; m.m_mat[2] = 0.0f; m.m_mat[3] = translation.x;
        m.m_mat[4] = 0.0f; m.m_mat[5] = 1.0f; m.m_mat[6] = 0.0f; m.m_mat[7] = translation.y;
        m.m_mat[8] = 0.0f; m.m_mat[9] = 0.0f; m.m_mat[10] = 1.0f; m.m_mat[11] = translation.z;
        m.m_mat[12] = 0.0f; m.m_mat[13] = 0.0f; m.m_mat[14] = 0.0f; m.m_mat[15] = 1.0f;
        return m;
    }

    public bool Invert()
    {
        Matrix4 inv = new();
        int i;

        inv[0] = this[5] * this[10] * this[15] -
                 this[5] * this[11] * this[14] -
                 this[9] * this[6] * this[15] +
                 this[9] * this[7] * this[14] +
                 this[13] * this[6] * this[11] -
                 this[13] * this[7] * this[10];

        inv[4] = -this[4] * this[10] * this[15] +
                  this[4] * this[11] * this[14] +
                  this[8] * this[6] * this[15] -
                  this[8] * this[7] * this[14] -
                  this[12] * this[6] * this[11] +
                  this[12] * this[7] * this[10];

        inv[8] = this[4] * this[9] * this[15] -
                 this[4] * this[11] * this[13] -
                 this[8] * this[5] * this[15] +
                 this[8] * this[7] * this[13] +
                 this[12] * this[5] * this[11] -
                 this[12] * this[7] * this[9];

        inv[12] = -this[4] * this[9] * this[14] +
                   this[4] * this[10] * this[13] +
                   this[8] * this[5] * this[14] -
                   this[8] * this[6] * this[13] -
                   this[12] * this[5] * this[10] +
                   this[12] * this[6] * this[9];

        inv[1] = -this[1] * this[10] * this[15] +
                  this[1] * this[11] * this[14] +
                  this[9] * this[2] * this[15] -
                  this[9] * this[3] * this[14] -
                  this[13] * this[2] * this[11] +
                  this[13] * this[3] * this[10];

        inv[5] = this[0] * this[10] * this[15] -
                 this[0] * this[11] * this[14] -
                 this[8] * this[2] * this[15] +
                 this[8] * this[3] * this[14] +
                 this[12] * this[2] * this[11] -
                 this[12] * this[3] * this[10];

        inv[9] = -this[0] * this[9] * this[15] +
                  this[0] * this[11] * this[13] +
                  this[8] * this[1] * this[15] -
                  this[8] * this[3] * this[13] -
                  this[12] * this[1] * this[11] +
                  this[12] * this[3] * this[9];

        inv[13] = this[0] * this[9] * this[14] -
                  this[0] * this[10] * this[13] -
                  this[8] * this[1] * this[14] +
                  this[8] * this[2] * this[13] +
                  this[12] * this[1] * this[10] -
                  this[12] * this[2] * this[9];

        inv[2] = this[1] * this[6] * this[15] -
                 this[1] * this[7] * this[14] -
                 this[5] * this[2] * this[15] +
                 this[5] * this[3] * this[14] +
                 this[13] * this[2] * this[7] -
                 this[13] * this[3] * this[6];

        inv[6] = -this[0] * this[6] * this[15] +
                  this[0] * this[7] * this[14] +
                  this[4] * this[2] * this[15] -
                  this[4] * this[3] * this[14] -
                  this[12] * this[2] * this[7] +
                  this[12] * this[3] * this[6];

        inv[10] = this[0] * this[5] * this[15] -
                  this[0] * this[7] * this[13] -
                  this[4] * this[1] * this[15] +
                  this[4] * this[3] * this[13] +
                  this[12] * this[1] * this[7] -
                  this[12] * this[3] * this[5];

        inv[14] = -this[0] * this[5] * this[14] +
                   this[0] * this[6] * this[13] +
                   this[4] * this[1] * this[14] -
                   this[4] * this[2] * this[13] -
                   this[12] * this[1] * this[6] +
                   this[12] * this[2] * this[5];

        inv[3] = -this[1] * this[6] * this[11] +
                  this[1] * this[7] * this[10] +
                  this[5] * this[2] * this[11] -
                  this[5] * this[3] * this[10] -
                  this[9] * this[2] * this[7] +
                  this[9] * this[3] * this[6];

        inv[7] = this[0] * this[6] * this[11] -
                 this[0] * this[7] * this[10] -
                 this[4] * this[2] * this[11] +
                 this[4] * this[3] * this[10] +
                 this[8] * this[2] * this[7] -
                 this[8] * this[3] * this[6];

        inv[11] = -this[0] * this[5] * this[11] +
                   this[0] * this[7] * this[9] +
                   this[4] * this[1] * this[11] -
                   this[4] * this[3] * this[9] -
                   this[8] * this[1] * this[7] +
                   this[8] * this[3] * this[5];

        inv[15] = this[0] * this[5] * this[10] -
                  this[0] * this[6] * this[9] -
                  this[4] * this[1] * this[10] +
                  this[4] * this[2] * this[9] +
                  this[8] * this[1] * this[6] -
                  this[8] * this[2] * this[5];

        float det = this[0] * inv[0] + this[1] * inv[4] + this[2] * inv[8] + this[3] * inv[12];

        if (det == 0)
        {
            return false;
        }

        for (i = 0; i < 16; i++)
            this[i] = inv[i] / det;

        return true;
    }

    public void Transpose()
    {
        Matrix4 temp = Clone();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                this[i, j] = temp[j, i];
            }
        }
    }

    public static Matrix4 Invserse(Matrix4 m)
    {
        Matrix4 mat = m.Clone();
        mat.Invert();
        return mat;
    }

    public static Matrix4 Transpose(Matrix4 mat)
    {
        Matrix4 m = mat.Clone();
        m.Transpose();
        return m;
    }

    public override string ToString()
    {
        return $"{m_mat[0]}, {m_mat[1]}, {m_mat[2]}, {m_mat[3]}\n" +
               $"{m_mat[4]}, {m_mat[5]}, {m_mat[6]}, {m_mat[7]}\n" +
               $"{m_mat[8]}, {m_mat[9]}, {m_mat[10]}, {m_mat[11]}\n" +
               $"{m_mat[12]}, {m_mat[13]}, {m_mat[14]}, {m_mat[15]}\n";
    }
}