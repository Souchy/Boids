using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsProject.Util;
public static class Extensions
{
    public static float ManhattanLength(this Vector2 vector)
    {
        return vector.X.Abs() + vector.Y.Abs();
    }
    public static float ManhattanLength(this Vector3 vector)
    {
        return vector.X.Abs() + vector.Y.Abs() + vector.Z.Abs();
    }
    public static float Abs(this float f) => Math.Abs(f);
    public static float Squared(this float f) => f * f;

    public static float LerpTo(this float a, float b, float t)
    {
        return a * (1 - t) + b * t;
    }
    public static Vector2 LerpTo(this Vector2 a, Vector2 b, float t)
    {
        return a * (1 - t) + b * t;
    }
    public static Vector3 LerpTo(this Vector3 a, Vector3 b, float t)
    {
        return a * (1 - t) + b * t;
    }
    public static long CurrentTimeMillis(this DateTime time)
    {
        return time.Ticks / TimeSpan.TicksPerMillisecond;
    }
    public static long CurrentTimeSecond(this DateTime time)
    {
        return time.Ticks / TimeSpan.TicksPerSecond;
    }
}
