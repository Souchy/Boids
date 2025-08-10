using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boids.Util;

public static class Intersections
{
    public record struct Circle(Vector2 Center, float Radius);
    public record struct Rectangle(Vector2 Center, Vector2 Radius);

    // Check that the distance between 2 circles is less than the sum of their radius
    public static bool Intersects(Circle a, Circle b)
    {
        var delta = a.Center - b.Center;
        var dist = delta.LengthSquared();
        var sum = a.Radius + b.Radius;
        return dist <= sum * sum;
    }

    public static bool Intersects(Circle a, Rectangle b)
    {
        var closestSide = new Vector2(
            Mathf.Clamp(a.Center.X, b.Center.X - b.Radius.X, b.Center.X + b.Radius.X),
            Mathf.Clamp(a.Center.Y, b.Center.Y - b.Radius.Y, b.Center.Y + b.Radius.Y)
        );
        var delta = a.Center - closestSide;
        return delta.LengthSquared() <= a.Radius * a.Radius;
    }

    public static bool Intersects(Rectangle a, Rectangle b)
    {
        var dist = a.Center - b.Center;
        var delta = dist.Abs() - (a.Radius + b.Radius);
        return delta.X <= 0 && delta.Y <= 0;
    }

    public static bool IsContained(Rectangle container, Rectangle contained)
    {
        var delta = container.Center - contained.Center;
        var radiusDiff = container.Radius - contained.Radius;
        return radiusDiff >= delta.Abs();
    }

    public static bool IsContained(Circle container, Circle contained)
    {
        var delta = container.Center - contained.Center;
        var radiusDiff = container.Radius - contained.Radius;
        return radiusDiff >= delta.Length();
    }

}