using BoidsProject.data;
using Godot;
using Godot.Sharp.Extras;
using System;

namespace BoidsProject;

public partial class Boids : Node2D
{
    #region Nodes
    [NodePath] public MultiMeshInstance2D MultiMeshInstance2D { get; set; }
    #endregion


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        this.OnReady();
         MultiMeshInstance2D.Multimesh.InstanceCount = 1000;
        for (int i = 0; i < 1000; i++)
        {
            Main.World.Create(
                new Alive(),
                new BoidTag(),
                new Position(Formulas.RandomVector3Centered().Normalized() * Formulas.BoundRadius),
                new Direction(Formulas.RandomVector3Centered().Normalized()),
                new Speed(Formulas.RandomSpeed()),
                new Angle(0),
                //new Transform3D(),
                new Id(i)
            );
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

}
