using Arch.Core;
using Arch.System;
using Boids.data;
using Boids.Util;
using BoidsProject.data;
using BoidsProject.Util;
using Godot;

namespace BoidsProject;

public partial class Main : Node
{
    public static Main Instance { get; private set; }

    public World World { get; set; }
    public Group<float> Systems { get; set; }

    public Quadtree<EntityReference> Tree;

    private Main()
    {
        Instance = this;
        World = World.Create();
        Systems = new Group<float>(
            "Physics",
            new UpdateTreeSystem(World),
            new MovementSystem(World)
        );
        Systems.Initialize();

        Parameters.BoundRadius = new Vector2(1280, 720) / 2f;

        var size = Parameters.BoundRadius * 2f * Parameters.TreeToSpaceboundFactor;
        var bounds = new Rect2(-size.X / 2f, -size.Y / 2f, size.X, size.Y);
        Tree = new Quadtree<EntityReference>(0, bounds);

        EventBus.centralBus.subscribe(this);
    }

    [Subscribe(nameof(Parameters.BoundRadius), nameof(Parameters.TreeToSpaceboundFactor))]
    public void OnResize()
    {
        var size = Parameters.BoundRadius * 2f * Parameters.TreeToSpaceboundFactor;
        var bounds = new Rect2(-size.X / 2f, -size.Y / 2f, size.X, size.Y);
        Tree = new Quadtree<EntityReference>(0, bounds);
    }


}
