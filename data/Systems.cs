using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using BoidsProject.Util;
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
    [All(typeof(Alive))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move([Data] in float delta, in Entity ent, ref ArchChunk2d archRoot, ref MultiMeshInstance2D mm, ref Id id,
        ref Position pos, ref Direction dir, ref Speed speed, ref Transform2D transform)
    {
        var steering = Vector2.Zero;

        var avgPos = Vector2.Zero;
        var avgVel = Vector2.Zero;
        var close_d = Vector2.Zero;
        int countInAvoidance = 0;
        int countInProximity = 0;

        var archChunk = archRoot.Search(pos.Value);
        foreach (var chunk in archChunk.Neighboors)
        {
            foreach(var e in chunk.Data)
            {
                // skip self
                if (e == ent) continue;

                var pos2 = e.Get<Position>().Value;
                var deltaPos = pos.Value - pos2;
                var dist = deltaPos.LengthSquared();
                // Avoidance
                if (dist <= Parameters.AvoidanceRadiusSquared)
                {
                    close_d += deltaPos;
                    countInAvoidance++;
                }
                else 
                // Flocking
                if (dist <= Parameters.DetectRadiusSquared)
                {
                    avgPos += pos2;
                    avgVel += e.Get<Direction>().Value * e.Get<Speed>().Value;
                    countInProximity++;
                }
            }
        }

        avgPos /= countInProximity;
        avgVel /= countInProximity;

        steering += close_d * Parameters.Separation;
        steering += avgPos * Parameters.Cohesion;
        steering += avgVel * Parameters.Alignment;
        /*
         * TODO obstacles avoidance
        foreach(var obstacle in obstacles) {
            steering += obstacle.N * obstacleWeight * deltaPosObs;
        }
         */


        ApplySteering(steering, archChunk, delta, ent, ref mm, ref id, ref pos, ref dir, ref speed, ref transform);
    }

    private void ApplySteering(Vector2 steering, ArchChunk2d archChunk,
        in float delta, in Entity ent, ref MultiMeshInstance2D mm, ref Id id,
        ref Position pos, ref Direction dir, ref Speed speed, ref Transform2D transform)
    {
        // Lerp velocity
        speed.Value = speed.Value.LerpTo(steering.Length(), Parameters.Lerp);
        dir.Value = dir.Value.LerpTo(steering.Normalized(), Parameters.Lerp);

        // Update pos
        pos.Value += dir.Value * speed.Value * delta;
        transform = new Transform2D(dir.Value.Angle(), pos.Value);
        mm.Multimesh.SetInstanceTransform2D(id.Value, transform);

        // Remove from leaf and move to tree
        archChunk.MoveFromLeafToTree(ent);
    }

}