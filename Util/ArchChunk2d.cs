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
    private const int CHILD_COUNT = 4; // QuadTree
    public ArchChunk2d Root { get; set; }
    public ArchChunk2d? Parent { get; set; }
    public ArchChunk2d[]? Children { get; set; }
    public List<Entity>? Data { get; set; } = new();
    public bool IsLeaf { get => Children == null; }
    public ArchChunk2d[] Neighboors
    {
        get
        {
            if (Parent == null)
                return Children;
            else
                return Parent.Children;
        }
    }
    #endregion

    #region Space
    public Vector2 Center { get; set; } = Vector2.Zero;
    private Vector2 _size;
    public Vector2 Size
    {
        get => _size;
        set
        {
            _size = value;
            HalfSize = value / 2f;
            QuarterSize = value / 4f;
        }
    }
    private Vector2 HalfSize;
    private Vector2 QuarterSize;
    #endregion

    public ArchChunk2d(Vector2 size)
    {
        this.Size = size;
        this.Root = this;
    }
    public ArchChunk2d(Vector2 center, Vector2 size, ArchChunk2d parent) : this(size)
    {
        this.Center = center;
        this.Parent = parent;
        this.Root = parent.Root;
    }

    /// <summary>
    /// Returns a leaf node containing the position, starting from this node
    /// </summary>
    public ArchChunk2d Search(Vector2 pos)
    {
        if (IsLeaf) return this;
        int index = PositionToIndex(pos);
        return Children[index].Search(pos);
    }

    public ArchChunk2d[] GetNeighboors(Vector2 pos) => Search(pos).Neighboors;

    /// <summary>
    /// Insert the entity in the node or its child
    /// </summary>
    public void Insert(in Entity e)
    {
        if (!IsLeaf)
        {
            // If node has children
            var pos = e.Get<Position>().Value;
            var index = PositionToIndex(pos);
            //if(index > -1)
            Children[index].Insert(e);
        }
        else
        {
            Data.Add(e);
        }
    }

    /// <summary>
    /// Remove the entity from the node or its child
    /// </summary>
    public void Remove(in Entity e)
    {
        if (!IsLeaf)
        {
            // If node has children
            var pos = e.Get<Position>().Value;
            var index = PositionToIndex(pos);
            Children[index].Remove(e);
        }
        else
        {
            Data.Remove(e);
        }
    }

    /// <summary>
    /// Remove the entity from this leaf and insert it in the tree to a new leaf.
    /// </summary>
    public void MoveFromLeafToTree(in Entity e)
    {
        if (!IsLeaf)
            throw new Exception("Call this function only on leaves to move an entity to a new leaf");
        Data.Remove(e);
        Root.Insert(e);
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
    /// Make 4 children, or subdivide children into 4 each
    /// </summary>
    public void Subdivide(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            if (IsLeaf)
            {
                Children = new ArchChunk2d[CHILD_COUNT];
                int index = 0;
                // Les 4 espaces autour d'un centre = Comme les 4 coins d'un carré 3x3
                for (int x = -1; x <= 1; x += 2)
                {
                    for (int y = -1; y <= 1; y += 2)
                    {
                        var offset = new Vector2(x * QuarterSize.X, y * QuarterSize.Y);
                        var node = new ArchChunk2d(Center + offset, HalfSize, this);
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
        Root = null;
    }

}
