
using BoidsProject.Util;
using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BoidsProject;

public static class Parameters
{
    public const float OneTurn = 2f * (float) Math.PI;
    public static readonly Random Rnd = new Random();
    public static readonly Vector2 HalfPointVector = new(0.5f, 0.5f);
    public static readonly Vector2 FlipV = new Vector2(1, -1);


    public static float Lerp = 0.1f;

    public static Vector2 BoundRadius = new(50, 50);
    public static float BoundAvoidanceWeight = 1f;
    public static float AvoidanceRadius = 30f; // .Squared()
    public static float DetectRadius = 100f;

    public static float Cohesion = 1f;
    public static float Alignment = 2f;
    public static float Separation = 10f;
    public static float MinimumSpeed = 10f;
    public static float MaximumSpeed = 100f;

    public static float DetectAngle = OneTurn * 0.8f;
    public static float ObstacleAvoidanceWeight = 0.1f;

    public static IEnumerable<FieldInfo> Fields => typeof(Parameters).GetFields().Where(f => !f.IsInitOnly);

    /// <summary>
    /// [0, 1]
    /// </summary>
    public static Vector2 RandomVector2() => new(Rnd.NextSingle(), Rnd.NextSingle());
    /// <summary>
    /// [-0.5, +0.5]
    /// </summary>
    public static Vector2 RandomVector2Centered() => RandomVector2() - HalfPointVector;
    /// <summary>
    /// [-Radius, +Radius]
    /// </summary>
    public static Vector2 RandomPosition() => RandomVector2Centered() * 2 * BoundRadius;
    public static float RandomSpeed()
    {
        return Rnd.NextSingle() * (MaximumSpeed - MinimumSpeed) + MinimumSpeed;
    }

}
