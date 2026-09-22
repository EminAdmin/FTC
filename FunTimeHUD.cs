using System;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public static class FunTimeHUD
{
    public static void Render(Player player, World world, int sw, int sh)
    {
        DrawItemLabelTopLeft(player);
        DrawSafeZoneOrCombatTag(player, world, sw, sh);
        DrawMinecraftStatusBars(player, sw, sh);
        DrawHotbar(player, sw, sh);
        DrawScoreboard(player, world, sw);
    }

    private static void DrawItemLabelTopLeft(Player player)
    {
        ItemStack held = player.GetSelectedStack();
        if (held.IsEmpty) return;

        string name = BlockRegistry.GetName(held.Type);
        string extraInfo = held.Type switch
        {
            ItemType.SphereAres => "Сила III (+75% к атаке)",
            ItemType.SphereScythian => "Скорость IV (+105% к скорости)",
            ItemType.SphereChaos => "Сингулярность (Баланс характеристик)",
            ItemType.TalismanCobra => "Ярость Кобры (+50% Скорость, +Урон)",
            ItemType.TalismanPhoenix => "Защита от смерти / Тотем",
            ItemType.TalismanArchangel => "+14 HP / Сверхрегенерация",
            ItemType.TrapBoxCobra => "Мгновенная обсидиановая ловушка",
            _ => $"Количество: {held.Count}"
        };

        int bx = 16, by = 16, bw = 280, bh = 54;
        Raylib.DrawRectangle(bx, by, bw, bh, new Color(15, 15, 20, 220));
        Raylib.DrawRectangleLines(bx, by, bw, bh, Color.Gold);

        Program.DrawItemIcon(held, bx + 10, by + 10, 34);
        Program.DrawTextCyr(name, bx + 52, by + 9, 15, Color.Gold);
        Program.DrawTextCyr(extraInfo, bx + 52, by + 30, 12, Color.LightGray);
    }

    private static void DrawSafeZoneOrCombatTag(Player player, World world, int sw, int sh)
    {
        if (world.IsInSafeZone(player.Position))
        {
            string txt = "✔ Безопасная зона: Спавн";
            int tw = (int)Program.MeasureTextCyr(txt, 15);
            Raylib.DrawRectangle(sw / 2 - tw / 2 - 8, sh - 105, tw + 16, 22, new Color(10, 30, 10, 200));
            Raylib.DrawRectangleLines(sw / 2 - tw / 2 - 8, sh - 105, tw + 16, 22, Color.Lime);
            Program.DrawTextCyr(txt, sw / 2 - tw / 2, sh - 102, 15, Color.Lime);
        }
        else if (player.CombatTagTimer > 0)
        {
            string txt = $"⚔ РЕЖИМ БОЯ: {player.CombatTagTimer:F1}с";
            int tw = (int)Program.MeasureTextCyr(txt, 15);
            Raylib.DrawRectangle(sw / 2 - tw / 2 - 8, sh - 105, tw + 16, 22, new Color(40, 10, 10, 200));
            Raylib.DrawRectangleLines(sw / 2 - tw / 2 - 8, sh - 105, tw + 16, 22, Color.Red);
            Program.DrawTextCyr(txt, sw / 2 - tw / 2, sh - 102, 15, Color.Red);
        }
    }

    private static void DrawMinecraftStatusBars(Player player, int sw, int sh)
    {
        int hs = 11, hy = sh - 75;
        int heartStartX = sw / 2 - 102;

        int totalHearts = (int)MathF.Ceiling(player.MaxHealth / 2f);
        for (int i = 0; i < totalHearts; i++)
        {
            int hx = heartStartX + (i % 10) * (hs + 2);
            int rowY = hy - (i / 10) * 12;

            Raylib.DrawRectangle(hx, rowY, hs, hs, new Color(20, 20, 20, 220));
            float hp = player.Health - i * 2;
            if (hp >= 2) Raylib.DrawRectangle(hx + 1, rowY + 1, hs - 2, hs - 2, new Color(220, 20, 30, 255));
            else if (hp >= 1) Raylib.DrawRectangle(hx + 1, rowY + 1, (hs - 2) / 2, hs - 2, new Color(220, 20, 30, 255));
            Raylib.DrawRectangleLines(hx, rowY, hs, hs, Color.DarkGray);
        }

        if (player.AbsorptionHealth > 0)
        {
            int absHearts = (int)MathF.Ceiling(player.AbsorptionHealth / 2f);
            int absY = hy - ((totalHearts + 9) / 10) * 12;
            for (int i = 0; i < absHearts; i++)
            {
                int ax = heartStartX + (i % 10) * (hs + 2);
                int rowY = absY - (i / 10) * 12;
                Raylib.DrawRectangle(ax, rowY, hs, hs, Color.Gold);
                Raylib.DrawRectangleLines(ax, rowY, hs, hs, new Color(180, 140, 20, 255));
            }
        }

        int hungerStartX = sw / 2 + 10;
        for (int i = 0; i < 10; i++)
        {
            int fx = hungerStartX + i * (hs + 2);
            Raylib.DrawRectangle(fx, hy, hs, hs, new Color(20, 20, 20, 220));
            Raylib.DrawRectangle(fx + 1, hy + 1, hs - 2, hs - 2, new Color(190, 110, 40, 255));
            Raylib.DrawRectangleLines(fx, hy, hs, hs, Color.DarkGray);
        }

        int expW = 200, expH = 5, expX = sw / 2 - expW / 2, expY = sh - 60;
        Raylib.DrawRectangle(expX, expY, expW, expH, new Color(25, 25, 25, 220));
        Raylib.DrawRectangle(expX + 1, expY + 1, (int)((expW - 2) * player.Exp), expH - 2, Color.Lime);
        string lvl = player.Level.ToString();
        Program.DrawTextCyr(lvl, sw / 2 - Program.MeasureTextCyr(lvl, 13) / 2, expY - 12, 13, Color.Lime);
    }

    private static void DrawHotbar(Player player, int sw, int sh)
    {
        int slotSize = 44;
        int sx = sw / 2 - (9 * slotSize) / 2;
        int sy = sh - 52;

        int ox = sx - 54;
        Raylib.DrawRectangle(ox, sy, slotSize, slotSize, new Color(25, 25, 30, 230));
        Raylib.DrawRectangleLinesEx(new Rectangle(ox, sy, slotSize, slotSize), 2, Color.Gold);
        Program.DrawItemIcon(player.OffHand, ox + 6, sy + 6, 32);
        Program.DrawTextCyr("Off", ox + 4, sy + 2, 10, Color.Gold);

        for (int i = 0; i < 9; i++)
        {
            int x = sx + i * slotSize;
            bool isSelected = (i == player.SelectedSlot);

            Raylib.DrawRectangle(x, sy, slotSize, slotSize, isSelected ? new Color(50, 50, 65, 240) : new Color(30, 30, 35, 220));
            Raylib.DrawRectangleLinesEx(new Rectangle(x, sy, slotSize, slotSize), isSelected ? 3 : 1, isSelected ? Color.White : Color.Gray);

            Program.DrawItemIcon(player.Hotbar[i], x + 6, sy + 6, 32);

            var cd = CooldownSystem.GetCooldownType(player.Hotbar[i].Type);
            if (cd.HasValue && player.Cooldowns.IsOnCooldown(cd.Value))
            {
                float prog = player.Cooldowns.GetProgress(cd.Value);
                int overlayH = (int)(slotSize * prog);
                Raylib.DrawRectangle(x, sy + slotSize - overlayH, slotSize, overlayH, new Color(0, 0, 0, 190));
                string rem = player.Cooldowns.GetRemaining(cd.Value).ToString("F0");
                Program.DrawTextCyr(rem, x + slotSize / 2 - Program.MeasureTextCyr(rem, 14) / 2, sy + slotSize / 2 - 7, 14, Color.Red);
            }

            Program.DrawTextCyr((i + 1).ToString(), x + 3, sy + 2, 10, Color.LightGray);
        }
    }

    private static void DrawScoreboard(Player player, World world, int sw)
    {
        int bw = 190, bh = 220;
        int bx = sw - bw - 12, by = 130;

        Raylib.DrawRectangle(bx, by, bw, bh, new Color(15, 15, 20, 185));
        Raylib.DrawRectangleLines(bx, by, bw, bh, new Color(80, 65, 25, 200));

        Program.DrawTextCyr("§6§l  FUNTIME.SU  ", bx + 30, by + 8, 16, Color.Gold);
        Program.DrawTextCyr("─────────────────────", bx + 6, by + 24, 10, Color.Gray);

        Program.DrawTextCyr($"Профиль: §f{player.Name}", bx + 10, by + 38, 13, Color.White);
        Program.DrawTextCyr("Донат: §6§l[COBRA]", bx + 10, by + 58, 13, Color.Orange);
        Program.DrawTextCyr($"Баланс: §a{player.Balance:N0}$", bx + 10, by + 78, 13, Color.Lime);
        Program.DrawTextCyr($"БП Уровень: §d{player.BP.CurrentLevel} lvl", bx + 10, by + 98, 13, Color.Magenta);

        Program.DrawTextCyr("─────────────────────", bx + 6, by + 116, 10, Color.Gray);

        Program.DrawTextCyr($"XYZ: {(int)player.Position.X} {(int)player.Position.Y} {(int)player.Position.Z}", bx + 10, by + 130, 13, Color.White);
        string mystStatus = world.Mystic.IsActive ? $"§600:{(int)world.Mystic.TimeLeft:D2}" : "§7Нет";
        Program.DrawTextCyr($"Мистик: {mystStatus}", bx + 10, by + 150, 13, Color.Gold);
        Program.DrawTextCyr($"Онлайн: §f{NetworkManager.RemotePlayers.Count + 1}", bx + 10, by + 170, 13, Color.LightGray);
        Program.DrawTextCyr("§eplay.funtime.su", bx + 42, by + 196, 12, Color.Yellow);
    }
}