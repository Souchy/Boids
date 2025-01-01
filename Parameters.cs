
using BoidsProject.Util;
using Godot;
using System;

namespace BoidsProject;

public static class Parameters
{
    public static readonly Random Rnd = new Random();
    public const float OneTurn = 2f * (float) Math.PI;

    public static Vector2 BoundSize = new(50, 50);
    public static Vector2 HalfPointVector = new(0.5f, 0.5f);

    public static float Cohesion = 1f;
    public static float Alignment = 1f;
    public static float Separation = 1f;

    public static float AvoidanceRadiusSquared = 1f.Squared();
    public static float DetectRadiusSquared = 4f.Squared();
    public static float DetectAngle = OneTurn * 0.8f;
    public static float MinimumSpeed = 1f;
    public static float MaximumSpeed = 2f;
    //public static float TurnSpeed = 1f;
    public static float Lerp = 0.1f;
    public static float ObstacleAvoidanceWeight = 0.1f;


    public static Vector2 RandomPosition() => RandomVector2Centered() * BoundSize;
    public static Vector2 RandomVector2() => new(Rnd.NextSingle(), Rnd.NextSingle());
    public static Vector2 RandomVector2Centered() => RandomVector2() - HalfPointVector;
    public static float RandomSpeed()
    {
        return Rnd.NextSingle() * (MaximumSpeed - MinimumSpeed) + MinimumSpeed;
    }

}
