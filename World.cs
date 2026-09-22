using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public class MysticEvent
{
    public Vector3 Position;
    public float TimeLeft = 60f;
    public bool IsActive = false;
    public bool IsOpened = false;
    public ItemRarity LootTier = ItemRarity.Legendary;
}

public class World
{
    public const int ChunkSize = 16;
    public const int WorldHeightChunks = 4;
    public const int TotalHeight = ChunkSize * WorldHeightChunks;

    private readonly Dictionary<ChunkPos, Chunk> _activeChunks = new();
    public readonly NPCManager NpcManager = new();

    public Chest? SpawnChest;
    public Chest? ActiveMysticChest;
    public MysticEvent Mystic = new();
    public Vector3 SpawnPosition = new(0.5f, 34.0f, 0.5f);

    public const float SafeZoneRadius = 26.0f;

    public World()
    {
        NpcManager.SetupSpawnNPCs(new Vector3(0, 32, 0));
        // Сначала генерируем чанки в зоне спавна, затем строим платформу
        UpdateChunkStreaming(SpawnPosition, 64f);
        BuildSpawnStructure(0, 0, 32);
    }

    public bool IsInSafeZone(Vector3 pos)
    {
        float dX = pos.X - 0.5f;
        float dZ = pos.Z - 0.5f;
        return (dX * dX + dZ * dZ) <= (SafeZoneRadius * SafeZoneRadius);
    }

    public void UpdateChunkStreaming(Vector3 playerPos, float renderDist)
    {
        int pCX = (int)MathF.Floor(playerPos.X / ChunkSize);
        int pCZ = (int)MathF.Floor(playerPos.Z / ChunkSize);
        int rad = (int)MathF.Ceiling(renderDist / ChunkSize) + 1;

        HashSet<ChunkPos> needed = new();

        for (int cx = pCX - rad; cx <= pCX + rad; cx++)
        for (int cz = pCZ - rad; cz <= pCZ + rad; cz++)
        {
            float dist = Vector2.Distance(new Vector2(cx * ChunkSize, cz * ChunkSize), new Vector2(playerPos.X, playerPos.Z));
            if (dist > renderDist + ChunkSize) continue;

            for (int cy = 0; cy < WorldHeightChunks; cy++)
            {
                ChunkPos pos = new(cx, cy, cz);
                needed.Add(pos);

                if (!_activeChunks.ContainsKey(pos))
                {
                    Chunk c = new(this, cx, cy, cz);
                    GenerateChunkTerrain(c);
                    _activeChunks[pos] = c;
                }
            }
        }

        List<ChunkPos> toRemove = new();
        foreach (var kvp in _activeChunks)
        {
            if (!needed.Contains(kvp.Key))
            {
                kvp.Value.Unload();
                toRemove.Add(kvp.Key);
            }
        }
        foreach (var p in toRemove) _activeChunks.Remove(p);
    }

    public void Update(float dt)
    {
        if (Mystic.IsActive && !Mystic.IsOpened)
        {
            Mystic.TimeLeft -= dt;
            ParticleSystem.SpawnBeaconBeam(Mystic.Position, Mystic.LootTier == ItemRarity.Mythic ? Color.Magenta : Color.Gold);

            if (Mystic.TimeLeft <= 0)
            {
                Mystic.IsOpened = true;
                ActiveMysticChest = Chest.GenerateMysticLoot(Mystic.Position, Mystic.LootTier);
                SetBlock((int)Mystic.Position.X, (int)Mystic.Position.Y, (int)Mystic.Position.Z, ItemType.MysticChest);
                SoundManager.PlayMystic();
                Program.AddChatMessage($"[FunTime] » §6§lМистический Сундук открылся!§r Забирайте топовый лут на X: {(int)Mystic.Position.X} Z: {(int)Mystic.Position.Z}!", Color.Gold);
            }
        }
    }

    public void StartMystic(Vector3 pos, ItemRarity tier = ItemRarity.Legendary)
    {
        Mystic.Position = pos;
        Mystic.TimeLeft = 60f;
        Mystic.IsActive = true;
        Mystic.IsOpened = false;
        Mystic.LootTier = tier;

        SetBlock((int)pos.X, (int)pos.Y, (int)pos.Z, ItemType.Glowstone);
        SoundManager.PlayMystic();
        Program.AddChatMessage($"[FunTime] » §e§lМистик [{tier}]§r упал на X: {(int)pos.X}, Z: {(int)pos.Z}! Открытие через 01:00!", Color.Gold);
    }

    public ItemType GetBlock(int x, int y, int z)
    {
        if (y < 0 || y >= TotalHeight) return ItemType.None;
        int cx = (int)MathF.Floor((float)x / ChunkSize);
        int cy = y / ChunkSize;
        int cz = (int)MathF.Floor((float)z / ChunkSize);

        ChunkPos cp = new(cx, cy, cz);
        if (_activeChunks.TryGetValue(cp, out var chunk))
        {
            int lx = x - cx * ChunkSize;
            int ly = y - cy * ChunkSize;
            int lz = z - cz * ChunkSize;
            return chunk.GetLocal(lx, ly, lz);
        }
        return ItemType.None;
    }

    public bool IsSolid(int x, int y, int z)
    {
        if (y < 0) return true;
        if (y >= TotalHeight) return false;
        return GetBlock(x, y, z) != ItemType.None;
    }

    public void SetBlock(int x, int y, int z, ItemType type)
    {
        if (y < 0 || y >= TotalHeight) return;
        int cx = (int)MathF.Floor((float)x / ChunkSize);
        int cy = y / ChunkSize;
        int cz = (int)MathF.Floor((float)z / ChunkSize);

        ChunkPos cp = new(cx, cy, cz);
        if (!_activeChunks.TryGetValue(cp, out var chunk))
        {
            chunk = new Chunk(this, cx, cy, cz);
            _activeChunks[cp] = chunk;
        }

        int lx = x - cx * ChunkSize;
        int ly = y - cy * ChunkSize;
        int lz = z - cz * ChunkSize;
        chunk.SetLocal(lx, ly, lz, type);

        if (lx == 0) MarkChunk(cx - 1, cy, cz);
        if (lx == ChunkSize - 1) MarkChunk(cx + 1, cy, cz);
        if (ly == 0) MarkChunk(cx, cy - 1, cz);
        if (ly == ChunkSize - 1) MarkChunk(cx, cy + 1, cz);
        if (lz == 0) MarkChunk(cx, cy, cz - 1);
        if (lz == ChunkSize - 1) MarkChunk(cx, cy, cz + 1);
    }

    private void MarkChunk(int cx, int cy, int cz)
    {
        if (_activeChunks.TryGetValue(new ChunkPos(cx, cy, cz), out var c))
            c.MarkDirty();
    }

    public int GetSurfaceHeight(int x, int z)
    {
        for (int y = TotalHeight - 2; y >= 1; y--)
        {
            if (GetBlock(x, y, z) != ItemType.None) return y;
        }
        return 32;
    }

    public Vector3 GetRandomSafeLocation()
    {
        Random rand = new();
        for (int attempt = 0; attempt < 40; attempt++)
        {
            int rx = rand.Next(-300, 300);
            int rz = rand.Next(-300, 300);
            if (Math.Abs(rx) < 40 && Math.Abs(rz) < 40) continue;

            int ry = GetSurfaceHeight(rx, rz);
            if (ry > 10 && ry < TotalHeight - 6)
            {
                return new Vector3(rx + 0.5f, ry + 1.2f, rz + 0.5f);
            }
        }
        return SpawnPosition;
    }

    public void Render(Vector3 camPos, float renderDist)
    {
        foreach (var c in _activeChunks.Values)
        {
            c.Render(camPos, renderDist);
        }
        NpcManager.RenderAll(camPos);
    }

    public bool Raycast(Vector3 origin, Vector3 dir, float maxDist, out Vector3Int hit, out Vector3Int normal)
    {
        hit = default;
        normal = default;
        dir = Vector3.Normalize(dir);

        int x = (int)MathF.Floor(origin.X);
        int y = (int)MathF.Floor(origin.Y);
        int z = (int)MathF.Floor(origin.Z);

        int stepX = dir.X > 0 ? 1 : (dir.X < 0 ? -1 : 0);
        int stepY = dir.Y > 0 ? 1 : (dir.Y < 0 ? -1 : 0);
        int stepZ = dir.Z > 0 ? 1 : (dir.Z < 0 ? -1 : 0);

        float tDeltaX = stepX != 0 ? MathF.Abs(1f / dir.X) : float.MaxValue;
        float tDeltaY = stepY != 0 ? MathF.Abs(1f / dir.Y) : float.MaxValue;
        float tDeltaZ = stepZ != 0 ? MathF.Abs(1f / dir.Z) : float.MaxValue;

        float tMaxX = stepX != 0 ? (stepX > 0 ? (x + 1 - origin.X) : (origin.X - x)) / MathF.Abs(dir.X) : float.MaxValue;
        float tMaxY = stepY != 0 ? (stepY > 0 ? (y + 1 - origin.Y) : (origin.Y - y)) / MathF.Abs(dir.Y) : float.MaxValue;
        float tMaxZ = stepZ != 0 ? (stepZ > 0 ? (z + 1 - origin.Z) : (origin.Z - z)) / MathF.Abs(dir.Z) : float.MaxValue;

        Vector3Int n = default;
        float t = 0;

        for (int i = 0; i < 512 && t <= maxDist; i++)
        {
            if (y < 0 || y >= TotalHeight) break;
            if (IsSolid(x, y, z))
            {
                hit = new Vector3Int(x, y, z);
                normal = n;
                return true;
            }

            if (tMaxX < tMaxY && tMaxX < tMaxZ)
            {
                x += stepX; t = tMaxX; tMaxX += tDeltaX; n = new Vector3Int(-stepX, 0, 0);
            }
            else if (tMaxY < tMaxZ)
            {
                y += stepY; t = tMaxY; tMaxY += tDeltaY; n = new Vector3Int(0, -stepY, 0);
            }
            else
            {
                z += stepZ; t = tMaxZ; tMaxZ += tDeltaZ; n = new Vector3Int(0, 0, -stepZ);
            }
        }
        return false;
    }

    private void GenerateChunkTerrain(Chunk chunk)
    {
        int startX = chunk.CX * ChunkSize;
        int startY = chunk.CY * ChunkSize;
        int startZ = chunk.CZ * ChunkSize;

        Random rnd = new(chunk.CX * 31337 + chunk.CZ * 101399 + chunk.CY);

        for (int x = 0; x < ChunkSize; x++)
        for (int z = 0; z < ChunkSize; z++)
        {
            int wx = startX + x;
            int wz = startZ + z;

            if (Math.Abs(wx) <= 24 && Math.Abs(wz) <= 24) continue;

            float n1 = SimpleNoise.Calc2D(wx * 0.015f, wz * 0.015f);
            float n2 = SimpleNoise.Calc2D(wx * 0.045f, wz * 0.045f);
            int h = (int)(28 + n1 * 18 + n2 * 6);
            h = Math.Clamp(h, 4, TotalHeight - 6);

            for (int y = 0; y < ChunkSize; y++)
            {
                int wy = startY + y;
                if (wy > h) continue;

                ItemType t = ItemType.Stone;
                if (wy == 0) t = ItemType.Bedrock;
                else if (wy == h) t = (h > 46) ? ItemType.Snow : ItemType.Grass;
                else if (wy > h - 4) t = ItemType.Dirt;
                else
                {
                    double r = rnd.NextDouble();
                    if (wy < 14 && r < 0.007) t = ItemType.DiamondOre;
                    else if (wy < 10 && r < 0.003) t = ItemType.NetheriteOre;
                    else if (wy < 22 && r < 0.009) t = ItemType.EmeraldOre;
                    else if (wy < 28 && r < 0.014) t = ItemType.GoldOre;
                    else if (wy < 32 && r < 0.020) t = ItemType.RedstoneOre;
                    else if (wy < 42 && r < 0.035) t = ItemType.IronOre;
                    else if (r < 0.045) t = ItemType.CoalOre;
                }

                chunk.SetLocal(x, y, z, t);
            }
        }
    }

    private void BuildSpawnStructure(int cx, int cz, int baseY)
    {
        int r = 16;
        for (int dx = -r; dx <= r; dx++)
        for (int dz = -r; dz <= r; dz++)
        {
            int wx = cx + dx;
            int wz = cz + dz;
            int distSq = dx * dx + dz * dz;

            if (distSq <= r * r)
            {
                ItemType floor = (distSq >= (r - 2) * (r - 2)) ? ItemType.Obsidian :
                                 ((Math.Abs(dx) % 4 == 0 && Math.Abs(dz) % 4 == 0) ? ItemType.Glowstone : ItemType.StoneBricks);
                SetBlock(wx, baseY, wz, floor);

                for (int y = baseY + 1; y <= baseY + 16; y++)
                {
                    SetBlock(wx, y, wz, ItemType.None);
                }
            }
        }

        SetBlock(cx, baseY + 1, cz, ItemType.Beacon);
        SetBlock(cx, baseY + 2, cz, ItemType.Chest);
        SpawnChest = new Chest(new Vector3(cx, baseY + 2, cz));
        SpawnChest.Inventory[0] = new ItemStack(ItemType.DiamondSword, 1);
        SpawnChest.Inventory[1] = new ItemStack(ItemType.GoldenApple, 16);
        SpawnChest.Inventory[2] = new ItemStack(ItemType.EnderPearl, 16);
        SpawnChest.Inventory[3] = new ItemStack(ItemType.Obsidian, 64);
    }
}