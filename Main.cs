using Arch.Core;
using Arch.System;
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
    public ArchChunk2d Tree { get; set; }

    private Main()
    {
        Instance = this;
        World = World.Create();
        Systems = new Group<float>(
            "Physics",
            new MovementSystem(World)
        );

        Parameters.BoundRadius = new Vector2(1280, 720) / 2f;
        Tree = new(Parameters.BoundRadius * 2f * Parameters.TreeToSpaceboundFactor);
        Tree.Subdivide(2);
        
        World.SubscribeEntityDestroyed(Tree.Remove);
        World.SubscribeEntityCreated(Tree.Insert);

        EventBus.centralBus.subscribe(this);
    }

    [Subscribe(nameof(Parameters.BoundRadius), nameof(Parameters.TreeToSpaceboundFactor))]
    public void OnResize()
    {
        //var newtree = new ArchChunk2d(Parameters.BoundRadius * Parameters.TreeToSpaceboundFactor);
        Tree.Resize(Parameters.BoundRadius * 2f * Parameters.TreeToSpaceboundFactor);
    }

}
