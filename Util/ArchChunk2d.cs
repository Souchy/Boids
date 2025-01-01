using Arch.Core;
using Arch.Core.Extensions;
using BoidsProject.data;
using Godot;
using System;
using System.Collections.Generic;

namespace BoidsProject.Util;

/// <summary>
/// Octree 2d = Quatree dumbass
/// </summary>
public class ArchChunk2d : IDisposable
{
    #region Tree
    public ArchChunk2d? Parent { get; set; }
    public ArchChunk2d[]? Children { get; set; }
    public List<Entity>? Data { get; set; }
    public bool IsLeaf { get => Children == null; }
    public ArchChunk2d[] Neighboors { get => Parent?.Children; }
    #endregion

    #region Space
    public Vector2 Center { get; set; } = Vector2.Zero;
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

    public ArchChunk2d(float size)
    {
        this.Size = size;
    }
    public ArchChunk2d(Vector2 center, float size) : this(size)
    {
        this.Center = center;
    }

    public void OnEntityDestroyed(in Entity entity)
    {

    }

    /// <summary>
    /// Returns a leaf node containing the position
    /// </summary>
    public ArchChunk2d Search(Vector2 pos)
    {
        if (IsLeaf) return this;
        int index = PositionToIndex(pos);
        return Children[index].Search(pos);
    }

    public ArchChunk2d[] GetNeighboors(Vector2 pos) => Search(pos).Neighboors;

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
            var pos = e.Get<Position2d>().Value;
            var index = PositionToIndex(pos);
            //if(index > -1)
            Children[index].Insert(e);
        }
    }

    /// <summary>
    /// Index corresponds to ArchNodePosition indexing
    /// </summary>
    private int PositionToIndex(Vector2 pos)
    {
        var delta = pos - Center;
        var deltaAbs = delta.Abs();
        //if (deltaAbs.X > HalfSize || deltaAbs.Y > HalfSize || deltaAbs.Z > HalfSize)
        //    return -1;
        int index = 0;
        if (delta.X >= 0) index += 2;
        if (delta.Y >= 0) index += 1;
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
                Children = new ArchChunk2d[8];
                int index = 0;
                // Les 8 espaces autour d'un centre = Comme les 8 coins d'un cube 3x3
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        var offset = new Vector2(x * QuarterSize, y * QuarterSize);
                        var node = new ArchChunk2d(Center + offset, HalfSize);
                        node.Parent = this;
                        Children[index++] = node;
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

}
