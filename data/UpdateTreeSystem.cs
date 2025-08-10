using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Boids.Util;
using BoidsProject;
using BoidsProject.data;
using Godot;
using System.Runtime.CompilerServices;

namespace Boids.data;

public partial class UpdateTreeSystem : BaseSystem<World, float>
{
    public UpdateTreeSystem(World world) : base(world) { }

    [Query]
    [All(typeof(Alive), typeof(BoidTag))]
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateTree([Data] in float delta, in Entity ent)
    {
        // update tree
        //archRoot = Main.Instance.Tree;
        ent.Set(Main.Instance.Tree);
        // insert entity
        Main.Instance.Tree.Insert(ent.Reference(), ent.Get<Position>().Value);
    }

}
