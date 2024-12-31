using Arch.Core;
using Arch.System;
using BoidsProject.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsProject;

public class Main
{
    public static World World { get; set; } = World.Create();
    public static Group<float> Systems { get; set; } = new Group<float>(
        "Physics",
        new MovementSystem(World)
    );
}
