using Arch.Core;
using Arch.Core.Extensions;
using Arch.System;
using Boids.Util;
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
    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Move([Data] in float delta, in Entity ent, ref Quadtree<EntityReference> archRoot, ref Sprite2D node2d,
        ref Position pos, ref Direction dir, ref Speed speed) //, ref Transform2D transform) // ref MultiMeshInstance2D mm, ref Id id, 
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


        var neighboorNodes = archRoot.QueryNodes(pos.Value, Parameters.DetectRadius, []);
        var neighboorEntities = neighboorNodes.SelectMany(n => n.Data).Where(eref => eref.IsAlive());
        //int count = neighboorEntities.Length;

        foreach (var eref in neighboorEntities)
        {
            //if (eref.IsAlive() == false) continue;
            Entity e = eref.Entity;

            // skip self and non-boid entities
            if (e == ent || !e.Has<BoidTag>()) continue;

            var pos2 = e.Get<Position>().Value;
            var deltaPos = pos.Value - pos2;
            var distSquare = deltaPos.LengthSquared();
            // Avoidance
            if (distSquare <= Parameters.AvoidanceRadiusSquare)
            {
                separation += deltaPos;
                countInAvoidance++;
            }
            else
            // Flocking
            if (distSquare <= Parameters.DetectRadiusSquare)
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
        steering += AvoidObstacles(neighboorEntities, pos, node2d);
        // Target
        steering += ToTarget(pos.Value);

        // Apply
        ApplySteering(steering, delta, ent, ref node2d, ref pos, ref dir, ref speed); //, ref transform); // ref mm, ref id, 

        // Remove from leaf and move to tree
        //var thisRef = ent.Reference();
        //archRoot.Remove(thisRef, pos.Value);
        //archRoot.Insert(thisRef, pos.Value);
    }

    private Vector2 ToTarget(Vector2 pos)
    {
        return Parameters.TargetWeight * 0.01f * (Parameters.Target - pos);
    }

    private Vector2 AvoidObstacles(IEnumerable<EntityReference> neighboorEntities, Position pos, Node2D node2d)
    {
        //var scene = node2d.GetParent().GetParent<Node2D>();

        /*
         * TODO obstacles avoidance
         */
        //var obstacles = neighboorEntities
        //    .Select(eref => eref.Entity)
        //    .Where(e => e.Has<ObstacleTag>());
        Vector2 steering = Vector2.Zero;
        bool hasObstacle = false;
        foreach (var oref in neighboorEntities)
        {
            var obstacle = oref.Entity;
            if (!obstacle.Has<ObstacleTag>()) continue;

            var pos2 = obstacle.Get<Position>().Value;

            var obstacleRadius = Parameters.ObstacleRadius;
            var detectRadius = Parameters.DetectRadius;
            // check if detect radius intersects with obstacle radius
            var deltaPosObs = pos.Value - pos2;
            var distSquareObs = deltaPosObs.LengthSquared();
            if (distSquareObs <= detectRadius * detectRadius + obstacleRadius * obstacleRadius)
            {
                steering += Vector2.One / deltaPosObs;
                //scene.DrawLine(pos.Value, pos2, Colors.Violet, 5f);
                //scene.CallDeferred("DrawLine", pos.Value, pos2, Colors.Violet, 5f, true);
                //Boids.Instance.Lines.AddChild(
                //    new Line2D()
                //    {
                //        Points = [pos.Value, pos2],
                //        Width = 3f,
                //        DefaultColor = Colors.Violet
                //    }
                //);
                hasObstacle = true;
                //steering +=  obstacle.N  * obstacleWeight * deltaPosObs; // N = face normal
            }
        }
        if (hasObstacle)
        {
            node2d.Modulate = Colors.Red;
        }
        else
        {
            node2d.Modulate = Colors.White;
        }
        return steering * Parameters.ObstacleAvoidanceWeight;
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

    private void ApplySteering(Vector2 steering,
        in float delta, in Entity ent, ref Sprite2D node2d, // ref MultiMeshInstance2D mm, ref Id id, 
        ref Position pos, ref Direction dir, ref Speed speed) //, ref Transform2D transform)
    {
        // Lerp velocity
        var newSpeed = Math.Clamp(steering.Length(), Parameters.MinimumSpeed, Parameters.MaximumSpeed);
        speed.Value = speed.Value.LerpTo(newSpeed, Parameters.Lerp);
        dir.Value = dir.Value.LerpTo(steering.Normalized(), Parameters.Lerp);

        // Update pos
        pos.Value += dir.Value * speed.Value * delta;
        var angle = dir.Value.Angle();

        node2d.Position = pos.Value;
        node2d.Rotation = angle;

    }

}