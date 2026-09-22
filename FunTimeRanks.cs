using Raylib_cs;

namespace FunTimeCobra;

public enum PlayerRank
{
    Player = 0,
    VIP = 1,
    Hero = 2,
    Titan = 3,
    Dragon = 4,
    Sponsor = 5,
    Magister = 6,
    Cobra = 7
}

public static class RankSystem
{
    public static string GetPrefix(PlayerRank rank) => rank switch
    {
        PlayerRank.Player => "§7[ИГРОК]",
        PlayerRank.VIP => "§a[VIP]",
        PlayerRank.Hero => "§b[HERO]",
        PlayerRank.Titan => "§9[TITAN]",
        PlayerRank.Dragon => "§c[DRAGON]",
        PlayerRank.Sponsor => "§e[СПОНСОР]",
        PlayerRank.Magister => "§d[МАГИСТР]",
        PlayerRank.Cobra => "§6§l[COBRA]",
        _ => "§7[ИГРОК]"
    };

    public static Color GetColor(PlayerRank rank) => rank switch
    {
        PlayerRank.Player => Color.LightGray,
        PlayerRank.VIP => new Color(80, 230, 80, 255),
        PlayerRank.Hero => new Color(60, 200, 255, 255),
        PlayerRank.Titan => new Color(50, 100, 255, 255),
        PlayerRank.Dragon => new Color(255, 60, 60, 255),
        PlayerRank.Sponsor => new Color(255, 215, 0, 255),
        PlayerRank.Magister => new Color(220, 80, 255, 255),
        PlayerRank.Cobra => new Color(255, 170, 0, 255),
        _ => Color.White
    };

    public static void GiveKit(Player player, string kitName)
    {
        kitName = kitName.ToLower();
        if (kitName == "starter")
        {
            player.TryAddItem(ItemType.StoneBricks, 64);
            player.TryAddItem(ItemType.Log, 32);
            player.TryAddItem(ItemType.GoldenApple, 2);
            player.TryAddItem(ItemType.EnderPearl, 4);
            Program.AddChatMessage("[FunTime] » Вы получили набор /kit starter!", Color.Lime);
        }
        else if (kitName == "hero")
        {
            player.TryAddItem(ItemType.Obsidian, 64);
            player.TryAddItem(ItemType.GoldenApple, 8);
            player.TryAddItem(ItemType.EnderPearl, 16);
            player.TryAddItem(ItemType.SphereAres, 1);
            player.TryAddItem(ItemType.TalismanHarmony, 1);
            Program.AddChatMessage("[FunTime] » Вы получили набор /kit hero!", Color.Lime);
        }
        else if (kitName == "sponsor")
        {
            player.TryAddItem(ItemType.Obsidian, 128);
            player.TryAddItem(ItemType.GoldenApple, 16);
            player.TryAddItem(ItemType.EnderPearl, 32);
            player.TryAddItem(ItemType.SphereScythian, 1);
            player.TryAddItem(ItemType.SphereAres, 1);
            player.TryAddItem(ItemType.TalismanCrusher, 1);
            player.TryAddItem(ItemType.TalismanPhoenix, 1);
            player.TryAddItem(ItemType.MysticSummon, 1);
            Program.AddChatMessage("[FunTime] » Вы получили набор /kit sponsor!", Color.Gold);
        }
        else if (kitName == "cobra")
        {
            player.TryAddItem(ItemType.Obsidian, 256);
            player.TryAddItem(ItemType.GoldenApple, 32);
            player.TryAddItem(ItemType.EnderPearl, 64);
            player.TryAddItem(ItemType.SphereChaos, 1);
            player.TryAddItem(ItemType.SphereEris, 1);
            player.TryAddItem(ItemType.TalismanCobra, 1);
            player.TryAddItem(ItemType.TalismanArchangel, 1);
            player.TryAddItem(ItemType.TalismanPhoenix, 2);
            player.TryAddItem(ItemType.MysticSummon, 2);
            Program.AddChatMessage("[FunTime] » Вы получили набор /kit cobra!", Color.Orange);
        }
        else
        {
            Program.AddChatMessage("[Ошибка] Доступные наборы: starter, hero, sponsor, cobra", Color.Red);
        }
    }
}