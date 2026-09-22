using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace FunTimeCobra;

public readonly struct ChunkPos : IEquatable<ChunkPos>
{
    public readonly int X;
    public readonly int Y;
    public readonly int Z;

    public ChunkPos(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public bool Equals(ChunkPos other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is ChunkPos other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public static bool operator ==(ChunkPos a, ChunkPos b) => a.Equals(b);
    public static bool operator !=(ChunkPos a, ChunkPos b) => !a.Equals(b);
}

public class Chunk
{
    public const int Size = 16;
    private readonly ItemType[,,] _blocks = new ItemType[Size, Size, Size];
    private Model _model;
    private bool _hasMesh;
    private bool _dirty = true;

    public readonly ChunkPos Pos;
    public int CX => Pos.X;
    public int CY => Pos.Y;
    public int CZ => Pos.Z;

    private readonly World _world;

    private static readonly Color ColTop = new(255, 255, 255, 255);
    private static readonly Color ColBottom = new(130, 130, 130, 255);
    private static readonly Color ColZ = new(205, 205, 205, 255);
    private static readonly Color ColX = new(165, 165, 165, 255);

    public Chunk(World world, int cx, int cy, int cz)
    {
        _world = world;
        Pos = new ChunkPos(cx, cy, cz);
    }

    public ItemType GetLocal(int x, int y, int z)
    {
        if (x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size)
            return ItemType.None;
        return _blocks[x, y, z];
    }

    public void SetLocal(int x, int y, int z, ItemType t)
    {
        if (x < 0 || x >= Size || y < 0 || y >= Size || z < 0 || z >= Size) return;
        _blocks[x, y, z] = t;
        _dirty = true;
    }

    public void MarkDirty() => _dirty = true;

    public void Render(Vector3 camPos, float renderDist)
    {
        if (_dirty) BuildMesh();
        if (!_hasMesh) return;

        Vector3 center = new((CX + 0.5f) * Size, (CY + 0.5f) * Size, (CZ + 0.5f) * Size);
        if (Vector3.Distance(camPos, center) > renderDist + Size) return;

        Vector3 pos = new(CX * Size, CY * Size, CZ * Size);
        Raylib.DrawModel(_model, pos, 1.0f, Color.White);
    }

    public void BuildMesh()
    {
        _dirty = false;
        Unload();

        List<float> verts = new(2048);
        List<float> uvs = new(2048);
        List<float> norms = new(2048);
        List<byte> cols = new(2048);

        for (int x = 0; x < Size; x++)
        for (int y = 0; y < Size; y++)
        for (int z = 0; z < Size; z++)
        {
            ItemType type = _blocks[x, y, z];
            if (type == ItemType.None) continue;

            int wx = CX * Size + x;
            int wy = CY * Size + y;
            int wz = CZ * Size + z;

            // Верхняя грань (Y+)
            ItemType topBlock = _world.GetBlock(wx, wy + 1, wz);
            if (ShouldRenderFace(type, topBlock))
                AddFace(verts, uvs, norms, cols, type, 0, new Vector3(0, 1, 0), ColTop,
                    new Vector3(x, y + 1, z), new Vector3(x, y + 1, z + 1),
                    new Vector3(x + 1, y + 1, z + 1), new Vector3(x + 1, y + 1, z), FaceUV.Top);

            // Нижняя грань (Y-)
            ItemType bottomBlock = _world.GetBlock(wx, wy - 1, wz);
            if (ShouldRenderFace(type, bottomBlock))
                AddFace(verts, uvs, norms, cols, type, 1, new Vector3(0, -1, 0), ColBottom,
                    new Vector3(x, y, z + 1), new Vector3(x, y, z),
                    new Vector3(x + 1, y, z), new Vector3(x + 1, y, z + 1), FaceUV.Bottom);

            // Передняя (Z+)
            ItemType frontBlock = _world.GetBlock(wx, wy, wz + 1);
            if (ShouldRenderFace(type, frontBlock))
                AddFace(verts, uvs, norms, cols, type, 2, new Vector3(0, 0, 1), ColZ,
                    new Vector3(x, y, z + 1), new Vector3(x + 1, y, z + 1),
                    new Vector3(x + 1, y + 1, z + 1), new Vector3(x, y + 1, z + 1), FaceUV.Front);

            // Задняя (Z-)
            ItemType backBlock = _world.GetBlock(wx, wy, wz - 1);
            if (ShouldRenderFace(type, backBlock))
                AddFace(verts, uvs, norms, cols, type, 2, new Vector3(0, 0, -1), ColZ,
                    new Vector3(x + 1, y, z), new Vector3(x, y, z),
                    new Vector3(x, y + 1, z), new Vector3(x + 1, y + 1, z), FaceUV.Back);

            // Правая (X+)
            ItemType rightBlock = _world.GetBlock(wx + 1, wy, wz);
            if (ShouldRenderFace(type, rightBlock))
                AddFace(verts, uvs, norms, cols, type, 2, new Vector3(1, 0, 0), ColX,
                    new Vector3(x + 1, y, z + 1), new Vector3(x + 1, y, z),
                    new Vector3(x + 1, y + 1, z), new Vector3(x + 1, y + 1, z + 1), FaceUV.Right);

            // Левая (X-)
            ItemType leftBlock = _world.GetBlock(wx - 1, wy, wz);
            if (ShouldRenderFace(type, leftBlock))
                AddFace(verts, uvs, norms, cols, type, 2, new Vector3(-1, 0, 0), ColX,
                    new Vector3(x, y, z), new Vector3(x, y, z + 1),
                    new Vector3(x, y + 1, z + 1), new Vector3(x, y + 1, z), FaceUV.Left);
        }

        if (verts.Count == 0) { _hasMesh = false; return; }

        float[] vArr = verts.ToArray();
        float[] uvArr = uvs.ToArray();
        float[] nArr = norms.ToArray();
        byte[] cArr = cols.ToArray();

        Mesh mesh = new()
        {
            VertexCount = vArr.Length / 3,
            TriangleCount = (vArr.Length / 3) / 3
        };

        unsafe
        {
            mesh.Vertices = (float*)Raylib.MemAlloc((uint)(vArr.Length * sizeof(float)));
            Marshal.Copy(vArr, 0, (IntPtr)mesh.Vertices, vArr.Length);

            mesh.TexCoords = (float*)Raylib.MemAlloc((uint)(uvArr.Length * sizeof(float)));
            Marshal.Copy(uvArr, 0, (IntPtr)mesh.TexCoords, uvArr.Length);

            mesh.Normals = (float*)Raylib.MemAlloc((uint)(nArr.Length * sizeof(float)));
            Marshal.Copy(nArr, 0, (IntPtr)mesh.Normals, nArr.Length);

            mesh.Colors = (byte*)Raylib.MemAlloc((uint)(cArr.Length * sizeof(byte)));
            Marshal.Copy(cArr, 0, (IntPtr)mesh.Colors, cArr.Length);
        }

        Raylib.UploadMesh(ref mesh, false);
        _model = Raylib.LoadModelFromMesh(mesh);

        if (TextureAtlas.AtlasTexture.Id != 0)
        {
            unsafe
            {
                _model.Materials[0].Maps[(int)MaterialMapIndex.Albedo].Texture = TextureAtlas.AtlasTexture;
            }
        }

        _hasMesh = true;
    }

    private static bool ShouldRenderFace(ItemType current, ItemType neighbor)
    {
        if (neighbor == ItemType.None) return true;
        if (BlockRegistry.IsTransparent(neighbor) && neighbor != current) return true;
        return false;
    }

    private enum FaceUV { Top, Bottom, Front, Back, Right, Left }

    private static void AddFace(List<float> v, List<float> uv, List<float> n, List<byte> col,
        ItemType type, int faceIndex, Vector3 normal, Color color,
        Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, FaceUV faceUV)
    {
        int tile = TextureAtlas.GetTileIndex(type, faceIndex);
        var (u0, v0, u1, v1) = TextureAtlas.GetUVs(tile);

        (float u, float v)[] uvCoords = faceUV switch
        {
            FaceUV.Top => new[] { (u0, v0), (u0, v1), (u1, v1), (u1, v0) },
            FaceUV.Bottom => new[] { (u0, v1), (u0, v0), (u1, v0), (u1, v1) },
            _ => new[] { (u0, v1), (u1, v1), (u1, v0), (u0, v0) }
        };

        Vector3[] corners = { p1, p2, p3, p4 };

        for (int i = 0; i < 3; i++)
        {
            v.Add(corners[i].X); v.Add(corners[i].Y); v.Add(corners[i].Z);
            uv.Add(uvCoords[i].u); uv.Add(uvCoords[i].v);
            n.Add(normal.X); n.Add(normal.Y); n.Add(normal.Z);
            col.Add(color.R); col.Add(color.G); col.Add(color.B); col.Add(color.A);
        }
        foreach (int i in new[] { 0, 2, 3 })
        {
            v.Add(corners[i].X); v.Add(corners[i].Y); v.Add(corners[i].Z);
            uv.Add(uvCoords[i].u); uv.Add(uvCoords[i].v);
            n.Add(normal.X); n.Add(normal.Y); n.Add(normal.Z);
            col.Add(color.R); col.Add(color.G); col.Add(color.B); col.Add(color.A);
        }
    }

    public void Unload()
    {
        if (_hasMesh)
        {
            Raylib.UnloadModel(_model);
            _hasMesh = false;
        }
    }
}