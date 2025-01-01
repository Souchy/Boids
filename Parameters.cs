
using BoidsProject.Util;
using Godot;
using System;

namespace BoidsProject;

public static class Parameters
{
    public static readonly Random Rnd = new Random();
    public const float OneTurn = 2f * (float) Math.PI;
    public static readonly Vector2 HalfPointVector = new(0.5f, 0.5f);

    public static Vector2 BoundRadius = new(50, 50);
    public static float BoundAvoidanceWeight = 1f;

    public static float Cohesion = 1f;
    public static float Alignment = 1f;
    public static float Separation = 1f;

    public static float AvoidanceRadiusSquared = 1f.Squared();
    public static float DetectRadiusSquared = 4f.Squared();
    public static float DetectAngle = OneTurn * 0.8f;
    public static float MinimumSpeed = 1f;
    public static float MaximumSpeed = 2f;
    public static float Lerp = 0.1f;
    public static float ObstacleAvoidanceWeight = 0.1f;

    /// <summary>
    /// [0, 1]
    /// </summary>
    /// <returns></returns>
    public static Vector2 RandomVector2() => new(Rnd.NextSingle(), Rnd.NextSingle());
    /// <summary>
    /// [-0.5, +0.5]
    /// </summary>
    public static Vector2 RandomVector2Centered() => RandomVector2() - HalfPointVector;
    /// <summary>
    /// [-Radius, +Radius]
    /// </summary>
    public static Vector2 RandomPosition() => RandomVector2Centered() * BoundRadius * 2;
    public static float RandomSpeed()
    {
        return Rnd.NextSingle() * (MaximumSpeed - MinimumSpeed) + MinimumSpeed;
    }

}
