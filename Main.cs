using Arch.Core;
using Arch.System;
using BoidsProject.data;
using BoidsProject.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Tree = new(100);
        Tree.Subdivide(3);
        World.SubscribeEntityDestroyed(Tree.Remove);
        World.SubscribeEntityCreated(Tree.Insert);
    }
}
