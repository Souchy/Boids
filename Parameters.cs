
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
    public static readonly float OneTurn = 2f * (float) Math.PI;
    public static readonly Random Rnd = new Random();
    public static readonly Vector2 HalfPointVector = new(0.5f, 0.5f);
    public static readonly Vector2 FlipV = new(1, -1);
    public static readonly Vector2 FlipH = new(-1, 1);

    public static readonly int MaximumBoidsPerChunk = 20;
    public static readonly int MinimumBoidsPerChunk = 16;

    // Count
    public static int Count = 1000;
    // Lerp
    public static float Lerp = 0.1f;
    // Tree factor
    public static float TreeToSpaceboundFactor = 1f;

    // Bounds
    public static Vector2 BoundRadius = new(50, 50);
    public static float BoundAvoidanceWeight = 1f;

    // Detection / Avoidance 
    public static float DetectRadius = 100f;
    public static float DetectAngle = OneTurn * 0.8f;
    public static float AvoidanceRadius = 15; //30f; // .Squared()

    // Base rules
    public static float Cohesion = 1f;
    public static float Alignment = 2f;
    public static float Separation = 3; //10f;

    // Speed
    public static float MinimumSpeed = 10f;
    public static float MaximumSpeed = 500f;

    // Obstacles
    public static float ObstacleAvoidanceWeight = 0.1f;

    // Target
    public static Vector2 Target = Vector2.Zero;
    public static float TargetWeight = 0.5f;

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
