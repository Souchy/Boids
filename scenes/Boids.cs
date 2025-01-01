using Arch.Core;
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

        int count = 1000;
        var texture = GD.Load<Texture2D>("res://assets/Flayer Evo1 1.png");
        MultiMeshInstance2D.Texture = texture;
        MultiMeshInstance2D.Multimesh.InstanceCount = count;
        MultiMeshInstance2D.Multimesh.Mesh = new QuadMesh()
        {
            Size = new Vector2(20, 20)
        };
        for (int i = 0; i < count; i++)
        {
            Main.World.Create(
                new Alive(),
                new BoidTag(),

                Main.Tree,
                MultiMeshInstance2D,
                new Id(i),

                new Position(Parameters.RandomPosition()),
                new Direction(Parameters.RandomVector2Centered().Normalized()),
                new Speed(Parameters.RandomSpeed()),
                new Transform2D()
            );
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Main.Systems.Update((float) delta);
    }

}
