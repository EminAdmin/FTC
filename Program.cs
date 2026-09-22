using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

enum GameState { Menu, Playing, Settings, InGamePause }

class Program
{
    private static ItemStack draggedSlot = new();
    private static readonly PluginManager pluginManager = new();

    private static Vector3Int miningTarget = new(-1, -1, -1);
    private static float miningProgress = 0f;
    private static ItemType miningType = ItemType.None;
    private static float placeCooldown = 0f;

    private static float timeOfDay = 0.25f;
    private const float DayLength = 360f;
    private static float renderDistance = 80f;

    private static bool frameHasTarget;
    private static Vector3Int frameTarget;
    private static Vector3Int frameNormal;
    private static ItemType frameTargetType;

    private static GameState gameState = GameState.Menu;
    private static World? world;
    private static Player? player;

    private static float settingsSens = 0.15f;
    private static float settingsFov = 70f;
    private static bool settingsInvertY = false;

    private static Font customFont;
    private static bool hasCustomFont = false;

    private static string chatInput = "";
    private static readonly List<(string text, Color color, float timer)> chatMessages = new();

    static void Main()
    {
        Raylib.InitWindow(1280, 720, "FunTime Cobra - Minecraft Anarchy 2026");
        Raylib.SetTargetFPS(60);
        Raylib.SetExitKey(KeyboardKey.Null);

        SoundManager.Initialize();
        LoadCyrillicFont();
        TextureAtlas.Initialize();
        pluginManager.RegisterPlugin(new FunTimeCorePlugin());

        InitGame();
        AddChatMessage("[FunTime] Добро пожаловать на Анархию! Меню: /menu | Помощь: /help", Color.Gold);

        while (!Raylib.WindowShouldClose())
        {
            float dt = Raylib.GetFrameTime();
            int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();

            UpdateChatTimers(dt);
            ParticleSystem.Update(dt);
            ItemRenderer3D.Update(dt);

            switch (gameState)
            {
                case GameState.Menu:
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(new Color(20, 20, 28, 255));
                    DrawMenu(sw, sh);
                    Raylib.EndDrawing();
                    break;

                case GameState.Playing:
                    UpdateGame(dt);
                    Raylib.BeginDrawing();
                    DrawGame(sw, sh);
                    Raylib.EndDrawing();
                    break;

                case GameState.InGamePause:
                    UpdatePause();
                    Raylib.BeginDrawing();
                    DrawGame(sw, sh);
                    DrawPauseMenu(sw, sh);
                    Raylib.EndDrawing();
                    break;

                case GameState.Settings:
                    Raylib.BeginDrawing();
                    Raylib.ClearBackground(new Color(25, 25, 35, 255));
                    DrawSettings(sw, sh);
                    Raylib.EndDrawing();
                    break;
            }
        }

        if (hasCustomFont) Raylib.UnloadFont(customFont);
        Raylib.CloseAudioDevice();
        Raylib.CloseWindow();
    }

    private static void LoadCyrillicFont()
    {
        List<int> codepoints = new();
        for (int i = 32; i <= 126; i++) codepoints.Add(i);
        for (int i = 0x0400; i <= 0x04FF; i++) codepoints.Add(i);

        int[] cpArray = codepoints.ToArray();
        string[] paths = {
            "C:\\Windows\\Fonts\\arial.ttf",
            "C:\\Windows\\Fonts\\tahoma.ttf",
            "C:\\Windows\\Fonts\\segoeui.ttf",
            "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
            "/System/Library/Fonts/Supplemental/Arial.ttf"
        };

        foreach (var p in paths)
        {
            if (File.Exists(p))
            {
                try
                {
                    customFont = Raylib.LoadFontEx(p, 36, cpArray, cpArray.Length);
                    Raylib.SetTextureFilter(customFont.Texture, TextureFilter.Bilinear);
                    hasCustomFont = true;
                    return;
                }
                catch { }
            }
        }
        customFont = Raylib.GetFontDefault();
    }

    public static void DrawTextCyr(string text, float x, float y, float size, Color color)
    {
        if (hasCustomFont)
            Raylib.DrawTextEx(customFont, text, new Vector2(x, y), size, 1.0f, color);
        else
            Raylib.DrawText(text, (int)x, (int)y, (int)size, color);
    }

    public static float MeasureTextCyr(string text, float size)
    {
        if (hasCustomFont)
            return Raylib.MeasureTextEx(customFont, text, size, 1.0f).X;
        return Raylib.MeasureText(text, (int)size);
    }

    static void InitGame()
    {
        world = new World();
        player = new Player(world.SpawnPosition)
        {
            MouseSensitivity = settingsSens,
            CameraFov = settingsFov,
            InvertMouseY = settingsInvertY
        };
        miningProgress = 0f;
        miningTarget = new Vector3Int(-1, -1, -1);
        placeCooldown = 0f;
        timeOfDay = 0.25f;
        draggedSlot = new ItemStack();
    }

    public static void AddChatMessage(string text, Color color)
    {
        chatMessages.Add((text, color, 8.5f));
        if (chatMessages.Count > 12) chatMessages.RemoveAt(0);
    }

    private static void UpdateChatTimers(float dt)
    {
        for (int i = chatMessages.Count - 1; i >= 0; i--)
        {
            var msg = chatMessages[i];
            msg.timer -= dt;
            if (msg.timer <= 0) chatMessages.RemoveAt(i);
            else chatMessages[i] = msg;
        }
    }

    static void DrawMenu(int sw, int sh)
    {
        int cw = sw / 2, ch = sh / 2;
        DrawTextCyr("FUNTIME ANARCHY 2026", cw - MeasureTextCyr("FUNTIME ANARCHY 2026", 46) / 2, ch - 180, 46, Color.Gold);
        DrawTextCyr("Minecraft Edition v4.5 • Cobra Network", cw - MeasureTextCyr("Minecraft Edition v4.5 • Cobra Network", 18) / 2, ch - 120, 18, Color.LightGray);

        if (DrawButton(sw, ch - 50, 260, 50, "ИГРАТЬ", new Color(46, 139, 87, 255), new Color(60, 179, 113, 255)))
        {
            if (player == null) InitGame();
            Raylib.DisableCursor();
            gameState = GameState.Playing;
        }
        else if (DrawButton(sw, ch + 20, 260, 50, "НАСТРОЙКИ", new Color(70, 80, 140, 255), new Color(90, 100, 180, 255)))
        {
            gameState = GameState.Settings;
        }
        else if (DrawButton(sw, ch + 90, 260, 50, "ВЫЙТИ", new Color(178, 34, 34, 255), new Color(220, 20, 60, 255)))
        {
            Raylib.CloseWindow();
        }
    }

    static void UpdatePause()
    {
        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            Raylib.DisableCursor();
            gameState = GameState.Playing;
        }
    }

    static void DrawPauseMenu(int sw, int sh)
    {
        Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 160));
        int cw = sw / 2, ch = sh / 2;
        DrawTextCyr("МЕНЮ ПАУЗЫ", cw - MeasureTextCyr("МЕНЮ ПАУЗЫ", 28) / 2, ch - 130, 28, Color.White);

        if (DrawButton(sw, ch - 40, 260, 44, "Вернуться в игру", new Color(50, 120, 50, 255), new Color(70, 160, 70, 255)))
        {
            Raylib.DisableCursor();
            gameState = GameState.Playing;
        }
        else if (DrawButton(sw, ch + 15, 260, 44, "Телепорт на Спавн", new Color(60, 70, 130, 255), new Color(80, 90, 170, 255)))
        {
            if (player != null && world != null)
            {
                player.Position = world.SpawnPosition;
                player.Velocity = Vector3.Zero;
            }
            Raylib.DisableCursor();
            gameState = GameState.Playing;
        }
        else if (DrawButton(sw, ch + 70, 260, 44, "Главное меню", new Color(150, 40, 40, 255), new Color(190, 50, 50, 255)))
        {
            Raylib.SetMousePosition(0, 0);
            gameState = GameState.Menu;
        }
    }

    static void DrawSettings(int sw, int sh)
    {
        int cw = sw / 2;
        DrawTextCyr("НАСТРОЙКИ", cw - MeasureTextCyr("НАСТРОЙКИ", 36) / 2, 70, 36, Color.White);

        DrawTextCyr($"Чувствительность мыши: {settingsSens:F2}", cw - 160, 150, 18, Color.White);
        settingsSens = DrawSlider(cw, 180, 320, 18, settingsSens, 0.05f, 0.5f);

        DrawTextCyr($"Поле зрения (FOV): {settingsFov:F0}", cw - 160, 220, 18, Color.White);
        settingsFov = DrawSlider(cw, 250, 320, 18, settingsFov, 60f, 110f);

        DrawTextCyr($"Дальность прорисовки: {renderDistance:F0}", cw - 160, 290, 18, Color.White);
        renderDistance = DrawSlider(cw, 320, 320, 18, renderDistance, 48f, 160f);

        if (DrawButton(sw, 560, 220, 46, "НАЗАД", Color.Gray, Color.DarkGray))
        {
            if (player != null)
            {
                player.MouseSensitivity = settingsSens;
                player.CameraFov = settingsFov;
                player.InvertMouseY = settingsInvertY;
            }
            Raylib.SetMousePosition(0, 0);
            gameState = GameState.Menu;
        }
    }

    static float DrawSlider(int centerX, int y, int width, int height, float value, float min, float max)
    {
        Rectangle track = new(centerX - width / 2, y, width, height);
        Raylib.DrawRectangleRec(track, new Color(50, 50, 60, 255));
        Raylib.DrawRectangleLinesEx(track, 1, Color.Gray);

        float t = (value - min) / (max - min);
        int knobX = (int)(track.X + t * width) - 8;
        Rectangle knob = new(knobX, y - 3, 16, height + 6);
        Raylib.DrawRectangleRec(knob, Color.White);

        Vector2 mouse = Raylib.GetMousePosition();
        if (Raylib.CheckCollisionPointRec(mouse, track) && Raylib.IsMouseButtonDown(MouseButton.Left))
        {
            float newT = Math.Clamp((mouse.X - track.X) / width, 0f, 1f);
            return min + newT * (max - min);
        }
        return value;
    }

    static bool DrawButton(int sw, int y, int w, int h, string text, Color col, Color hoverCol)
    {
        Rectangle r = new(sw / 2 - w / 2, y - h / 2, w, h);
        bool hover = Raylib.CheckCollisionPointRec(Raylib.GetMousePosition(), r);
        Raylib.DrawRectangleRec(r, hover ? hoverCol : col);
        Raylib.DrawRectangleLinesEx(r, 2, Color.White);
        float tx = r.X + r.Width / 2 - MeasureTextCyr(text, 20) / 2;
        float ty = r.Y + r.Height / 2 - 10;
        DrawTextCyr(text, tx, ty, 20, Color.White);
        return hover && Raylib.IsMouseButtonPressed(MouseButton.Left);
    }

    static void UpdateGame(float dt)
    {
        if (player == null || world == null) return;

        timeOfDay = (timeOfDay + dt / DayLength) % 1.0f;
        world.Update(dt);
        world.UpdateChunkStreaming(player.Position, renderDistance);
        NetworkManager.Update(dt, player, world);

        if ((Raylib.IsKeyPressed(KeyboardKey.T) || Raylib.IsKeyPressed(KeyboardKey.Slash)) && !player.IsChatOpen && !player.IsInventoryOpen && !player.IsChestOpen && !player.IsGUIOpen)
        {
            player.IsChatOpen = true;
            chatInput = Raylib.IsKeyPressed(KeyboardKey.Slash) ? "/" : "";
            Raylib.EnableCursor();
            return;
        }

        if (player.IsChatOpen)
        {
            UpdateChatInput();
            return;
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            if (player.IsGUIOpen) GUIManager.Close(player);
            else if (player.IsChestOpen)
            {
                player.IsChestOpen = false;
                if (!player.IsInventoryOpen) Raylib.DisableCursor();
            }
            else if (player.IsInventoryOpen)
            {
                player.IsInventoryOpen = false;
                Raylib.DisableCursor();
            }
            else
            {
                Raylib.EnableCursor();
                gameState = GameState.InGamePause;
                return;
            }
        }

        player.Update(dt, world);

        if (!player.IsInventoryOpen && !player.IsChestOpen && !player.IsGUIOpen && !draggedSlot.IsEmpty)
        {
            player.TryAddItem(draggedSlot.Type, draggedSlot.Count);
            draggedSlot = new ItemStack();
        }

        placeCooldown -= dt;

        frameHasTarget = false;
        frameTarget = new Vector3Int(-1, -1, -1);
        frameNormal = default;
        frameTargetType = ItemType.None;

        if (!player.IsInventoryOpen && !player.IsChestOpen && !player.IsGUIOpen)
        {
            var hitNPC = world.NpcManager.CheckInteraction(player.Position, player.Forward);
            if (hitNPC != null && Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                switch (hitNPC.Type)
                {
                    case NPCType.Buyer: GUIManager.Open(GUIState.Buyer, player); break;
                    case NPCType.Auction: GUIManager.Open(GUIState.Auction, player); break;
                    case NPCType.BattlePass: GUIManager.Open(GUIState.BattlePass, player); break;
                    case NPCType.Donation: GUIManager.Open(GUIState.Donate, player); break;
                }
                return;
            }

            if (world.Raycast(player.GetCamera().Position, player.Forward, 6.0f, out Vector3Int target, out Vector3Int targetNormal))
            {
                frameHasTarget = true;
                frameTarget = target;
                frameNormal = targetNormal;
                frameTargetType = world.GetBlock(target.X, target.Y, target.Z);
            }

            ItemType targetType = frameTargetType;
            ItemStack selStack = player.GetSelectedStack();
            bool isInteractiveBlock = targetType == ItemType.CraftingTable || targetType == ItemType.Chest || targetType == ItemType.MysticChest;

            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                if (selStack.IsFood && !player.IsEating && !isInteractiveBlock)
                {
                    player.StartEating();
                }
            }
            else if (player.IsEating)
            {
                player.StopEating();
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Right) && placeCooldown <= 0f && !player.IsEating)
            {
                if (targetType == ItemType.CraftingTable)
                {
                    GUIManager.Open(GUIState.Crafting, player);
                }
                else if (targetType == ItemType.Chest || targetType == ItemType.MysticChest)
                {
                    if (targetType == ItemType.MysticChest) player.BP.AddProgress("open_mystic", 1, player);
                    player.IsChestOpen = true;
                    Raylib.EnableCursor();
                }
                else if (selStack.Type == ItemType.MoneyVoucher50k || selStack.Type == ItemType.MoneyVoucher250k)
                {
                    int reward = (selStack.Type == ItemType.MoneyVoucher50k) ? 50000 : 250000;
                    player.Balance += reward;
                    player.Hotbar[player.SelectedSlot].Count--;
                    if (player.Hotbar[player.SelectedSlot].Count <= 0) player.Hotbar[player.SelectedSlot] = new ItemStack();
                    SoundManager.PlayTotem();
                    AddChatMessage($"[FunTime] » Вы успешно активировали чек на +{reward:N0}$!", Color.Gold);
                    placeCooldown = 0.3f;
                }
                else if (selStack.Type == ItemType.EnderPearl)
                {
                    player.ThrowEnderPearl();
                    placeCooldown = 0.25f;
                }
                else if (selStack.Type >= ItemType.TrapBox && selStack.Type <= ItemType.TrapBoxCryo)
                {
                    player.DeployTrap(world, selStack.Type);
                    placeCooldown = 0.4f;
                }
                else if (selStack.Type == ItemType.Plast)
                {
                    player.DeployPlast(world);
                    placeCooldown = 0.4f;
                }
                else if (selStack.Type == ItemType.MysticSummon)
                {
                    Vector3 mPos = player.Position + player.Forward * 4f;
                    mPos.Y = world.GetSurfaceHeight((int)mPos.X, (int)mPos.Z) + 1;
                    world.StartMystic(mPos, ItemRarity.Mythic);
                    player.Hotbar[player.SelectedSlot].Count--;
                    if (player.Hotbar[player.SelectedSlot].Count <= 0) player.Hotbar[player.SelectedSlot] = new ItemStack();
                    placeCooldown = 1.0f;
                }
                else if (targetType != ItemType.None)
                {
                    player.SwingHand();
                    TryPlaceBlock(player, world, target, targetNormal);
                    placeCooldown = 0.20f;
                }
            }

            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                player.SwingHand();

                if (targetType != ItemType.None && !BlockRegistry.IsSolid(targetType) == false && BlockRegistry.GetHardness(targetType) < float.PositiveInfinity)
                {
                    if (target != miningTarget || targetType != miningType)
                    {
                        miningTarget = target;
                        miningType = targetType;
                        miningProgress = 0f;
                    }

                    float hardness = BlockRegistry.GetHardness(miningType) / player.MiningSpeedMultiplier;
                    miningProgress += dt / hardness;

                    if (miningProgress >= 1.0f)
                    {
                        if (pluginManager.EventBus.TriggerBlockBreak(miningTarget, miningType))
                        {
                            Vector3 bPos = new(miningTarget.X, miningTarget.Y, miningTarget.Z);
                            Color pCol = miningType switch
                            {
                                ItemType.Grass => new Color(92, 175, 54, 255),
                                ItemType.Dirt => new Color(134, 96, 67, 255),
                                ItemType.Log or ItemType.Planks => new Color(160, 120, 70, 255),
                                ItemType.Leaves => new Color(60, 120, 45, 255),
                                ItemType.DiamondOre => new Color(90, 230, 225, 255),
                                ItemType.Obsidian => new Color(30, 20, 45, 255),
                                ItemType.Glowstone => Color.Gold,
                                _ => new Color(125, 125, 128, 255)
                            };
                            ParticleSystem.SpawnBlockBreak(bPos, pCol);
                            SoundManager.PlayBreak();

                            world.SetBlock(miningTarget.X, miningTarget.Y, miningTarget.Z, ItemType.None);
                            NetworkManager.BroadcastBlockChange(miningTarget.X, miningTarget.Y, miningTarget.Z, ItemType.None);

                            ItemType drop = ItemData.GetDrop(miningType);
                            if (drop != ItemType.None) player.TryAddItem(drop, 1);

                            if (miningType == ItemType.Obsidian) player.BP.AddProgress("break_obsidian", 1, player);
                            if (miningType == ItemType.DiamondOre) player.BP.AddProgress("mine_diamonds", 1, player);
                        }
                        miningProgress = 0f;
                        miningTarget = new Vector3Int(-1, -1, -1);
                        miningType = ItemType.None;
                    }
                }
                else miningProgress = 0f;
            }
            else
            {
                miningProgress = 0f;
                miningTarget = new Vector3Int(-1, -1, -1);
                miningType = ItemType.None;
            }
        }
    }

    private static void UpdateChatInput()
    {
        int key = Raylib.GetCharPressed();
        while (key > 0)
        {
            if (key >= 32) chatInput += char.ConvertFromUtf32(key);
            key = Raylib.GetCharPressed();
        }

        if (Raylib.IsKeyPressed(KeyboardKey.Backspace) && chatInput.Length > 0)
            chatInput = chatInput[..^1];

        if (Raylib.IsKeyPressed(KeyboardKey.Enter))
        {
            ExecuteChatCommand(chatInput);
            chatInput = "";
            player!.IsChatOpen = false;
            Raylib.DisableCursor();
        }
        else if (Raylib.IsKeyPressed(KeyboardKey.Escape))
        {
            chatInput = "";
            player!.IsChatOpen = false;
            Raylib.DisableCursor();
        }
    }

    private static void ExecuteChatCommand(string cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) return;
        cmd = cmd.Trim();

        if (cmd.StartsWith("/"))
        {
            string[] parts = cmd.Split(' ');
            string action = parts[0].ToLower();

            if (action == "/rtp")
            {
                if (player != null && world != null)
                {
                    RtpManager.ExecuteRTP(player, world);
                }
            }
            else if (action == "/spawn")
            {
                if (player != null && world != null)
                {
                    player.Position = world.SpawnPosition;
                    player.Velocity = Vector3.Zero;
                    ParticleSystem.SpawnTotemEffect(world.SpawnPosition);
                    SoundManager.PlayTeleport();
                    AddChatMessage("[FunTime] » Вы телепортированы на Спавн!", Color.Gold);
                }
            }
            else if (action == "/kit")
            {
                if (player != null)
                {
                    if (parts.Length > 1) RankSystem.GiveKit(player, parts[1]);
                    else GUIManager.Open(GUIState.Kits, player);
                }
            }
            else if (action == "/bp")
            {
                if (player != null) GUIManager.Open(GUIState.BattlePass, player);
            }
            else if (action == "/craft")
            {
                if (player != null) GUIManager.Open(GUIState.Crafting, player);
            }
            else if (action == "/ah")
            {
                if (player != null) GUIManager.Open(GUIState.Auction, player);
            }
            else if (action == "/buyer")
            {
                if (player != null) GUIManager.Open(GUIState.Buyer, player);
            }
            else if (action == "/donate")
            {
                if (player != null) GUIManager.Open(GUIState.Donate, player);
            }
            else if (action == "/fly")
            {
                if (player != null)
                {
                    player.IsFlying = !player.IsFlying;
                    AddChatMessage($"[FunTime] » Режим полёта: {(player.IsFlying ? "ВКЛЮЧЕН" : "ВЫКЛЮЧЕН")}", Color.Yellow);
                }
            }
            else if (action == "/heal" || action == "/feed")
            {
                if (player != null)
                {
                    player.Health = player.MaxHealth;
                    player.Hunger = 20f;
                    AddChatMessage("[FunTime] » Здоровье и сытость полностью восстановлены!", Color.Lime);
                }
            }
            else if (action == "/mystic")
            {
                if (player != null && world != null)
                {
                    Vector3 mPos = player.Position + player.Forward * 5f;
                    mPos.Y = world.GetSurfaceHeight((int)mPos.X, (int)mPos.Z) + 1;
                    world.StartMystic(mPos, ItemRarity.Legendary);
                }
            }
            else if (action == "/help")
            {
                AddChatMessage("[Помощь FunTime] Доступные команды:", Color.Yellow);
                AddChatMessage("  /kit, /bp, /craft, /ah, /buyer, /donate", Color.White);
                AddChatMessage("  /rtp, /spawn, /mystic, /fly, /heal, /clear", Color.White);
            }
            else if (action == "/clear")
            {
                if (player != null)
                {
                    Array.Clear(player.MainInventory);
                    Array.Clear(player.Hotbar);
                    player.OffHand = new ItemStack();
                    AddChatMessage("[FunTime] » Инвентарь очищен!", Color.Orange);
                }
            }
            else
            {
                AddChatMessage($"[Ошибка] Неизвестная команда: {cmd}. Введите /help", Color.Red);
            }
        }
        else
        {
            string pfx = RankSystem.GetPrefix(player!.Rank);
            AddChatMessage($"{pfx} {player.Name} » {cmd}", RankSystem.GetColor(player.Rank));
        }
    }

    static void DrawGame(int sw, int sh)
    {
        if (player == null || world == null) return;

        float daylight = MathF.Sin(timeOfDay * MathF.PI * 2.0f);
        daylight = Math.Clamp(daylight, 0f, 1f);
        float brightness = 0.15f + 0.85f * daylight;

        Color skyColor = new(
            (byte)(15 + 115 * brightness),
            (byte)(20 + 175 * brightness),
            (byte)(45 + 195 * brightness),
            (byte)255);

        Raylib.ClearBackground(skyColor);

        Raylib.BeginMode3D(player.GetCamera());

        DrawSunAndMoon(player.Position);
        world.Render(player.Position, renderDistance);
        ParticleSystem.Render();
        NetworkManager.RenderRemotePlayers();

        if (frameHasTarget && frameTargetType != ItemType.None && !player.IsInventoryOpen && !player.IsChestOpen && !player.IsGUIOpen)
        {
            DrawBlockOutline(frameTarget.X, frameTarget.Y, frameTarget.Z);
            if (miningProgress > 0f && miningTarget == frameTarget)
            {
                byte alpha = (byte)Math.Min(230, 40 + 190 * miningProgress);
                Raylib.DrawCube(new Vector3(frameTarget.X + 0.5f, frameTarget.Y + 0.5f, frameTarget.Z + 0.5f), 1.01f, 1.01f, 1.01f, new Color((byte)20, (byte)20, (byte)20, alpha));
            }
        }

        Raylib.EndMode3D();

        // 3D оружие/предметы в руках
        if (!player.IsInventoryOpen && !player.IsChestOpen && !player.IsGUIOpen && gameState == GameState.Playing)
        {
            ItemRenderer3D.RenderFirstPersonHands3D(player, sw, sh);
        }

        // Единый аутентичный FunTime HUD
        FunTimeHUD.Render(player, world, sw, sh);
        DrawChat(sw, sh);

        if (player.IsGUIOpen)
        {
            GUIManager.UpdateAndDraw(player, world, sw, sh, ref draggedSlot);
        }
        else if (player.IsChestOpen)
        {
            if (world.ActiveMysticChest != null && world.Mystic.IsOpened) DrawChestUI(player, world.ActiveMysticChest, "§6МИСТИЧЕСКИЙ СУНДУК");
            else if (world.SpawnChest != null) DrawChestUI(player, world.SpawnChest, "СУНДУК СПАВНА");
            else player.IsChestOpen = false;
        }
        else if (player.IsInventoryOpen) DrawInventoryUI(player);

        if (!draggedSlot.IsEmpty)
        {
            Vector2 mp = Raylib.GetMousePosition();
            DrawItemIcon(draggedSlot, (int)mp.X - 16, (int)mp.Y - 16, 32);
        }
    }

    private static void DrawSunAndMoon(Vector3 camPos)
    {
        float angle = timeOfDay * MathF.PI * 2f;
        float dist = 90f;

        Vector3 sunPos = camPos + new Vector3(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist, 0);
        Raylib.DrawCube(sunPos, 14f, 14f, 14f, Color.Gold);

        Vector3 moonPos = camPos - new Vector3(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist, 0);
        Raylib.DrawCube(moonPos, 10f, 10f, 10f, new Color(230, 240, 255, 255));
    }

    static void TryPlaceBlock(Player player, World world, Vector3Int target, Vector3Int normal)
    {
        int ax = target.X + normal.X, ay = target.Y + normal.Y, az = target.Z + normal.Z;
        ItemStack stack = player.GetSelectedStack();
        if (stack.IsEmpty || !stack.IsPlaceable) return;

        ItemType placeType = stack.Type;
        if (!pluginManager.EventBus.TriggerBlockPlace(new Vector3Int(ax, ay, az), ref placeType)) return;

        BoxAABB blockBox = new(new Vector3(ax, ay, az), new Vector3(ax + 1, ay + 1, az + 1));
        if (blockBox.Intersects(player.GetBoundingBox(player.Position))) return;

        world.SetBlock(ax, ay, az, placeType);
        NetworkManager.BroadcastBlockChange(ax, ay, az, placeType);
        SoundManager.PlayPlace();

        player.Hotbar[player.SelectedSlot].Count--;
        if (player.Hotbar[player.SelectedSlot].Count <= 0)
            player.Hotbar[player.SelectedSlot] = new ItemStack();
    }

    static void DrawBlockOutline(float x, float y, float z)
    {
        Color c = new((byte)20, (byte)20, (byte)20, (byte)220);
        Vector3 p000 = new(x, y, z), p100 = new(x + 1, y, z);
        Vector3 p010 = new(x, y + 1, z), p110 = new(x + 1, y + 1, z);
        Vector3 p001 = new(x, y, z + 1), p101 = new(x + 1, y, z + 1);
        Vector3 p011 = new(x, y + 1, z + 1), p111 = new(x + 1, y + 1, z + 1);
        Raylib.DrawLine3D(p000, p100, c); Raylib.DrawLine3D(p001, p101, c);
        Raylib.DrawLine3D(p000, p001, c); Raylib.DrawLine3D(p100, p101, c);
        Raylib.DrawLine3D(p010, p110, c); Raylib.DrawLine3D(p011, p111, c);
        Raylib.DrawLine3D(p010, p011, c); Raylib.DrawLine3D(p110, p111, c);
        Raylib.DrawLine3D(p000, p010, c); Raylib.DrawLine3D(p100, p110, c);
        Raylib.DrawLine3D(p001, p011, c); Raylib.DrawLine3D(p101, p111, c);
    }

    private static void DrawChat(int sw, int sh)
    {
        int cy = sh - 160;
        for (int i = 0; i < chatMessages.Count; i++)
        {
            var msg = chatMessages[i];
            Raylib.DrawRectangle(15, cy - (chatMessages.Count - 1 - i) * 22, (int)MeasureTextCyr(msg.text, 14) + 12, 20, new Color(0, 0, 0, 150));
            DrawTextCyr(msg.text, 20, cy - (chatMessages.Count - 1 - i) * 22 + 3, 14, msg.color);
        }

        if (player != null && player.IsChatOpen)
        {
            Raylib.DrawRectangle(15, sh - 40, 480, 30, new Color(0, 0, 0, 200));
            Raylib.DrawRectangleLines(15, sh - 40, 480, 30, Color.Gold);
            DrawTextCyr("Чат: " + chatInput + "_", 22, sh - 33, 16, Color.White);
        }
    }

    static void DrawInventoryUI(Player player)
    {
        int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 185));
        DrawTextCyr("ИНВЕНТАРЬ", sw / 2 - 60, 35, 22, Color.White);
        int slotSize = 44, startX = sw / 2 - 9 * slotSize / 2;
        for (int i = 0; i < 27; i++)
        {
            int row = i / 9, col = i % 9;
            DrawSlot(startX + col * slotSize, 75 + row * slotSize, slotSize, ref player.MainInventory[i], Color.Gray);
        }
        DrawTextCyr("Хотбар & Левая рука:", startX, 220, 14, Color.White);
        DrawSlot(startX - 56, 240, slotSize, ref player.OffHand, Color.Gold);
        for (int i = 0; i < 9; i++)
            DrawSlot(startX + i * slotSize, 240, slotSize, ref player.Hotbar[i], Color.Yellow);
    }

    static void DrawChestUI(Player player, Chest chest, string title)
    {
        int sw = Raylib.GetScreenWidth(), sh = Raylib.GetScreenHeight();
        Raylib.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 185));
        DrawTextCyr(title, sw / 2 - MeasureTextCyr(title, 22) / 2, 35, 22, Color.Gold);
        int slotSize = 44, startX = sw / 2 - 9 * slotSize / 2;
        for (int i = 0; i < 27; i++)
        {
            int row = i / 9, col = i % 9;
            DrawSlot(startX + col * slotSize, 75 + row * slotSize, slotSize, ref chest.Inventory[i], Color.Orange);
        }
        DrawTextCyr("Инвентарь:", startX, 220, 14, Color.White);
        for (int i = 0; i < 27; i++)
        {
            int row = i / 9, col = i % 9;
            DrawSlot(startX + col * slotSize, 240 + row * slotSize, slotSize, ref player.MainInventory[i], Color.Gray);
        }
        DrawTextCyr("Хотбар:", startX, 385, 14, Color.White);
        DrawSlot(startX - 56, 405, slotSize, ref player.OffHand, Color.Gold);
        for (int i = 0; i < 9; i++)
            DrawSlot(startX + i * slotSize, 405, slotSize, ref player.Hotbar[i], Color.Yellow);
    }

    public static bool DrawSlot(int x, int y, int size, ref ItemStack stack, Color borderColor)
    {
        Vector2 mouse = Raylib.GetMousePosition();
        Rectangle rect = new(x, y, size, size);
        bool hovered = Raylib.CheckCollisionPointRec(mouse, rect);
        Raylib.DrawRectangleRec(rect, hovered ? new Color(70, 70, 95, 220) : new Color(40, 40, 40, 220));
        Raylib.DrawRectangleLinesEx(rect, 2, hovered ? Color.White : borderColor);

        DrawItemIcon(stack, x + 6, y + 6, 32);

        if (hovered && !stack.IsEmpty)
        {
            string name = BlockRegistry.GetName(stack.Type);
            Raylib.DrawRectangle((int)mouse.X + 12, (int)mouse.Y - 24, (int)MeasureTextCyr(name, 14) + 10, 22, new Color(20, 20, 25, 240));
            Raylib.DrawRectangleLines((int)mouse.X + 12, (int)mouse.Y - 24, (int)MeasureTextCyr(name, 14) + 10, 22, Color.Gold);
            DrawTextCyr(name, mouse.X + 17, mouse.Y - 20, 14, Color.White);
        }

        if (hovered && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            ItemStack temp = stack;
            stack = draggedSlot;
            draggedSlot = temp;
            SoundManager.PlayPlace();
            return true;
        }
        return false;
    }

    public static void DrawItemIcon(ItemStack stack, int x, int y, int size)
    {
        if (stack.IsEmpty) return;

        Rectangle src = TextureAtlas.GetSourceRect(stack.Type);
        Rectangle dst = new(x, y, size, size);

        Raylib.DrawTexturePro(TextureAtlas.AtlasTexture, src, dst, Vector2.Zero, 0.0f, Color.White);

        if (stack.Count > 1)
        {
            string countStr = stack.Count.ToString();
            DrawTextCyr(countStr, x + size - MeasureTextCyr(countStr, 12) - 2, y + size - 14, 12, Color.Yellow);
        }
    }
}