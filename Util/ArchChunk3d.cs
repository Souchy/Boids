using Arch.Core;
using Arch.Core.Extensions;
using BoidsProject.data;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BoidsProject.Util;

public enum ArchNodePosition
{
    //  x,  y,  z // binary id
    LeftBottomBack,     // -1, -1, -1 // 0
    LeftBottomFront,    // -1, -1, +1 // 1
    LeftTopBack,        // -1, +1, -1 // 2
    LeftTopFront,       // -1, +1, +1 // 3
    RightBottomBack,    // +1, -1, -1 // 4
    RightBottomFront,   // +1, -1, +1 // 5
    RightTopBack,       // +1, +1, -1 // 6
    RightTopFront       // +1, +1, +1 // 7
}

/// <summary>
/// Octree 3d
/// </summary>
public class ArchChunk3d : IDisposable
{
    #region Tree
    public ArchChunk3d? Parent { get; set; }
    public ArchChunk3d[]? Children { get; set; }
    public List<Entity>? Data { get; set; }
    public bool IsLeaf { get => Children == null; }
    public ArchChunk3d[] Neighboors { get => Parent?.Children; }
    #endregion

    #region Space
    public Vector3 Center { get; set; } = Vector3.Zero;
    private float _size;
    public float Size
    {
        get => _size;
        set
        {
            _size = value;
            HalfSize = value / 2f;
            QuarterSize = value / 4f;
        }
    }
    private float HalfSize;
    private float QuarterSize;
    #endregion

    public ArchChunk3d(float size)
    {
        this.Size = size;
    }
    public ArchChunk3d(Vector3 center, float size) : this(size)
    {
        this.Center = center;
    }

    public void OnEntityDestroyed(in Entity entity)
    {

    }

    /// <summary>
    /// Returns a leaf node containing the position
    /// </summary>
    public ArchChunk3d Search(Vector3 pos)
    {
        if (IsLeaf) return this;
        int index = PositionToIndex(pos);
        return Children[index].Search(pos);
    }

    public ArchChunk3d[] GetNeighboors(Vector3 pos) => Search(pos).Neighboors;

    public void Insert(Entity e)
    {
        if (IsLeaf)
        {
            Data.Add(e);
            return;
        }
        else
        {
            // If node has children
            var pos = e.Get<Position>().Value;
            var index = PositionToIndex(pos);
            //if(index > -1)
            Children[index].Insert(e);
        }
    }

    /// <summary>
    /// Index corresponds to ArchNodePosition indexing
    /// </summary>
    private int PositionToIndex(Vector3 pos)
    {
        var delta = pos - Center;
        var deltaAbs = delta.Abs();
        //if (deltaAbs.X > HalfSize || deltaAbs.Y > HalfSize || deltaAbs.Z > HalfSize)
        //    return -1;
        int index = 0;
        if (delta.X >= 0) index += 4;
        if (delta.Y >= 0) index += 2;
        if (delta.Z >= 0) index += 1;
        return index;
    }

    /// <summary>
    /// Make 8 children, or subdivide children into 8 each
    /// </summary>
    public void Subdivide(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            if (IsLeaf)
            {

                Children = new ArchChunk3d[8];
                int index = 0;
                // Les 8 espaces autour d'un centre = Comme les 8 coins d'un cube 3x3
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        for (int z = -1; z <= 1; z += 2)
                        {
                            var offset = new Vector3(x * QuarterSize, y * QuarterSize, z * QuarterSize);
                            var node = new ArchChunk3d(Center + offset, HalfSize);
                            node.Parent = this;
                            Children[index++] = node;
                        }
                    }
                }
                foreach (var entt in Data)
                {
                    Insert(entt);
                }
                Data = null;
            }
            else
            {
                foreach (var child in Children)
                    child.Subdivide();
            }
        }
    }

    public void Merge()
    {
        if (IsLeaf) return;
        Data = new();
        foreach (var child in Children)
        {
            child.Merge();
            this.Data.AddRange(child.Data);
            child.Dispose();
        }
        Children = null;
    }

    public void Dispose()
    {
        if (!IsLeaf)
        {
            foreach (var child in Children)
                child.Dispose();
        }
        Children = null;
        Data = null;
        Parent = null;
    }

    public int GetChildCount()
    {
        if (IsLeaf) return 0;
        return Children.Select(c => c.GetChildCount()).Sum();
    }
}
