using Arch.Core;
using Arch.Core.Extensions;
using Boids.data;
using Boids.Util;
using BoidsProject.data;
using BoidsProject.Util;
using Godot;
using Godot.Sharp.Extras;
using System;

namespace BoidsProject;

public partial class Boids : Node2D
{
    #region Nodes
    [NodePath] public Camera2D Camera2D { get; set; }
    [NodePath] public ColorRect BoundsBackground { get; set; }
    [NodePath] public MultiMeshInstance2D MultiMeshInstance2D { get; set; }
    [NodePath] public Node2D Lines { get; set; }
    [NodePath] public Node2D Target { get; set; }
    #endregion

    public override void _PhysicsProcess(double delta) => Main.Instance.Systems.Update((float) delta);

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        EventBus.centralBus.subscribe(this);

        var texture = GD.Load<Texture2D>("res://assets/Flayer Evo1 1.png");

        MultiMeshInstance2D.Texture = texture;
        MultiMeshInstance2D.Multimesh.InstanceCount = Parameters.Count;
        MultiMeshInstance2D.Multimesh.Mesh = new QuadMesh()
        {
            Size = new Vector2(50, 36)
        };

        var boidScene = GD.Load<PackedScene>("res://scenes/Boid.tscn");
        for (int i = 0; i < Parameters.Count; i++)
        {
            var node = boidScene.Instantiate<Node2D>();
            var entt = Main.Instance.World.Create(
                new Alive(),
                new BoidTag(),

                Main.Instance.Tree,
                MultiMeshInstance2D,
                new Id(i),

                new Position(Parameters.RandomPosition()),
                new Direction(Parameters.RandomVector2Centered().Normalized()),
                new Speed(Parameters.RandomSpeed()),
                new Transform2D(),
                node
            );
            // Add node2d to scene if 1st entity
            if(i == 0)
                MultiMeshInstance2D.AddChild(node);
        }

        DrawChunks(Main.Instance.Tree, 0, Main.Instance.Tree.GetDeepestDepth());
    }

    [Subscribe(Events.DrawChunks, nameof(Parameters.BoundRadius), nameof(Parameters.TreeToSpaceboundFactor))]
    public void OnResize()
    {
        BoundsBackground.Size = Parameters.BoundRadius * 2f;
        BoundsBackground.Position = -Parameters.BoundRadius;
        Lines.RemoveAndQueueFreeChildren();
        DrawChunks(Main.Instance.Tree, 0, Main.Instance.Tree.GetDeepestDepth());
    }

    private void DrawChunks(ArchChunk2d chunk, int depth, int totalDepth)
    {
        if (chunk.IsLeaf) return;
        foreach (var child in chunk.Children)
            DrawChunks(child, depth + 1, totalDepth);

        float hue = (float) depth / (float) totalDepth;
        float width = 5f * (1f - hue);
        Color color = Color.FromHsv(hue, 1, 1, (1f - hue));

        var v = new Line2D();
        v.AddPoint(new Vector2(chunk.Center.X, chunk.Center.Y - chunk.HalfSize.Y));
        v.AddPoint(new Vector2(chunk.Center.X, chunk.Center.Y + chunk.HalfSize.Y));
        v.DefaultColor = color;
        v.Width = width;

        var h = new Line2D();
        h.AddPoint(new Vector2(chunk.Center.X - chunk.HalfSize.X, chunk.Center.Y));
        h.AddPoint(new Vector2(chunk.Center.X + chunk.HalfSize.X, chunk.Center.Y));
        h.DefaultColor = color;
        h.Width = width;

        Lines.AddChild(v);
        Lines.AddChild(h);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            InputEventMouseButton emb = (InputEventMouseButton) @event;
            if (emb.IsPressed())
            {
                if(emb.ButtonIndex == MouseButton.Left)
                {
                    Target.Position = this.GetGlobalMousePosition(); //emb.Position;
                    Parameters.Target = Target.Position;
                }
                if (emb.ButtonIndex == MouseButton.WheelUp)
                {
                    Camera2D.Zoom *= 1.1f;
                }
                if (emb.ButtonIndex == MouseButton.WheelDown)
                {
                    Camera2D.Zoom *= 0.9f;
                }
            }
        }
    }

}
