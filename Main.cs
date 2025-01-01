using Arch.Core;
using Arch.System;
using BoidsProject.data;
using BoidsProject.Util;
using Godot;

namespace BoidsProject;

public static class Main
{
    public static World World { get; set; } = World.Create();
    public static Group<float> Systems { get; set; } = new Group<float>(
        "Physics",
        new MovementSystem(World)
    );
    public static ArchChunk2d Tree { get; set; }

    static Main()
    {
        Parameters.BoundRadius = new Vector2(1280, 720) / 2f;
        Tree = new(Parameters.BoundRadius * 2);
        Tree.Subdivide(3);
        World.SubscribeEntityDestroyed(Tree.Remove);
        World.SubscribeEntityCreated(Tree.Insert);
    }
}
