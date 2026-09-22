using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public class RemotePlayer
{
    public int Id;
    public string Name = "Player";
    public PlayerRank Rank = PlayerRank.Player;
    public Vector3 Position;
    public float Yaw;
    public float Pitch;
    public ItemType HeldItem = ItemType.None;
    public ItemType OffHand = ItemType.None;
    public float Health = 20f;
}

public static class NetworkManager
{
    public static bool IsHost { get; private set; } = false;
    public static bool IsConnected { get; private set; } = false;

    private static UdpClient? udpClient;
    private static IPEndPoint? serverEndPoint;
    public static readonly Dictionary<int, RemotePlayer> RemotePlayers = new();
    private static int myPlayerId = 1;
    private static float syncTimer = 0f;

    public static void StartHost(int port = 25565)
    {
        try
        {
            udpClient = new UdpClient(port);
            IsHost = true;
            IsConnected = true;
            myPlayerId = 1;
            Program.AddChatMessage($"[Мультиплеер] Сервер успешно запущен на порту {port}!", Color.Lime);
        }
        catch (Exception ex)
        {
            Program.AddChatMessage($"[Ошибка сети] {ex.Message}", Color.Red);
        }
    }

    public static void ConnectTo(string ip, int port = 25565)
    {
        try
        {
            udpClient = new UdpClient();
            serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
            IsHost = false;
            IsConnected = true;
            myPlayerId = new Random().Next(100, 9999);

            // Отправляем пакет подключения
            SendPacket(new byte[] { 1, (byte)(myPlayerId >> 8), (byte)myPlayerId });
            Program.AddChatMessage($"[Мультиплеер] Подключение к {ip}:{port}...", Color.Gold);
        }
        catch (Exception ex)
        {
            Program.AddChatMessage($"[Ошибка сети] {ex.Message}", Color.Red);
        }
    }

    public static void Update(float dt, Player player, World world)
    {
        if (!IsConnected || udpClient == null) return;

        syncTimer += dt;
        if (syncTimer >= 0.05f) // 20 тиков в секунду
        {
            syncTimer = 0f;
            SendPlayerState(player);
        }

        // Приём сетевых пакетов
        while (udpClient.Available > 0)
        {
            try
            {
                IPEndPoint ep = new(IPAddress.Any, 0);
                byte[] data = udpClient.Receive(ref ep);
                ProcessPacket(data, ep, world);
            }
            catch { break; }
        }
    }

    private static void SendPlayerState(Player player)
    {
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);
        bw.Write((byte)2); // Пакет движения
        bw.Write(myPlayerId);
        bw.Write(player.Position.X);
        bw.Write(player.Position.Y);
        bw.Write(player.Position.Z);
        bw.Write(player.Yaw);
        bw.Write(player.Pitch);
        bw.Write((byte)player.GetSelectedStack().Type);
        bw.Write((byte)player.OffHand.Type);
        bw.Write(player.Health);

        SendPacket(ms.ToArray());
    }

    public static void BroadcastBlockChange(int x, int y, int z, ItemType type)
    {
        if (!IsConnected) return;
        using MemoryStream ms = new();
        using BinaryWriter bw = new(ms);
        bw.Write((byte)3);
        bw.Write(x);
        bw.Write(y);
        bw.Write(z);
        bw.Write((byte)type);
        SendPacket(ms.ToArray());
    }

    private static void SendPacket(byte[] data)
    {
        if (udpClient == null) return;
        if (IsHost)
        {
            // Хост отправляет всем клиентам
        }
        else if (serverEndPoint != null)
        {
            udpClient.Send(data, data.Length, serverEndPoint);
        }
    }

    private static void ProcessPacket(byte[] data, IPEndPoint sender, World world)
    {
        if (data.Length == 0) return;
        using MemoryStream ms = new(data);
        using BinaryReader br = new(ms);

        byte type = br.ReadByte();
        if (type == 2) // Синхронизация игрока
        {
            int id = br.ReadInt32();
            if (id == myPlayerId) return;

            if (!RemotePlayers.TryGetValue(id, out var rp))
            {
                rp = new RemotePlayer { Id = id };
                RemotePlayers[id] = rp;
            }

            rp.Position = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
            rp.Yaw = br.ReadSingle();
            rp.Pitch = br.ReadSingle();
            rp.HeldItem = (ItemType)br.ReadByte();
            rp.OffHand = (ItemType)br.ReadByte();
            rp.Health = br.ReadSingle();
        }
        else if (type == 3) // Изменение блока
        {
            int bx = br.ReadInt32();
            int by = br.ReadInt32();
            int bz = br.ReadInt32();
            ItemType bt = (ItemType)br.ReadByte();
            world.SetBlock(bx, by, bz, bt);
        }
    }

    public static void RenderRemotePlayers()
    {
        foreach (var p in RemotePlayers.Values)
        {
            Raylib.DrawCube(p.Position + new Vector3(0, 0.9f, 0), 0.6f, 1.8f, 0.6f, new Color(80, 140, 220, 255));
            Raylib.DrawCubeWires(p.Position + new Vector3(0, 0.9f, 0), 0.6f, 1.8f, 0.6f, Color.DarkBlue);

            // Отрисовка предмета в руке удаленного игрока
            if (p.HeldItem != ItemType.None)
            {
                Raylib.DrawCube(p.Position + new Vector3(0.4f, 0.8f, 0.4f), 0.25f, 0.25f, 0.25f, Color.Gold);
            }
        }
    }
}