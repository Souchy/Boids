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
    public static Boids Instance;
    #region Nodes
    [NodePath] public Camera2D Camera2D { get; set; }
    [NodePath] public ColorRect BoundsBackground { get; set; }
    //[NodePath] public MultiMeshInstance2D MultiMeshInstance2D { get; set; }
    [NodePath] public Node2D Lines { get; set; }
    [NodePath] public Node2D Target { get; set; }
    #endregion

    //public override void _PhysicsProcess(double delta)
    //{
    //    Parameters.AvoidanceRadiusSquare = Parameters.AvoidanceRadius * Parameters.AvoidanceRadius;
    //    Parameters.DetectRadiusSquare = Parameters.DetectRadius * Parameters.DetectRadius;
    //    Main.Instance.Tree.Clear();
    //    Main.Instance.Systems.Update((float) delta);
    //    OnResize();
    //}

    public override void _PhysicsProcess(double delta)
    {

        Parameters.AvoidanceRadiusSquare = Parameters.AvoidanceRadius * Parameters.AvoidanceRadius;
        Parameters.DetectRadiusSquare = Parameters.DetectRadius * Parameters.DetectRadius;
        Main.Instance.Tree.Clear();
        Main.Instance.Systems.Update((float) delta);
        OnResize();
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.OnReady();
        Instance = this;
        EventBus.centralBus.subscribe(this);

        //var evoTex = GD.Load<Texture2D>("res://assets/Flayer Evo1 1.png");
        var ringTex = GD.Load<Texture2D>("res://assets/ring100.png");
        var texture = GD.Load<Texture2D>("res://assets/arrow32.png");

        var poolNode = GetNode("SpritePool"); //new Node2D();
        this.AddChild(poolNode);

        // Create boids
        for (int i = 0; i < Parameters.Count; i++)
        {
            var sprite = new Sprite2D() { Texture = texture, TextureFilter = TextureFilterEnum.Nearest };
            //sprite.Modulate = new Color("#03fc17");
            var entt = Main.Instance.World.Create(
                new Alive(),
                new BoidTag(),
                Main.Instance.Tree,
                new Position(Parameters.RandomPosition()),
                new Direction(Parameters.RandomVector2Centered().Normalized()),
                new Speed(Parameters.RandomSpeed()),
                sprite
            );
            poolNode.AddChild(sprite);
            Main.Instance.Tree.Insert(entt.Reference(), entt.Get<Position>().Value);
        }

        // Create obstacle
        var obstacleNode = new Sprite2D()
        {
            Texture = ringTex,
            Modulate = new Color("#ff0000")
        };
        obstacleNode.Scale = Vector2.One * (Parameters.ObstacleRadius / 50f); // 50 is the radius of the texture
        var obstacle = Main.Instance.World.Create(
            new ObstacleTag(),
            Main.Instance.Tree,
            new Position(new Vector2(300, 200)),
            new CollisionShape2D()
            {
                Shape = new CircleShape2D()
                {
                    Radius = Parameters.ObstacleRadius
                }
            },
            obstacleNode
        );
        obstacleNode.Position = obstacle.Get<Position>().Value; 
        Main.Instance.Tree.Insert(obstacle.Reference(), obstacle.Get<Position>().Value);
        this.AddChild(obstacleNode);

        //DrawChunks(Main.Instance.Tree, 0, Quadtree<int>.MAX_DEPTH);
    }

    [Subscribe(Events.DrawChunks, nameof(Parameters.BoundRadius), nameof(Parameters.TreeToSpaceboundFactor))]
    public void OnResize()
    {
        BoundsBackground.Size = Parameters.BoundRadius * 2f;
        BoundsBackground.Position = -Parameters.BoundRadius;
        Lines.RemoveAndQueueFreeChildren();
        DrawChunks(Main.Instance.Tree, 0, Quadtree<int>.MAX_DEPTH); //Main.Instance.Tree.GetDeepestDepth());
    }

    private void DrawChunks<T>(Quadtree<T> chunk, int depth, int totalDepth)
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
        //GD.Print("Draw " + chunk.Center);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            InputEventMouseButton emb = (InputEventMouseButton) @event;
            if (emb.IsPressed())
            {
                if (emb.ButtonIndex == MouseButton.Right)
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
