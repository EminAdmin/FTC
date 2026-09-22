using System;
using System.Collections.Generic;
using System.Numerics;

namespace FunTimeCobra;

public struct BoxAABB
{
    public Vector3 Min;
    public Vector3 Max;

    public BoxAABB(Vector3 min, Vector3 max)
    {
        Min = min;
        Max = max;
    }

    public static BoxAABB FromCenterSize(Vector3 center, Vector3 size)
    {
        Vector3 half = size * 0.5f;
        return new BoxAABB(center - half, center + half);
    }

    public static BoxAABB FromPlayer(Vector3 pos, float width = 0.6f, float height = 1.8f)
    {
        float hw = width * 0.5f;
        return new BoxAABB(new Vector3(pos.X - hw, pos.Y, pos.Z - hw), new Vector3(pos.X + hw, pos.Y + height, pos.Z + hw));
    }

    public readonly bool Intersects(BoxAABB o)
    {
        return Min.X < o.Max.X && Max.X > o.Min.X &&
               Min.Y < o.Max.Y && Max.Y > o.Min.Y &&
               Min.Z < o.Max.Z && Max.Z > o.Min.Z;
    }

    public readonly BoxAABB Expand(Vector3 vel)
    {
        Vector3 min = Min;
        Vector3 max = Max;

        if (vel.X < 0) min.X += vel.X;
        if (vel.X > 0) max.X += vel.X;
        if (vel.Y < 0) min.Y += vel.Y;
        if (vel.Y > 0) max.Y += vel.Y;
        if (vel.Z < 0) min.Z += vel.Z;
        if (vel.Z > 0) max.Z += vel.Z;

        return new BoxAABB(min, max);
    }

    public readonly float ClipX(BoxAABB o, float dx)
    {
        if (o.Max.Y <= Min.Y || o.Min.Y >= Max.Y) return dx;
        if (o.Max.Z <= Min.Z || o.Min.Z >= Max.Z) return dx;

        if (dx > 0.0f && o.Max.X <= Min.X)
        {
            float maxDx = Min.X - o.Max.X;
            if (maxDx < dx) dx = maxDx;
        }
        else if (dx < 0.0f && o.Min.X >= Max.X)
        {
            float maxDx = Max.X - o.Min.X;
            if (maxDx > dx) dx = maxDx;
        }
        return dx;
    }

    public readonly float ClipY(BoxAABB o, float dy)
    {
        if (o.Max.X <= Min.X || o.Min.X >= Max.X) return dy;
        if (o.Max.Z <= Min.Z || o.Min.Z >= Max.Z) return dy;

        if (dy > 0.0f && o.Max.Y <= Min.Y)
        {
            float maxDy = Min.Y - o.Max.Y;
            if (maxDy < dy) dy = maxDy;
        }
        else if (dy < 0.0f && o.Min.Y >= Max.Y)
        {
            float maxDy = Max.Y - o.Min.Y;
            if (maxDy > dy) dy = maxDy;
        }
        return dy;
    }

    public readonly float ClipZ(BoxAABB o, float dz)
    {
        if (o.Max.X <= Min.X || o.Min.X >= Max.X) return dz;
        if (o.Max.Y <= Min.Y || o.Min.Y >= Max.Y) return dz;

        if (dz > 0.0f && o.Max.Z <= Min.Z)
        {
            float maxDz = Min.Z - o.Max.Z;
            if (maxDz < dz) dz = maxDz;
        }
        else if (dz < 0.0f && o.Min.Z >= Max.Z)
        {
            float maxDz = Max.Z - o.Min.Z;
            if (maxDz > dz) dz = maxDz;
        }
        return dz;
    }
}

public static class PhysicsEngine
{
    private const float StepHeight = 0.5f;

    public static void MovePlayer(Player player, World world, float dt)
    {
        if (player.IsFlying)
        {
            player.Position += player.Velocity * dt;
            player.IsGrounded = false;
            return;
        }

        // 1. Приоритетное выталкивание при застревании (Depenetration / Anti-Stuck)
        ResolveStuckState(player, world);

        Vector3 moveDelta = player.Velocity * dt;
        BoxAABB playerBox = BoxAABB.FromPlayer(player.Position);
        BoxAABB broadphase = playerBox.Expand(moveDelta);

        List<BoxAABB> potentialBlocks = GetSurroundingBoxes(world, broadphase);

        float originalDx = moveDelta.X;
        float originalDy = moveDelta.Y;
        float originalDz = moveDelta.Z;

        // 2. Раздельное перемещение по осям (Minecraft Standard Axis Separation)
        // Ось Y (Вертикаль)
        foreach (var b in potentialBlocks)
        {
            moveDelta.Y = b.ClipY(playerBox, moveDelta.Y);
        }
        playerBox.Min.Y += moveDelta.Y;
        playerBox.Max.Y += moveDelta.Y;

        // Ось X
        foreach (var b in potentialBlocks)
        {
            moveDelta.X = b.ClipX(playerBox, moveDelta.X);
        }
        playerBox.Min.X += moveDelta.X;
        playerBox.Max.X += moveDelta.X;

        // Ось Z
        foreach (var b in potentialBlocks)
        {
            moveDelta.Z = b.ClipZ(playerBox, moveDelta.Z);
        }
        playerBox.Min.Z += moveDelta.Z;
        playerBox.Max.Z += moveDelta.Z;

        // 3. Вычисление флагов касания и сброс импульса
        player.IsGrounded = originalDy < 0.0f && MathF.Abs(moveDelta.Y - originalDy) > 0.0001f;
        bool hitCeiling = originalDy > 0.0f && MathF.Abs(moveDelta.Y - originalDy) > 0.0001f;

        if (player.IsGrounded || hitCeiling) player.Velocity.Y = 0.0f;
        if (MathF.Abs(moveDelta.X - originalDx) > 0.0001f) player.Velocity.X = 0.0f;
        if (MathF.Abs(moveDelta.Z - originalDz) > 0.0001f) player.Velocity.Z = 0.0f;

        player.Position = new Vector3(
            (playerBox.Min.X + playerBox.Max.X) * 0.5f,
            playerBox.Min.Y,
            (playerBox.Min.Z + playerBox.Max.Z) * 0.5f
        );
    }

    private static void ResolveStuckState(Player player, World world)
    {
        BoxAABB box = BoxAABB.FromPlayer(player.Position);
        List<BoxAABB> intersecting = GetSurroundingBoxes(world, box);

        bool stuck = false;
        foreach (var b in intersecting)
        {
            if (box.Intersects(b))
            {
                stuck = true;
                break;
            }
        }

        if (!stuck) return;

        // Поиск ближайшего свободного пространства сверху
        int startX = (int)MathF.Floor(player.Position.X);
        int startY = (int)MathF.Floor(player.Position.Y);
        int startZ = (int)MathF.Floor(player.Position.Z);

        for (int yOffset = 1; yOffset <= 12; yOffset++)
        {
            int checkY = startY + yOffset;
            if (!world.IsSolid(startX, checkY, startZ) && !world.IsSolid(startX, checkY + 1, startZ))
            {
                player.Position.Y = checkY + 0.02f;
                player.Velocity = Vector3.Zero;
                return;
            }
        }
    }

    private static List<BoxAABB> GetSurroundingBoxes(World world, BoxAABB broadphase)
    {
        List<BoxAABB> boxes = new(64);
        int minX = (int)MathF.Floor(broadphase.Min.X) - 1;
        int maxX = (int)MathF.Floor(broadphase.Max.X) + 1;
        int minY = Math.Max(0, (int)MathF.Floor(broadphase.Min.Y) - 1);
        int maxY = Math.Min(World.TotalHeight - 1, (int)MathF.Floor(broadphase.Max.Y) + 1);
        int minZ = (int)MathF.Floor(broadphase.Min.Z) - 1;
        int maxZ = (int)MathF.Floor(broadphase.Max.Z) + 1;

        for (int x = minX; x <= maxX; x++)
        for (int y = minY; y <= maxY; y++)
        for (int z = minZ; z <= maxZ; z++)
        {
            ItemType block = world.GetBlock(x, y, z);
            if (BlockRegistry.IsSolid(block))
            {
                boxes.Add(new BoxAABB(new Vector3(x, y, z), new Vector3(x + 1, y + 1, z + 1)));
            }
        }
        return boxes;
    }
}