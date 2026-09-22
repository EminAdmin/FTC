using System;

namespace FunTimeCobra;

public struct Vector3Int
{
    public int X;
    public int Y;
    public int Z;

    public Vector3Int(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3Int operator +(Vector3Int a, Vector3Int b)
        => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static bool operator ==(Vector3Int a, Vector3Int b)
        => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

    public static bool operator !=(Vector3Int a, Vector3Int b)
        => !(a == b);

    public override bool Equals(object? obj) => obj is Vector3Int v && this == v;
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X}, {Y}, {Z})";
}