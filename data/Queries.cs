using Arch.Core.Utils;
using Arch.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsProject.data;

public static class Queries
{

    public static readonly QueryDescription AliveBoids = new QueryDescription
    {
        All = new ComponentType[]
        {
            typeof(Alive),
            typeof(Position),
        },
        Any = Array.Empty<ComponentType>(),
        None = Array.Empty<ComponentType>(),
        Exclusive = Array.Empty<ComponentType>()
    };
}