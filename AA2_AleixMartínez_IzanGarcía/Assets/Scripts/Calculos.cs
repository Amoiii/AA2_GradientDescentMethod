using UnityEngine; 
using System;      


[System.Serializable]
public struct MyVector3
{
    public float x, y, z;

    public MyVector3(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // Operaciones básicas
    public static MyVector3 operator +(MyVector3 a, MyVector3 b) => new MyVector3(a.x + b.x, a.y + b.y, a.z + b.z);
    public static MyVector3 operator -(MyVector3 a, MyVector3 b) => new MyVector3(a.x - b.x, a.y - b.y, a.z - b.z);
    public static MyVector3 operator *(MyVector3 a, float d) => new MyVector3(a.x * d, a.y * d, a.z * d);
    public static MyVector3 operator *(float d, MyVector3 a) => new MyVector3(a.x * d, a.y * d, a.z * d);

    // Cálculo de Distancia (Teorema de Pitágoras 3D)
    public static float Distance(MyVector3 a, MyVector3 b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        float dz = a.z - b.z;
        return (float)System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    // Convertidores
    public Vector3 ToUnity() => new Vector3(x, y, z);
    public static MyVector3 FromUnity(Vector3 v) => new MyVector3(v.x, v.y, v.z);
}

// cuatern

public struct MyQuaternion
{
    public float x, y, z, w;

    public MyQuaternion(float x, float y, float z, float w)
    {
        this.x = x; this.y = y; this.z = z; this.w = w;
    }

    // de Euler (Grados) a Cuaternión q = cos(a/2) + sin(a/2) * eje

    public static MyQuaternion Euler(float pitch, float yaw, float roll)
    {
        // Convertir a radianes
        float p = pitch * MyMath.Deg2Rad * 0.5f;
        float y = yaw * MyMath.Deg2Rad * 0.5f;
        float r = roll * MyMath.Deg2Rad * 0.5f;

        float sinP = (float)System.Math.Sin(p);
        float cosP = (float)System.Math.Cos(p);
        float sinY = (float)System.Math.Sin(y);
        float cosY = (float)System.Math.Cos(y);
        float sinR = (float)System.Math.Sin(r);
        float cosR = (float)System.Math.Cos(r);

        MyQuaternion q;
        q.x = cosY * sinP * cosR + sinY * cosP * sinR;
        q.y = sinY * cosP * cosR - cosY * sinP * sinR;
        q.z = cosY * cosP * sinR - sinY * sinP * cosR;
        q.w = cosY * cosP * cosR + sinY * sinP * sinR;

        return q;
    }

    public Quaternion ToUnity() => new Quaternion(x, y, z, w);
}


public static class MyMath
{
    public const float PI = 3.14159265359f;
    public const float Deg2Rad = PI / 180f;

    public static float Abs(float val) => (float)System.Math.Abs(val);

    public static float Pow(float bases, float exp) => (float)System.Math.Pow(bases, exp);

    // Interpolación lineal de ángulos 
    public static float LerpAngle(float a, float b, float t)
    {
        float diff = b - a;
        // Ajustar la diferencia para ir por el camino corto (-180 a 180)
        while (diff > 180f) diff -= 360f;
        while (diff < -180f) diff += 360f;
        return a + diff * t;
    }

    // Función Clamp 
    public static float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
}