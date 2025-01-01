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
    public static float Abs(this float x) => Math.Abs(x);
}
