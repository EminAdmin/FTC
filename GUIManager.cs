using System;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public enum GUIState
{
    None,
    Kits,
    BattlePass,
    Crafting,
    Auction,
    Buyer,
    Donate
}

public static class GUIManager
{
    public static GUIState CurrentState = GUIState.None;
    public static readonly ItemStack[] CraftingGrid = new ItemStack[9];
    public static ItemStack CraftResult = new();

    public static void Open(GUIState state, Player player)
    {
        CurrentState = state;
        player.IsGUIOpen = true;
        Raylib.EnableCursor();
        SoundManager.PlayPlace();
    }

    public static void Close(Player player)
    {
        CurrentState = GUIState.None;
        player.IsGUIOpen = false;

        for (int i = 0; i < 9; i++)
        {
            if (!CraftingGrid[i].IsEmpty)
            {
                player.TryAddItem(CraftingGrid[i].Type, CraftingGrid[i].Count);
                CraftingGrid[i] = new ItemStack();
            }
        }
        CraftResult = new ItemStack();

        if (!player.IsInventoryOpen && !player.IsChestOpen)
            Raylib.DisableCursor();
    }

    public static void UpdateAndDraw(Player player, World world, int sw, int sh, ref ItemStack draggedSlot)
    {
        if (CurrentState == GUIState.None) return;

        Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 195));

        switch (CurrentState)
        {
            case GUIState.Kits:
                DrawKitsGUI(player, sw, sh);
                break;
            case GUIState.BattlePass:
                DrawBattlePassGUI(player, sw, sh);
                break;
            case GUIState.Crafting:
                DrawCraftingGUI(player, sw, sh, ref draggedSlot);
                break;
            case GUIState.Buyer:
                DrawBuyerGUI(player, sw, sh);
                break;
            case GUIState.Auction:
                DrawAuctionGUI(player, sw, sh);
                break;
            case GUIState.Donate:
                DrawDonateGUI(player, sw, sh);
                break;
        }

        int closeX = sw / 2 + 250, closeY = sh / 2 - 200;
        if (DrawButton(closeX, closeY, 32, 32, "X", new Color(180, 40, 40, 255), new Color(220, 50, 50, 255)))
        {
            Close(player);
        }
    }

    private static void DrawKitsGUI(Player player, int sw, int sh)
    {
        int cx = sw / 2, cy = sh / 2;
        int pw = 520, ph = 380;
        Rectangle panel = new(cx - pw / 2, cy - ph / 2, pw, ph);

        Raylib.DrawRectangleRec(panel, new Color(25, 25, 30, 245));
        Raylib.DrawRectangleLinesEx(panel, 2, Color.Gold);

        Program.DrawTextCyr("ДОСТУПНЫЕ НАБОРЫ (/kit)", cx - Program.MeasureTextCyr("ДОСТУПНЫЕ НАБОРЫ (/kit)", 22) / 2, panel.Y + 20, 22, Color.Gold);

        string[] kits = { "starter", "hero", "sponsor", "cobra" };
        string[] titles = { "§7Набор: Игрок", "§bНабор: Hero", "§eНабор: Sponsor", "§6§lНабор: COBRA" };
        string[] descs = {
            "Каменные блоки, дуб, 2 золотых яблока, перлы",
            "Обсидиан, 8 яблок, перлы, Сфера Ареса, Гармония",
            "128 обсидиана, 16 яблок, Скиф, Крушитель, Мистик",
            "256 обсидиана, 32 чарки, Хаос, Талисман Кобры, Архангел"
        };

        for (int i = 0; i < 4; i++)
        {
            int rowY = (int)panel.Y + 70 + i * 70;
            Rectangle r = new(panel.X + 20, rowY, panel.Width - 40, 60);
            Raylib.DrawRectangleRec(r, new Color(40, 40, 50, 220));
            Raylib.DrawRectangleLinesEx(r, 1, Color.Gray);

            Program.DrawTextCyr(titles[i], r.X + 15, r.Y + 10, 16, Color.White);
            Program.DrawTextCyr(descs[i], r.X + 15, r.Y + 32, 11, Color.LightGray);

            if (DrawButton((int)r.X + (int)r.Width - 110, (int)r.Y + 30, 95, 36, "ВЗЯТЬ", new Color(46, 139, 87, 255), new Color(60, 179, 113, 255)))
            {
                RankSystem.GiveKit(player, kits[i]);
            }
        }
    }

    private static void DrawBattlePassGUI(Player player, int sw, int sh)
    {
        int cx = sw / 2, cy = sh / 2;
        int pw = 580, ph = 420;
        Rectangle panel = new(cx - pw / 2, cy - ph / 2, pw, ph);

        Raylib.DrawRectangleRec(panel, new Color(20, 20, 28, 245));
        Raylib.DrawRectangleLinesEx(panel, 2, new Color(180, 40, 220, 255));

        Program.DrawTextCyr("БОЕВОЙ ПРОПУСК - СЕЗОН 1", cx - Program.MeasureTextCyr("БОЕВОЙ ПРОПУСК - СЕЗОН 1", 22) / 2, panel.Y + 15, 22, Color.Gold);

        string lvlInfo = $"Уровень: {player.BP.CurrentLevel} / {BattlePass.MaxLevel} | Опыт: {player.BP.CurrentExp} / {BattlePass.ExpPerLevel} XP";
        Program.DrawTextCyr(lvlInfo, cx - Program.MeasureTextCyr(lvlInfo, 14) / 2, panel.Y + 45, 14, Color.White);

        int barW = 440, barH = 12, barX = cx - barW / 2, barY = (int)panel.Y + 68;
        float progress = (float)player.BP.CurrentExp / BattlePass.ExpPerLevel;
        Raylib.DrawRectangle(barX, barY, barW, barH, new Color(40, 40, 45, 255));
        Raylib.DrawRectangle(barX + 1, barY + 1, (int)((barW - 2) * progress), barH - 2, new Color(180, 40, 220, 255));
        Raylib.DrawRectangleLines(barX, barY, barW, barH, Color.Gold);

        Program.DrawTextCyr("Текущие задания сезона:", panel.X + 25, panel.Y + 95, 15, Color.Yellow);
        for (int i = 0; i < Math.Min(3, player.BP.Tasks.Count); i++)
        {
            var task = player.BP.Tasks[i];
            int ty = (int)panel.Y + 120 + i * 28;
            string status = task.IsCompleted ? "§a[ВЫПОЛНЕНО]" : $"§e[{task.CurrentProgress}/{task.RequiredProgress}]";
            Program.DrawTextCyr($"• {task.Title} {status}", panel.X + 30, ty, 13, Color.White);
        }

        Program.DrawTextCyr("Награды уровней (Бесплатные / Премиум):", panel.X + 25, panel.Y + 215, 15, Color.Gold);

        int startLvl = Math.Max(1, player.BP.CurrentLevel - 2);
        for (int i = 0; i < 5; i++)
        {
            int lvl = startLvl + i;
            if (lvl > BattlePass.MaxLevel) break;

            var rLevel = player.BP.Rewards.Find(x => x.Level == lvl);
            if (rLevel == null) continue;

            int slotX = (int)panel.X + 35 + i * 105;
            int slotY = (int)panel.Y + 245;

            Raylib.DrawRectangle(slotX, slotY, 95, 110, (lvl <= player.BP.CurrentLevel) ? new Color(40, 60, 40, 200) : new Color(40, 40, 40, 180));
            Raylib.DrawRectangleLines(slotX, slotY, 95, 110, (lvl == player.BP.CurrentLevel) ? Color.Gold : Color.Gray);

            Program.DrawTextCyr($"LVL {lvl}", slotX + 25, slotY + 6, 12, Color.Yellow);
            Program.DrawItemIcon(rLevel.FreeReward, slotX + 10, slotY + 25, 28);
            Program.DrawItemIcon(rLevel.PremiumReward, slotX + 55, slotY + 25, 28);

            if (lvl <= player.BP.CurrentLevel)
            {
                if (!rLevel.FreeClaimed || (!rLevel.PremiumClaimed && player.BP.HasPremium))
                {
                    if (DrawButton(slotX + 47, slotY + 85, 75, 24, "Забрать", new Color(46, 139, 87, 255), new Color(60, 179, 113, 255)))
                    {
                        player.BP.ClaimReward(lvl, false, player);
                        player.BP.ClaimReward(lvl, true, player);
                    }
                }
                else
                {
                    Program.DrawTextCyr("Забрано", slotX + 22, slotY + 75, 11, Color.Gray);
                }
            }
        }
    }

    private static void DrawCraftingGUI(Player player, int sw, int sh, ref ItemStack draggedSlot)
    {
        int cx = sw / 2, cy = sh / 2;
        int pw = 520, ph = 460;
        Rectangle panel = new(cx - pw / 2, cy - ph / 2, pw, ph);

        Raylib.DrawRectangleRec(panel, new Color(25, 25, 30, 245));
        Raylib.DrawRectangleLinesEx(panel, 2, Color.Gold);

        Program.DrawTextCyr("ВЕРСТАК FUNTIME (3x3)", cx - Program.MeasureTextCyr("ВЕРСТАК FUNTIME (3x3)", 22) / 2, panel.Y + 16, 22, Color.Gold);

        int slotSize = 44;
        int gridX = (int)panel.X + 60;
        int gridY = (int)panel.Y + 55;

        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            int idx = r * 3 + c;
            int sx = gridX + c * (slotSize + 4);
            int sy = gridY + r * (slotSize + 4);
            Program.DrawSlot(sx, sy, slotSize, ref CraftingGrid[idx], Color.Gray);
        }

        Program.DrawTextCyr("===>", gridX + 160, gridY + 50, 24, Color.Gold);

        CraftResult = CraftingManager.MatchRecipe(CraftingGrid);
        int resX = gridX + 240, resY = gridY + 45;
        Rectangle resRect = new(resX, resY, 54, 54);
        Raylib.DrawRectangleRec(resRect, new Color(50, 50, 60, 220));
        Raylib.DrawRectangleLinesEx(resRect, 2, Color.Gold);
        Program.DrawItemIcon(CraftResult, resX + 11, resY + 11, 32);

        if (Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), resRect) && Raylib.IsMouseButtonPressed(MouseButton.Left) && !CraftResult.IsEmpty)
        {
            if (player.TryAddItem(CraftResult.Type, CraftResult.Count))
            {
                for (int i = 0; i < 9; i++)
                {
                    if (!CraftingGrid[i].IsEmpty)
                    {
                        CraftingGrid[i].Count--;
                        if (CraftingGrid[i].Count <= 0) CraftingGrid[i] = new ItemStack();
                    }
                }
                SoundManager.PlayPlace();
            }
        }

        // Полное отображение инвентаря игрока (27 слотов + Хотбар)
        Program.DrawTextCyr("Инвентарь:", panel.X + 40, panel.Y + 205, 14, Color.White);
        for (int i = 0; i < 27; i++)
        {
            int row = i / 9, col = i % 9;
            Program.DrawSlot((int)panel.X + 40 + col * slotSize, (int)panel.Y + 225 + row * slotSize, slotSize, ref player.MainInventory[i], Color.Gray);
        }

        Program.DrawTextCyr("Хотбар:", panel.X + 40, panel.Y + 365, 14, Color.White);
        for (int i = 0; i < 9; i++)
        {
            Program.DrawSlot((int)panel.X + 40 + i * slotSize, (int)panel.Y + 385, slotSize, ref player.Hotbar[i], Color.Yellow);
        }
    }

    private static void DrawBuyerGUI(Player player, int sw, int sh)
    {
        int cx = sw / 2, cy = sh / 2;
        int pw = 480, ph = 360;
        Rectangle panel = new(cx - pw / 2, cy - ph / 2, pw, ph);

        Raylib.DrawRectangleRec(panel, new Color(20, 25, 20, 245));
        Raylib.DrawRectangleLinesEx(panel, 2, Color.Lime);

        Program.DrawTextCyr("СКУПЩИК РУДЫ И ПРЕДМЕТОВ", cx - Program.MeasureTextCyr("СКУПЩИК РУДЫ И ПРЕДМЕТОВ", 20) / 2, panel.Y + 20, 20, Color.Lime);

        (ItemType item, int price)[] sellList = {
            (ItemType.CoalOre, 150),
            (ItemType.IronOre, 450),
            (ItemType.GoldOre, 900),
            (ItemType.DiamondOre, 3500),
            (ItemType.EmeraldOre, 5000),
            (ItemType.NetheriteOre, 25000)
        };

        for (int i = 0; i < sellList.Length; i++)
        {
            var (item, price) = sellList[i];
            int rowY = (int)panel.Y + 60 + i * 44;
            Rectangle r = new(panel.X + 25, rowY, panel.Width - 50, 38);
            Raylib.DrawRectangleRec(r, new Color(35, 45, 35, 220));
            Raylib.DrawRectangleLinesEx(r, 1, Color.Lime);

            Program.DrawItemIcon(new ItemStack(item, 1), (int)r.X + 6, (int)r.Y + 3, 32);
            Program.DrawTextCyr($"{BlockInfo.Name(item)} » +{price}$ / шт.", r.X + 45, r.Y + 11, 14, Color.White);

            if (DrawButton((int)r.X + (int)r.Width - 90, (int)r.Y + 19, 80, 28, "ПРОДАТЬ", new Color(46, 139, 87, 255), new Color(60, 179, 113, 255)))
            {
                SellItem(player, item, price);
            }
        }
    }

    private static void SellItem(Player player, ItemType type, int pricePerItem)
    {
        int totalSold = 0;
        for (int i = 0; i < player.MainInventory.Length; i++)
        {
            if (player.MainInventory[i].Type == type)
            {
                totalSold += player.MainInventory[i].Count;
                player.MainInventory[i] = new ItemStack();
            }
        }
        for (int i = 0; i < player.Hotbar.Length; i++)
        {
            if (player.Hotbar[i].Type == type)
            {
                totalSold += player.Hotbar[i].Count;
                player.Hotbar[i] = new ItemStack();
            }
        }

        if (totalSold > 0)
        {
            int earned = totalSold * pricePerItem;
            player.Balance += earned;
            SoundManager.PlayPlace();
            Program.AddChatMessage($"[Скупщик] Вы продали {totalSold}x {BlockInfo.Name(type)} за +{earned:N0}$!", Color.Lime);
        }
        else
        {
            Program.AddChatMessage("[Скупщик] У вас нет данного ресурса в инвентаре!", Color.Red);
        }
    }

    private static void DrawAuctionGUI(Player player, int sw, int sh)
    {
        int cx = sw / 2, cy = sh / 2;
        int pw = 520, ph = 380;
        Rectangle panel = new(cx - pw / 2, cy - ph / 2, pw, ph);

        Raylib.DrawRectangleRec(panel, new Color(25, 25, 30, 245));
        Raylib.DrawRectangleLinesEx(panel, 2, Color.Gold);

        Program.DrawTextCyr("АУКЦИОН СЕРВЕРА (/ah)", cx - Program.MeasureTextCyr("АУКЦИОН СЕРВЕРА (/ah)", 22) / 2, panel.Y + 20, 22, Color.Gold);

        (ItemStack stack, int cost)[] market = {
            (new ItemStack(ItemType.SphereAres, 1), 150000),
            (new ItemStack(ItemType.SphereScythian, 1), 220000),
            (new ItemStack(ItemType.TalismanCobra, 1), 350000),
            (new ItemStack(ItemType.TrapBoxCobra, 4), 60000)
        };

        for (int i = 0; i < market.Length; i++)
        {
            var (stack, cost) = market[i];
            int rowY = (int)panel.Y + 70 + i * 65;
            Rectangle r = new(panel.X + 25, rowY, panel.Width - 50, 55);
            Raylib.DrawRectangleRec(r, new Color(40, 40, 50, 220));
            Raylib.DrawRectangleLinesEx(r, 1, Color.Gold);

            Program.DrawItemIcon(stack, (int)r.X + 10, (int)r.Y + 11, 32);
            Program.DrawTextCyr(BlockInfo.Name(stack.Type), r.X + 52, r.Y + 10, 15, Color.Gold);
            Program.DrawTextCyr($"Цена: {cost:N0}$", r.X + 52, r.Y + 30, 13, Color.Lime);

            if (DrawButton((int)r.X + (int)r.Width - 100, (int)r.Y + 27, 85, 32, "КУПИТЬ", new Color(180, 120, 30, 255), new Color(220, 150, 40, 255)))
            {
                if (player.Balance >= cost)
                {
                    if (player.TryAddItem(stack.Type, stack.Count))
                    {
                        player.Balance -= cost;
                        SoundManager.PlayTotem();
                        Program.AddChatMessage($"[Аукцион] Вы приобрели {BlockInfo.Name(stack.Type)}!", Color.Lime);
                    }
                }
                else
                {
                    Program.AddChatMessage("[Аукцион] Недостаточно средств на балансе!", Color.Red);
                }
            }
        }
    }

    private static void DrawDonateGUI(Player player, int sw, int sh)
    {
        int cx = sw / 2, cy = sh / 2;
        int pw = 520, ph = 360;
        Rectangle panel = new(cx - pw / 2, cy - ph / 2, pw, ph);

        Raylib.DrawRectangleRec(panel, new Color(25, 20, 35, 245));
        Raylib.DrawRectangleLinesEx(panel, 2, Color.Magenta);

        Program.DrawTextCyr("ДОНАТ ПРИВИЛЕГИИ FUNTIME", cx - Program.MeasureTextCyr("ДОНАТ ПРИВИЛЕГИИ FUNTIME", 22) / 2, panel.Y + 20, 22, Color.Magenta);

        Program.DrawTextCyr("• VIP - /feed, /kit vip", panel.X + 30, panel.Y + 70, 14, Color.Lime);
        Program.DrawTextCyr("• HERO - /heal, /kit hero, Сохранение инвентаря 50%", panel.X + 30, panel.Y + 105, 14, Color.SkyBlue);
        Program.DrawTextCyr("• TITAN - /fly на спавне, +5% скорости, /kit titan", panel.X + 30, panel.Y + 140, 14, Color.Blue);
        Program.DrawTextCyr("• DRAGON - +2 HP пассивно, /craft везде, /kit dragon", panel.X + 30, panel.Y + 175, 14, Color.Red);
        Program.DrawTextCyr("• SPONSOR - +10% урона, +0.3 реген, /kit sponsor", panel.X + 30, panel.Y + 210, 14, Color.Gold);
        Program.DrawTextCyr("• COBRA (MAX) - Все наборы, +4 HP, +10% скорости, полный БП!", panel.X + 30, panel.Y + 245, 14, Color.Orange);

        Program.DrawTextCyr("Сайт авто-доната: shop.funtime.su", cx - Program.MeasureTextCyr("Сайт авто-доната: shop.funtime.su", 14) / 2, panel.Y + 305, 14, Color.Yellow);
    }

    private static bool DrawButton(int centerX, int centerY, int w, int h, string text, Color col, Color hoverCol)
    {
        Rectangle r = new(centerX - w / 2, centerY - h / 2, w, h);
        bool hover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), r);
        Raylib.DrawRectangleRec(r, hover ? hoverCol : col);
        Raylib.DrawRectangleLinesEx(r, 1, Color.White);
        float tx = r.X + r.Width / 2 - Program.MeasureTextCyr(text, 14) / 2;
        float ty = r.Y + r.Height / 2 - 7;
        Program.DrawTextCyr(text, tx, ty, 14, Color.White);
        return hover && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }
}