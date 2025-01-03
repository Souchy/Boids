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
    [All(typeof(Alive), typeof(BoidTag))]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move([Data] in float delta, in Entity ent, ref ArchChunk2d archRoot, ref MultiMeshInstance2D mm, ref Id id, ref Node2D node2d,
        ref Position pos, ref Direction dir, ref Speed speed, ref Transform2D transform)
    {
        var currentVel = dir.Value * speed.Value;
        var steering = Vector2.Zero;
        steering += currentVel;

        #region Neighboors
        var avgPos = Vector2.Zero;
        var avgVel = Vector2.Zero;
        var separation = Vector2.Zero;
        int countInAvoidance = 0;
        int countInProximity = 0;

        var archChunk = archRoot.Search(pos.Value);
        foreach (var e in archChunk.Neighboors.SelectMany(n => n.Data))
        {
            // skip self
            if (e == ent) continue;

            var pos2 = e.Get<Position>().Value;
            var deltaPos = pos.Value - pos2;
            var dist = deltaPos.Length();
            // Avoidance
            if (dist <= Parameters.AvoidanceRadius)
            {
                separation += deltaPos;
                countInAvoidance++;
            }
            else
            // Flocking
            if (dist <= Parameters.DetectRadius)
            {
                avgPos += pos2;
                avgVel += e.Get<Direction>().Value * e.Get<Speed>().Value;
                countInProximity++;
            }
        }
        if (countInProximity > 0)
        {
            avgPos /= countInProximity;
            avgVel /= countInProximity;
            steering += (avgPos - pos.Value) * Parameters.Cohesion;
            steering += (avgVel - currentVel) * Parameters.Alignment;
        }
        if (countInAvoidance > 0)
        {
            steering += separation * Parameters.Separation;
        }
        #endregion

        // Bound avoidance
        steering += AvoidBounds(pos.Value, dir.Value);
        // Obstacles
        steering += AvoidObstacles();
        // Target
        steering += ToTarget(pos.Value);

        // Apply
        ApplySteering(steering, archChunk, delta, ent, ref mm, ref id, ref node2d, ref pos, ref dir, ref speed, ref transform);

        // Remove from leaf and move to tree
        archChunk.MoveFromLeafToTree(ent);
    }

    private Vector2 ToTarget(Vector2 pos)
    {
        return (Parameters.Target - pos) * Parameters.TargetWeight;
    }

    private Vector2 AvoidObstacles()
    {
        /*
         * TODO obstacles avoidance
        foreach(var obstacle in obstacles) {
            steering += obstacle.N * obstacleWeight * deltaPosObs;
        }
         */
        return Vector2.Zero;
    }

    private Vector2 AvoidBounds(Vector2 pos, Vector2 dir)
    {
        // Avoid Bounds
        var avoidBounds = Vector2.Zero;
        var vectorToOrigin = pos;
        var forwardDetection = vectorToOrigin + dir * Parameters.DetectRadius;
        var boundDelta = forwardDetection.Abs() - Parameters.BoundRadius;
        if (boundDelta.X > 0)
        {
            avoidBounds -= boundDelta.X * Parameters.BoundAvoidanceWeight * vectorToOrigin.Normalized().X * Vector2.Right;
        }
        if (boundDelta.Y > 0)
        {
            avoidBounds -= boundDelta.Y * Parameters.BoundAvoidanceWeight * vectorToOrigin.Normalized().Y * Vector2.Down;
        }
        return avoidBounds;
    }

    private void ApplySteering(Vector2 steering, ArchChunk2d archChunk,
        in float delta, in Entity ent, ref MultiMeshInstance2D mm, ref Id id, ref Node2D node2d,
        ref Position pos, ref Direction dir, ref Speed speed, ref Transform2D transform)
    {
        // Lerp velocity
        var newSpeed = Math.Clamp(steering.Length(), Parameters.MinimumSpeed, Parameters.MaximumSpeed);
        speed.Value = speed.Value.LerpTo(newSpeed, Parameters.Lerp);
        dir.Value = dir.Value.LerpTo(steering.Normalized(), Parameters.Lerp);

        // Update pos
        pos.Value += dir.Value * speed.Value * delta;
        var angle = dir.Value.Angle();
        {
            if (dir.Value.X > 0)
            {
                transform = new Transform2D(angle % (float) Math.PI, Parameters.FlipV, 0, pos.Value);
                //transform = new Transform2D(angle, Parameters.FlipH, 0, pos.Value);
            }
            else
            {
                transform = new Transform2D(angle, pos.Value);
                //transform = new Transform2D(angle, Vector2.One, 0, pos.Value);
            }
            mm.Multimesh.SetInstanceTransform2D(id.Value, transform);
        }
        {
            node2d.Position = pos.Value;
        }

    }

}