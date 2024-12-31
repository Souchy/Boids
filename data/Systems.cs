using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BoidsProject.data;


public partial class MovementSystem : BaseSystem<World, float>
{

    public MovementSystem(World world) : base(world) { }
    [Query]
    //[All(typeof(Alive), typeof(ProjectileTag))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move([Data] in float delta, in Entity ent, ref Position pos, ref Direction dir, ref Speed speed, ref Id id, Partition partition0, ref Transform3D transform, ref MultiMeshInstance3D mm)
    {


        Vector3 close_d = Vector3.Zero;
        Vector3 sumPos = Vector3.Zero;
        Vector3 sumDir = Vector3.Zero;
        float sumSpeed = 0f;
        int count = 0;

        //this.World.relation
        var q = World.Query(Queries.AliveBoids);
        foreach (var chunk in q) //ent.GetChunk().Entities)
        {
            foreach (var e in chunk.Entities)
            {
                // skip self
                if (e == ent) continue;
                //var partition1 = e.Get<Partition>();
                //if (partition1.Value - partition0.Value > 3) continue;


                var pos2 = e.Get<Position>().Value;
                var deltaPos = pos.Value - pos2;
                if(deltaPos.X > Formulas.PartitionSize.X || deltaPos.Y > Formulas.PartitionSize.Y || deltaPos.Z > Formulas.PartitionSize.Z) continue;

                var dist = deltaPos.LengthSquared();
                // Avoidance
                if (dist <= Formulas.PersonalRadiusSquared)
                {
                    close_d += deltaPos;
                }
                else 
                // Flocking
                if (dist <= Formulas.DetectAngle)
                {
                    sumPos += pos2;
                    sumDir += e.Get<Direction>().Value;
                    sumSpeed += e.Get<Speed>().Value;
                    count++;
                }
            }
        }

        close_d *= Formulas.Separation;

        speed.Value = (sumSpeed / count);
        dir.Value = (sumDir / count).Normalized();

        pos.Value += dir.Value * delta * speed.Value;

        var basis = Basis.LookingAt(dir.Value, Vector3.Up);
        transform = new Transform3D(basis, pos.Value);
        mm.Multimesh.SetInstanceTransform(id.Value, transform);
    }

    public void align()
    {

    }

}