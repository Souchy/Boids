using Arch.Core;
using Arch.Core.Extensions;
using Boids.data;
using Boids.Util;
using BoidsProject.data;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BoidsProject.Util;

public static class ChunkPositions
{
    public static Vector2[] Vectors = // (int X, int Y)
    {
        new(-1, -1),
        new(-1, +1),
        new(+1, -1),
        new(+1, +1)
    };

    public static int LeftTop = 0b00000000;     // -1, -1 // 00 = 0
    public static int LeftBottom = 0b00000001;  // -1, +1 // 01 = 1
    public static int RightTop = 0b00000010;    // +1, -1 // 10 = 2
    public static int RightBottom = 0b00000011; // +1, +1 // 11 = 3

    public static int[] Quadrants = { LeftTop, LeftBottom, RightTop, RightBottom };
    public static int[] Left = { LeftTop, LeftBottom };
    public static int[] Right = { RightTop, RightBottom };
    public static int[] Top = { LeftTop, RightTop };
    public static int[] Bottom = { LeftBottom, RightBottom };

    public static int[][] Rows = { Top, Bottom };
    public static int[][] Columns = { Left, Right };

    public static int GetOpposite(int pos)
    {
        return ~pos & 0x000000FF;
    }
    public static int Col(int pos) => pos & 0b00000001;
    public static int Row(int pos) => (pos & 0b00000010) >> 1; // convert to 0 or 1 instead of 2 or 3
    public static int[] GetOppositeRow(int pos)
    {
        int row = Row(~pos); // flip then get row
        return Rows[row];
    }
    public static int[] OppositeColumn(int pos)
    {
        int col = Col(~pos); // flip then get col
        return Columns[col];
    }
}

/// <summary>
/// Octree 2d = Quatree dumbass
/// </summary>
public class ArchChunk2d : IDisposable
{
    #region Tree
    private const int CHILD_COUNT = 4; // QuadTree
    public int position;
    public ArchChunk2d Root { get; set; }
    public ArchChunk2d? Parent { get; set; }
    public ArchChunk2d[] Children { get; set; } = Array.Empty<ArchChunk2d>();
    public List<Entity> Data { get; set; } = new();
    public bool IsLeaf { get => Children.Length == 0; }
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
    public Vector2 HalfSize { get; private set; }
    public Vector2 QuarterSize { get; private set; }
    #endregion

    #region Other
    public long LastTimeChunkWasBig;
    #endregion

    public ArchChunk2d(Vector2 size)
    {
        this.Size = size;
        this.Root = this;
        ResetLastTimeChunkWasBig();
    }
    public ArchChunk2d(Vector2 center, Vector2 size, ArchChunk2d parent) : this(size)
    {
        this.Center = center;
        this.Parent = parent;
        this.Root = parent.Root;
    }

    public ArchChunk2d GetNeighboorUp()
    {
        int totalDepth = Root.GetDeepestDepth();
        int currentDepth = this.GetDeepestDepth();

        // Rules:
        // 1: Siblings are always neighboors
        // 2: Check if the cell is touching the edge of the grand-father and any old ancestor.

        // A, B, C, D
        // E, F, G, H
        // I, J, K, L
        // M, N, O, P

        // R0 | 00, 01 | 04, 05 | 16, 17 | 00, 01
        // R1 | 02, 03 | 06, 07 | 18, 19 | 02, 03
        //    ------------------------------------
        // R2 | 08, 09 | 12, 13 | 20, 21 | 00, 01
        // R3 | 10, 11 | 14, 15 | 22, 23 | 02, 03
        //    -----------------------------------
        // R2 | 00, 01 | 00, 01 | 00, 01 | 00, 01
        // R3 | 02, 03 | 02, 03 | 02, 03 | 02, 03
        //    -----------------------------------
        // R2 | 00, 01 | 00, 01 | 00, 01 | 00, 01
        // R3 | 02, 03 | 02, 03 | 02, 03 | 02, 03

        // B 06 -> A [01, 03], B [04, 05, 06, 07], C [09], D [12, 13] }
        // B 02 = BottomLeft
        //      A (left of B)       -> 01, 03 (right column, opposite side)
        //      C (bottomleft of B) -> 09 (top-right = opposite corner)
        //      D (down of B)       -> 12, 13 (top row, opposite side)
        List<ArchChunk2d> neighboors06 = new();
        // A
        neighboors06.Add(Parent.Parent.Children[0].Children[1]);
        neighboors06.Add(Parent.Parent.Children[0].Children[3]);
        // B: #1 rule
        neighboors06.AddRange(Parent.Children);
        // C
        neighboors06.Add(Parent.Parent.Children[2].Children[1]);
        // D
        neighboors06.Add(Parent.Parent.Children[3].Children[1]);
        neighboors06.Add(Parent.Parent.Children[3].Children[2]);

        //ChunkPositionInParent pos = ChunkPositionInParent.BottomLeft;




        var parent = Parent;
        while (parent != null)
        {
            parent = parent.Parent;
        }

        for (int i = currentDepth; i < totalDepth; i++)
        {

        }

        return this;
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
            ResetLastTimeChunkWasBig();
            if (Data.Count > Parameters.MaximumBoidsPerChunk)
            {
                this.Subdivide();
                EventBus.centralBus.publish(Events.DrawChunks);
            }
        }
    }

    private void ResetLastTimeChunkWasBig()
    {
        LastTimeChunkWasBig = DateTime.Now.CurrentTimeSecond();
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
            var now = DateTime.Now.CurrentTimeSecond();
            if (now - LastTimeChunkWasBig > 10)
            {
                var sum = Parent?.Children.Sum(c => c.Data.Count) ?? Parameters.MaximumBoidsPerChunk;
                if (sum < Parameters.MinimumBoidsPerChunk)
                {
                    Parent.Merge();
                    EventBus.centralBus.publish(Events.DrawChunks);
                }
            }
        }
    }

    /// <summary>
    /// Remove the entity from this leaf and insert it in the tree to a new leaf.
    /// </summary>
    public void MoveFromLeafToTree(in Entity e)
    {
        if (!IsLeaf)
            throw new Exception("Call this function only on leaves to move an entity to a new leaf");
        //Data.Remove(e);
        var root = Root;
        Remove(e);
        root.Insert(e);
    }

    /// <summary>
    /// Index corresponds to ArchNodePosition indexing
    /// </summary>
    private int PositionToIndex(Vector2 pos)
    {
        var delta = pos - Center;
        int index = 0;
        if (delta.X >= 0) index += 2;
        if (delta.Y >= 0) index += 1;
        return index;
    }

    public void SetSubdivisions(int subCount = 1)
    {

    }
    /// <summary>
    /// Make 4 children, or subdivide children into 4 each
    /// </summary>
    public void Subdivide(int subCount = 1)
    {
        for (int i = 0; i < subCount; i++)
        {
            if (IsLeaf)
            {
                Children = new ArchChunk2d[CHILD_COUNT];
                // Les 4 espaces autour d'un centre = Comme les 4 coins d'un carré 3x3
                int index = 0;
                ForeachCell((x, y) =>
                {
                    var offset = new Vector2(x * QuarterSize.X, y * QuarterSize.Y);
                    var node = new ArchChunk2d(Center + offset, HalfSize, this);
                    node.position = index;
                    Children[index++] = node;
                });
                var temp = Data;
                Data = new();
                foreach (var entt in temp)
                {
                    Insert(entt);
                }
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
        Children = Array.Empty<ArchChunk2d>(); ;
    }

    public int GetDeepestDepth()
    {
        if (IsLeaf) return 1;
        return 1 + Children.Max(c => c.GetDeepestDepth());
    }

    public int GetEntityCount()
    {
        if (IsLeaf) return Data.Count;
        return Children.Sum(c => c.GetEntityCount());
    }

    public void Resize(Vector2 size)
    {
        this.Size = size;
        if (!IsLeaf)
        {
            // Les 4 espaces autour d'un centre = Comme les 4 coins d'un carré 3x3
            int index = 0;
            ForeachCell((x, y) =>
            {
                var offset = new Vector2(x * QuarterSize.X, y * QuarterSize.Y);
                Children[index].Center = Center + offset;
                Children[index].Resize(HalfSize);
                index++;
            });
        }
    }

    private void ForeachCell(Action<float, float> act)
    {
        //for (int x = -1; x <= 1; x += 2)
        //{
        //    for (int y = -1; y <= 1; y += 2)
        //    {
        //        act(x, y);
        //    }
        //}
        foreach (var vec in ChunkPositions.Vectors)
            act(vec.X, vec.Y);
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
