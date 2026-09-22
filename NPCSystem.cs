using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public enum NPCType
{
    Buyer,       // Скупщик
    Auction,     // Аукцион
    BattlePass,  // Боевой Пропуск
    Quests,      // Задания / Квесты
    Donation     // Донат-магазин
}

public class NPC
{
    public string Name;
    public string Tag;
    public NPCType Type;
    public Vector3 Position;
    public Color SkinColor;

    public NPC(string name, string tag, NPCType type, Vector3 pos, Color skin)
    {
        Name = name;
        Tag = tag;
        Type = type;
        Position = pos;
        SkinColor = skin;
    }

    public void Render(Vector3 camPos)
    {
        // Тело NPC
        Raylib.DrawCube(Position + new Vector3(0, 0.9f, 0), 0.6f, 1.8f, 0.6f, SkinColor);
        Raylib.DrawCubeWires(Position + new Vector3(0, 0.9f, 0), 0.62f, 1.82f, 0.62f, Color.Gold);

        // Голова NPC
        Raylib.DrawCube(Position + new Vector3(0, 2.1f, 0), 0.5f, 0.5f, 0.5f, new Color(220, 180, 140, 255));
        Raylib.DrawCubeWires(Position + new Vector3(0, 2.1f, 0), 0.52f, 0.52f, 0.52f, Color.Black);
    }
}

public class NPCManager
{
    public readonly List<NPC> SpawnNPCs = new();

    public void SetupSpawnNPCs(Vector3 spawnCenter)
    {
        SpawnNPCs.Clear();
        SpawnNPCs.Add(new NPC("§6§lСКУПЩИК РУДЫ", "[ПКМ - Продать лут]", NPCType.Buyer, spawnCenter + new Vector3(-4, 0, -3), new Color(40, 180, 80, 255)));
        SpawnNPCs.Add(new NPC("§e§lАУКЦИОН (/ah)", "[ПКМ - Открыть рынок]", NPCType.Auction, spawnCenter + new Vector3(4, 0, -3), new Color(220, 160, 30, 255)));
        SpawnNPCs.Add(new NPC("§d§lБОЕВОЙ ПРОПУСК", "[ПКМ - Награды БП]", NPCType.BattlePass, spawnCenter + new Vector3(-4, 0, 3), new Color(180, 40, 230, 255)));
        SpawnNPCs.Add(new NPC("§b§lДОНАТ-ШОП", "[ПКМ - Кит / Привилегии]", NPCType.Donation, spawnCenter + new Vector3(4, 0, 3), new Color(30, 140, 240, 255)));
    }

    public void RenderAll(Vector3 camPos)
    {
        foreach (var npc in SpawnNPCs)
        {
            npc.Render(camPos);
        }
    }

    public NPC? CheckInteraction(Vector3 playerPos, Vector3 lookDir, float maxDist = 3.5f)
    {
        foreach (var npc in SpawnNPCs)
        {
            Vector3 center = npc.Position + new Vector3(0, 1.0f, 0);
            if (Vector3.Distance(playerPos, center) <= maxDist)
            {
                return npc;
            }
        }
        return null;
    }
}