
using Godot;
using System;

namespace BoidsProject;

public static class Formulas
{
    public static readonly Random Rnd = new Random();

    public static readonly Vector3 BoundRadius = new Vector3(50, 50, 50);
    public static readonly Vector3 PartitionSize = new Vector3(10, 10, 10);

    public static readonly float Separation = 1f;
    public static readonly float Cohesion = 1f;
    public static readonly float Alignment = 1f;

    public static readonly float PersonalRadiusSquared = (float) Math.Pow(1, 2);
    public static readonly float DetectRadiusSquared = (float) Math.Pow(4, 2);
    public static readonly float DetectAngle = 2f * (float) Math.PI;
    public static readonly float MinimumSpeed = 1f;
    public static readonly float MaximumSpeed = 2f;
    public static readonly float TurnSpeed = 1f;

    public static readonly Vector3 HalfPointVector = new Vector3(0.5f, 0.5f, 0.5f);

    public static Vector3 RandomVector3()
    {
        return new Vector3(Rnd.NextSingle(), Rnd.NextSingle(), Rnd.NextSingle());
    }

    public static Vector3 RandomVector3Centered()
    {
        return RandomVector3() - HalfPointVector;
    }

    public static float RandomSpeed()
    {
        return Rnd.NextSingle() * (MaximumSpeed - MinimumSpeed) + MinimumSpeed;
    }




}
