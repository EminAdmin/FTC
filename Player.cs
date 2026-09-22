using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public enum StatusEffectType
{
    SlowFalling,
    Speed,
    Strength,
    Regeneration,
    Resistance
}

public class ActiveEffect
{
    public float Duration;
    public float MaxDuration;
    public int Amplifier;

    public ActiveEffect(float duration, int amplifier = 0)
    {
        Duration = duration;
        MaxDuration = duration;
        Amplifier = amplifier;
    }
}

public class Projectile
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Life = 3.0f;
}

public class Player
{
    // Позиционирование и ориентация в пространстве
    public Vector3 Position;
    public Vector3 Velocity;
    public float Yaw = 0.0f;
    public float Pitch = 0.0f;
    public bool IsGrounded = false;
    public bool IsFlying = false;

    // Игровой профиль и привилегии
    public string Name = "FunTimeLord";
    public PlayerRank Rank = PlayerRank.Cobra;
    public int Balance = 750000;

    // Слоты инвентаря
    public readonly ItemStack[] Hotbar = new ItemStack[9];
    public readonly ItemStack[] MainInventory = new ItemStack[27];
    public ItemStack OffHand = new();

    // Состояния интерфейса
    public int SelectedSlot = 0;
    public bool IsInventoryOpen = false;
    public bool IsChestOpen = false;
    public bool IsChatOpen = false;
    public bool IsGUIOpen = false;

    // Константы движения (стандарт Minecraft 1.16+)
    private const float BaseSpeed = 4.317f;
    private const float SprintMultiplier = 1.35f;
    private const float SneakMultiplier = 0.35f;
    private const float BaseJumpForce = 8.8f;
    private const float Gravity = 27.5f;
    public const float PlayerWidth = 0.6f;
    public const float PlayerHeight = 1.8f;

    // Динамические множители от Сфер и Талисманов
    public float ActiveSpeedMultiplier = 1.0f;
    public float ActiveJumpForce = BaseJumpForce;
    public float ActiveGravity = Gravity;
    public float MiningSpeedMultiplier = 1.0f;
    public bool NoFallDamage = false;
    public float DamageBoost = 1.0f;
    public float DamageResistance = 1.0f;
    private float currentFov = 70.0f;

    // Анимации и боевая система
    public float HandSwingProgress = 0f;
    public bool IsSwinging = false;
    public float AttackCooldownProgress = 1.0f;
    public float CombatTagTimer = 0f;

    // Механика употребления пищи/зелий
    public bool IsEating = false;
    public float EatTimer = 0f;
    public const float MaxEatDuration = 1.6f;

    // Жизненные показатели
    public float Health = 20f;
    public float MaxHealth = 20f;
    public float AbsorptionHealth = 0f;
    public float Hunger = 20f;
    public int Level = 30;
    public float Exp = 0.85f;

    // Состояния передвижения
    public bool IsSprinting = false;
    public bool IsSneaking = false;
    public Vector3 SpawnPoint;
    private float fallStartY = 0f;
    private bool wasGrounded = true;
    private float regenRate = 0.5f;
    private float regenCooldown = 0f;

    // Настройки камеры и ввода
    public float MouseSensitivity = 0.15f;
    public float CameraFov = 70f;
    public bool InvertMouseY = false;

    // Подсистемы
    public readonly CooldownSystem Cooldowns = new();
    public readonly BattlePass BP = new();
    public readonly List<Projectile> ActivePearls = new();
    public readonly Dictionary<StatusEffectType, ActiveEffect> ActiveEffects = new();

    public Player(Vector3 startPos)
    {
        Position = startPos;
        SpawnPoint = startPos;
        fallStartY = startPos.Y;

        // Стартовый PvP набор COBRA
        Hotbar[0] = new ItemStack(ItemType.NetheriteSword, 1);
        Hotbar[1] = new ItemStack(ItemType.EnderPearl, 16);
        Hotbar[2] = new ItemStack(ItemType.GoldenApple, 16);
        Hotbar[3] = new ItemStack(ItemType.EnchantedGoldenApple, 4);
        Hotbar[4] = new ItemStack(ItemType.TrapBoxCobra, 5);
        Hotbar[5] = new ItemStack(ItemType.SphereChaos, 1);
        Hotbar[6] = new ItemStack(ItemType.TalismanCobra, 1);
        Hotbar[7] = new ItemStack(ItemType.Plast, 8);
        Hotbar[8] = new ItemStack(ItemType.MysticSummon, 2);

        OffHand = new ItemStack(ItemType.TalismanPhoenix, 1);
    }

    public void AddStatusEffect(StatusEffectType type, float duration, int amplifier = 0)
    {
        ActiveEffects[type] = new ActiveEffect(duration, amplifier);
    }

    public bool HasStatusEffect(StatusEffectType type) => ActiveEffects.ContainsKey(type) && ActiveEffects[type].Duration > 0f;

    public void UpdateStatusEffects(float dt)
    {
        var keys = new List<StatusEffectType>(ActiveEffects.Keys);
        foreach (var key in keys)
        {
            ActiveEffects[key].Duration -= dt;
            if (ActiveEffects[key].Duration <= 0f)
            {
                ActiveEffects.Remove(key);
            }
        }

        // Логика Замедленного падения (Slow Falling)
        if (HasStatusEffect(StatusEffectType.SlowFalling))
        {
            NoFallDamage = true;
            if (Velocity.Y < -2.2f)
            {
                Velocity.Y = -2.2f;
            }
            if (Raylib.GetRandomValue(0, 3) == 0)
            {
                ParticleSystem.SpawnAuraParticle(Position, new Color(255, 255, 255, 180), 0.4f);
            }
        }
    }

    public void SwingHand()
    {
        IsSwinging = true;
        HandSwingProgress = 0f;
    }

    public void StartEating()
    {
        ItemStack sel = GetSelectedStack();
        if (sel.IsEmpty || !sel.IsFood) return;

        var cdType = CooldownSystem.GetCooldownType(sel.Type);
        if (cdType.HasValue && Cooldowns.IsOnCooldown(cdType.Value))
        {
            Program.AddChatMessage($"[FunTime] Предмет перезаряжается! Осталось: {Cooldowns.GetRemaining(cdType.Value):F1}с", Color.Red);
            return;
        }

        IsEating = true;
        EatTimer = 0f;
    }

    public void StopEating()
    {
        IsEating = false;
        EatTimer = 0f;
    }

    public void FinishEating()
    {
        ItemStack sel = GetSelectedStack();
        if (sel.IsEmpty) return;

        if (sel.Type == ItemType.GoldenApple)
        {
            AbsorptionHealth = 4.0f;
            Health = Math.Min(Health + 4.0f, MaxHealth);
            Hunger = 20f;
            Cooldowns.Trigger(CooldownType.GoldenApple, CooldownSystem.CD_GoldenApple);
            ParticleSystem.SpawnTotemEffect(Position);
            SoundManager.PlayTotem();
            Program.AddChatMessage("[FunTime] » Вы съели Золотое Яблоко!", Color.Gold);
            BP.AddProgress("eat_gapples", 1, this);
        }
        else if (sel.Type == ItemType.EnchantedGoldenApple)
        {
            AbsorptionHealth = 16.0f;
            Health = MaxHealth;
            Hunger = 20f;
            Cooldowns.Trigger(CooldownType.EnchantedGoldenApple, CooldownSystem.CD_EnchantedGoldenApple);
            ParticleSystem.SpawnTotemEffect(Position);
            SoundManager.PlayTotem();
            Program.AddChatMessage("[FunTime] » Вы съели Чарку! (Поглощение V, Регенерация II)", Color.Magenta);
        }
        else if (sel.Type == ItemType.ChorusFruit)
        {
            Position += new Vector3(Raylib.GetRandomValue(-8, 8), 0, Raylib.GetRandomValue(-8, 8));
            SoundManager.PlayTeleport();
        }
        else
        {
            Hunger = Math.Min(20f, Hunger + 8f);
            Health = Math.Min(Health + 2.0f, MaxHealth);
            SoundManager.PlayPlace();
        }

        Hotbar[SelectedSlot].Count--;
        if (Hotbar[SelectedSlot].Count <= 0) Hotbar[SelectedSlot] = new ItemStack();
        StopEating();
    }

    public void ThrowEnderPearl()
    {
        if (Cooldowns.IsOnCooldown(CooldownType.EnderPearl))
        {
            Program.AddChatMessage($"[FunTime] Эндер-жемчуг на перезарядке! {Cooldowns.GetRemaining(CooldownType.EnderPearl):F1}с", Color.Red);
            return;
        }

        Vector3 spawn = Position + new Vector3(0, 1.5f, 0) + Forward * 0.5f;
        Vector3 vel = Forward * 24.0f + new Vector3(0, 3.5f, 0);
        ActivePearls.Add(new Projectile { Position = spawn, Velocity = vel });

        Cooldowns.Trigger(CooldownType.EnderPearl, CooldownSystem.CD_EnderPearl);
        Hotbar[SelectedSlot].Count--;
        if (Hotbar[SelectedSlot].Count <= 0) Hotbar[SelectedSlot] = new ItemStack();

        BP.AddProgress("throw_pearls", 1, this);
        SwingHand();
        SoundManager.PlayTeleport();
    }

    public void DeployTrap(World world, ItemType trapType)
    {
        if (world.IsInSafeZone(Position))
        {
            Program.AddChatMessage("[FunTime] Нельзя ставить трапки в безопасной зоне спавна!", Color.Red);
            return;
        }

        if (Cooldowns.IsOnCooldown(CooldownType.TrapBox))
        {
            Program.AddChatMessage($"[FunTime] Трапка на перезарядке! {Cooldowns.GetRemaining(CooldownType.TrapBox):F1}с", Color.Red);
            return;
        }

        int px = (int)MathF.Floor(Position.X);
        int py = (int)MathF.Floor(Position.Y);
        int pz = (int)MathF.Floor(Position.Z);

        ItemType blockToPlace = trapType switch
        {
            ItemType.TrapBoxCobra => ItemType.CryingObsidian,
            ItemType.TrapBoxNetherite => ItemType.NetheriteOre,
            ItemType.TrapBoxCryo => ItemType.Snow,
            _ => ItemType.Obsidian
        };

        for (int x = -1; x <= 1; x++)
        for (int y = -1; y <= 2; y++)
        for (int z = -1; z <= 1; z++)
        {
            if (x == 0 && (y == 0 || y == 1) && z == 0) continue;
            world.SetBlock(px + x, py + y, pz + z, blockToPlace);
            NetworkManager.BroadcastBlockChange(px + x, py + y, pz + z, blockToPlace);
        }

        Cooldowns.Trigger(CooldownType.TrapBox, CooldownSystem.CD_TrapBox);
        Hotbar[SelectedSlot].Count--;
        if (Hotbar[SelectedSlot].Count <= 0) Hotbar[SelectedSlot] = new ItemStack();

        BP.AddProgress("place_traps", 1, this);
        SoundManager.PlayPlace();
        Program.AddChatMessage($"[FunTime] » Трапка ({BlockRegistry.GetName(trapType)}) успешно установлена!", Color.Magenta);
    }

    public void DeployPlast(World world)
    {
        if (world.IsInSafeZone(Position))
        {
            Program.AddChatMessage("[FunTime] Нельзя ставить пласт в безопасной зоне спавна!", Color.Red);
            return;
        }

        if (Cooldowns.IsOnCooldown(CooldownType.Plast))
        {
            Program.AddChatMessage($"[FunTime] Пласт на перезарядке! {Cooldowns.GetRemaining(CooldownType.Plast):F1}с", Color.Red);
            return;
        }

        Vector3 front = Position + Forward * 2f;
        int fx = (int)MathF.Floor(front.X);
        int fy = (int)MathF.Floor(front.Y);
        int fz = (int)MathF.Floor(front.Z);

        bool alignAlongZ = MathF.Abs(Forward.X) > MathF.Abs(Forward.Z);

        for (int i = -1; i <= 1; i++)
        for (int y = 0; y <= 2; y++)
        {
            int bx = alignAlongZ ? fx : fx + i;
            int bz = alignAlongZ ? fz + i : fz;
            world.SetBlock(bx, fy + y, bz, ItemType.Obsidian);
            NetworkManager.BroadcastBlockChange(bx, fy + y, bz, ItemType.Obsidian);
        }

        Cooldowns.Trigger(CooldownType.Plast, CooldownSystem.CD_Plast);
        Hotbar[SelectedSlot].Count--;
        if (Hotbar[SelectedSlot].Count <= 0) Hotbar[SelectedSlot] = new ItemStack();

        SoundManager.PlayPlace();
        Program.AddChatMessage("[FunTime] » Стена пласта возведена!", Color.Yellow);
    }

    public void TakeDamage(float amount, World? world = null)
    {
        if (amount <= 0 || IsFlying) return;
        if (world != null && world.IsInSafeZone(Position)) return;

        amount *= DamageResistance;
        CombatTagTimer = 15.0f;

        if (AbsorptionHealth > 0)
        {
            float rem = amount - AbsorptionHealth;
            AbsorptionHealth = Math.Max(0, AbsorptionHealth - amount);
            amount = Math.Max(0, rem);
        }

        Health -= amount;
        regenCooldown = 3.5f;

        if (Health <= 0)
        {
            if (OffHand.Type == ItemType.TalismanPhoenix || OffHand.Type == ItemType.TotemOfUndying)
            {
                Health = MaxHealth;
                AbsorptionHealth = 4.0f;
                ParticleSystem.SpawnTotemEffect(Position);
                SoundManager.PlayTotem();
                Program.AddChatMessage("[FunTime] » Тотем/Талисман спас вас от верной гибели!", Color.Gold);

                if (OffHand.Type == ItemType.TotemOfUndying)
                    OffHand = new ItemStack();
                return;
            }
            Program.AddChatMessage("[FunTime] » Вы погибли и возродились на спавне!", Color.Red);
            Respawn();
        }
    }

    public void Respawn()
    {
        Position = SpawnPoint;
        Velocity = Vector3.Zero;
        Health = MaxHealth;
        AbsorptionHealth = 0;
        Hunger = 20f;
        CombatTagTimer = 0f;
        fallStartY = SpawnPoint.Y;
        wasGrounded = true;
        ActiveEffects.Clear();
    }

    public ItemStack GetSelectedStack() => Hotbar[SelectedSlot];

    public Vector3 Forward
    {
        get
        {
            float yawRad = Yaw * Raylib.DEG2RAD;
            float pitchRad = Pitch * Raylib.DEG2RAD;
            return Vector3.Normalize(new Vector3(
                -MathF.Sin(yawRad) * MathF.Cos(pitchRad),
                MathF.Sin(pitchRad),
                MathF.Cos(yawRad) * MathF.Cos(pitchRad)
            ));
        }
    }

    public Camera3D GetCamera()
    {
        float targetFov = CameraFov;
        if (IsSprinting) targetFov += 9.0f;
        if (ActiveSpeedMultiplier > 1.2f) targetFov += 8.0f;
        if (IsEating) targetFov -= 4.0f;

        currentFov += (targetFov - currentFov) * 0.15f;
        float eyeY = IsSneaking ? 1.42f : 1.62f;

        if (IsEating)
        {
            eyeY += MathF.Sin(EatTimer * 20f) * 0.03f;
        }

        return new Camera3D
        {
            Position = Position + new Vector3(0, eyeY, 0),
            Target = Position + new Vector3(0, eyeY, 0) + Forward,
            Up = new Vector3(0, 1, 0),
            FovY = currentFov,
            Projection = CameraProjection.Perspective
        };
    }

    public BoxAABB GetBoundingBox(Vector3 pos) => BoxAABB.FromPlayer(pos, PlayerWidth, PlayerHeight);

    public void Update(float deltaTime, World world)
    {
        Cooldowns.Update(deltaTime);
        UpdateStatusEffects(deltaTime);

        if (CombatTagTimer > 0) CombatTagTimer -= deltaTime;
        AttackCooldownProgress = Math.Min(1.0f, AttackCooldownProgress + deltaTime * 2.5f);

        // Логика поедания
        if (IsEating)
        {
            EatTimer += deltaTime;
            if (Raylib.GetRandomValue(0, 2) == 0)
                ParticleSystem.SpawnAuraParticle(Position + Forward * 0.4f + new Vector3(0, 1.4f, 0), Color.Gold, 0.15f);

            if (EatTimer >= MaxEatDuration)
            {
                FinishEating();
            }
        }

        // Обновление летящих эндер-перлов
        for (int i = ActivePearls.Count - 1; i >= 0; i--)
        {
            var p = ActivePearls[i];
            p.Life -= deltaTime;
            p.Velocity.Y -= 22f * deltaTime;
            Vector3 nextPos = p.Position + p.Velocity * deltaTime;

            int bx = (int)MathF.Floor(nextPos.X);
            int by = (int)MathF.Floor(nextPos.Y);
            int bz = (int)MathF.Floor(nextPos.Z);

            if (world.IsSolid(bx, by, bz) || p.Life <= 0)
            {
                Position = p.Position + new Vector3(0, 0.2f, 0);
                Velocity = Vector3.Zero;
                TakeDamage(2.0f, world);
                ParticleSystem.SpawnTotemEffect(Position);
                SoundManager.PlayTeleport();
                ActivePearls.RemoveAt(i);
                continue;
            }

            p.Position = nextPos;
            ParticleSystem.SpawnAuraParticle(p.Position, new Color(40, 220, 180, 255), 0.2f);
            ActivePearls[i] = p;
        }

        // Анимация взмаха рукой
        if (IsSwinging)
        {
            HandSwingProgress += deltaTime * 5.5f;
            if (HandSwingProgress >= 1.0f)
            {
                HandSwingProgress = 0f;
                IsSwinging = false;
            }
        }

        // Смена предмета в левую руку по клавише F
        if (Raylib.IsKeyPressed(KeyboardKey.F) && !IsInventoryOpen && !IsChestOpen && !IsChatOpen && !IsGUIOpen)
        {
            ItemStack temp = Hotbar[SelectedSlot];
            Hotbar[SelectedSlot] = OffHand;
            OffHand = temp;
            SwingHand();
            SoundManager.PlayPlace();
        }

        UpdateFunTimeBuffs();
        EmitSphereParticles();

        // Управление инвентарем и GUI
        if (Raylib.IsKeyPressed(KeyboardKey.E) || Raylib.IsKeyPressed(KeyboardKey.Tab))
        {
            if (!IsChatOpen)
            {
                if (IsChestOpen) IsChestOpen = false;
                else if (IsGUIOpen) IsGUIOpen = false;
                else IsInventoryOpen = !IsInventoryOpen;

                if (IsInventoryOpen || IsChestOpen || IsGUIOpen) Raylib.EnableCursor();
                else Raylib.DisableCursor();
            }
        }

        // Смена активного слота Хотбара
        for (int i = 0; i < 9; i++)
        {
            if (Raylib.IsKeyPressed((KeyboardKey)((int)KeyboardKey.One + i))) SelectedSlot = i;
        }
        float wheel = Raylib.GetMouseWheelMove();
        if (wheel > 0) SelectedSlot = (SelectedSlot - 1 + 9) % 9;
        if (wheel < 0) SelectedSlot = (SelectedSlot + 1) % 9;

        if (IsInventoryOpen || IsChestOpen || IsChatOpen || IsGUIOpen) return;

        // Вращение камеры
        Vector2 mouseDelta = Raylib.GetMouseDelta();
        Yaw += mouseDelta.X * MouseSensitivity;
        float pMul = InvertMouseY ? 1.0f : -1.0f;
        Pitch += mouseDelta.Y * MouseSensitivity * pMul;
        Pitch = Math.Clamp(Pitch, -89.0f, 89.0f);

        float yawRad = Yaw * Raylib.DEG2RAD;
        Vector3 forwardDir = new(-MathF.Sin(yawRad), 0, MathF.Cos(yawRad));
        Vector3 rightDir = new(-MathF.Cos(yawRad), 0, -MathF.Sin(yawRad));

        Vector3 moveDir = Vector3.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.W)) moveDir += forwardDir;
        if (Raylib.IsKeyDown(KeyboardKey.S)) moveDir -= forwardDir;
        if (Raylib.IsKeyDown(KeyboardKey.D)) moveDir += rightDir;
        if (Raylib.IsKeyDown(KeyboardKey.A)) moveDir -= rightDir;

        if (moveDir != Vector3.Zero) moveDir = Vector3.Normalize(moveDir);

        IsSneaking = Raylib.IsKeyDown(KeyboardKey.LeftShift) || Raylib.IsKeyDown(KeyboardKey.RightShift);
        bool wantSprint = Raylib.IsKeyDown(KeyboardKey.LeftControl) || Raylib.IsKeyDown(KeyboardKey.RightControl);
        IsSprinting = wantSprint && !IsSneaking && moveDir != Vector3.Zero && Raylib.IsKeyDown(KeyboardKey.W);

        float speedMul = ActiveSpeedMultiplier;
        if (IsSprinting) speedMul *= SprintMultiplier;
        if (IsSneaking) speedMul *= SneakMultiplier;
        if (IsEating) speedMul *= 0.4f;
        if (IsFlying) speedMul *= 2.2f;

        float activeSpeed = BaseSpeed * speedMul;
        Velocity.X = moveDir.X * activeSpeed;
        Velocity.Z = moveDir.Z * activeSpeed;

        if (IsFlying)
        {
            Velocity.Y = 0;
            if (Raylib.IsKeyDown(KeyboardKey.Space)) Velocity.Y = activeSpeed;
            if (Raylib.IsKeyDown(KeyboardKey.LeftShift)) Velocity.Y = -activeSpeed;
        }
        else
        {
            Velocity.Y -= ActiveGravity * deltaTime;
            Velocity.Y *= MathF.Pow(0.98f, 60f * deltaTime);
            if (Velocity.Y < -78.4f) Velocity.Y = -78.4f;

            bool holdingJump = Raylib.IsKeyDown(KeyboardKey.Space);
            if (IsGrounded && holdingJump)
            {
                Velocity.Y = ActiveJumpForce;
                IsGrounded = false;
                SoundManager.PlayJump();
            }
        }

        // Вызов непрерывного физического решателя без застреваний
        PhysicsEngine.MovePlayer(this, world, deltaTime);

        if (IsSneaking && IsGrounded && !IsFlying) SneakEdgeProtection(world);

        // Расчет урона от падения
        if (wasGrounded && !IsGrounded) fallStartY = Position.Y;
        if (!IsGrounded && Position.Y > fallStartY) fallStartY = Position.Y;
        if (!wasGrounded && IsGrounded && !IsFlying)
        {
            float fallDist = fallStartY - Position.Y;
            if (fallDist > 3.5f && !NoFallDamage)
            {
                TakeDamage(MathF.Floor(fallDist - 3.0f), world);
            }
            fallStartY = Position.Y;
        }
        wasGrounded = IsGrounded;

        // Пассивная регенерация здоровья
        regenCooldown -= deltaTime;
        if (regenCooldown <= 0 && Health < MaxHealth)
            Health = Math.Min(MaxHealth, Health + regenRate * deltaTime);

        if (Position.Y < -5f) Respawn();
    }

    private void EmitSphereParticles()
    {
        if (OffHand.IsEmpty) return;

        Color auraCol = OffHand.Type switch
        {
            ItemType.SphereAres or ItemType.TalismanPunisher => new Color(255, 60, 40, 220),
            ItemType.SphereScythian or ItemType.TalismanCobra => new Color(0, 240, 255, 220),
            ItemType.SphereAstraea or ItemType.TalismanAegis => new Color(80, 140, 255, 220),
            ItemType.SphereChaos => new Color(210, 40, 255, 220),
            ItemType.SphereEris or ItemType.TalismanCrusher => new Color(255, 200, 20, 220),
            ItemType.TalismanHarmony or ItemType.TalismanArchangel => new Color(80, 255, 120, 220),
            ItemType.TalismanPhoenix or ItemType.TotemOfUndying => new Color(255, 180, 30, 220),
            _ => Color.Blank
        };

        if (auraCol.A > 0 && Raylib.GetRandomValue(0, 2) == 0)
        {
            ParticleSystem.SpawnAuraParticle(Position, auraCol);
        }
    }

    private void UpdateFunTimeBuffs()
    {
        ActiveSpeedMultiplier = 1.0f;
        ActiveJumpForce = BaseJumpForce;
        ActiveGravity = Gravity;
        MiningSpeedMultiplier = 1.0f;
        NoFallDamage = false;
        MaxHealth = 20f;
        regenRate = 0.5f;
        DamageBoost = 1.0f;
        DamageResistance = 1.0f;

        if (Rank >= PlayerRank.Titan) ActiveSpeedMultiplier += 0.05f;
        if (Rank >= PlayerRank.Dragon) MaxHealth += 2f;
        if (Rank >= PlayerRank.Sponsor) { DamageBoost += 0.1f; regenRate += 0.3f; }
        if (Rank == PlayerRank.Cobra) { MaxHealth += 4f; ActiveSpeedMultiplier += 0.10f; }

        switch (OffHand.Type)
        {
            case ItemType.SphereAres:
                DamageBoost += 0.75f;
                ActiveSpeedMultiplier += 0.15f;
                break;
            case ItemType.SphereScythian:
                ActiveSpeedMultiplier += 1.05f;
                break;
            case ItemType.SphereAstraea:
                ActiveJumpForce = 15.5f;
                ActiveGravity = 14.0f;
                NoFallDamage = true;
                break;
            case ItemType.SphereChaos:
                ActiveSpeedMultiplier += 0.4f;
                ActiveJumpForce = 12.0f;
                DamageBoost += 0.35f;
                regenRate += 1.5f;
                break;
            case ItemType.SphereEris:
                MiningSpeedMultiplier = 4.8f;
                ActiveSpeedMultiplier += 0.2f;
                break;
            case ItemType.SphereTitan:
                DamageResistance = 0.5f;
                MaxHealth += 4f;
                break;

            case ItemType.TalismanCrusher:
                MaxHealth += 8f;
                ActiveSpeedMultiplier += 0.35f;
                ActiveJumpForce = 11.5f;
                DamageBoost += 0.5f;
                NoFallDamage = true;
                regenRate += 1.2f;
                break;
            case ItemType.TalismanPunisher:
                DamageBoost += 0.8f;
                ActiveSpeedMultiplier += 0.3f;
                break;
            case ItemType.TalismanHarmony:
                MaxHealth += 4f;
                ActiveSpeedMultiplier += 0.25f;
                DamageBoost += 0.25f;
                regenRate += 2.0f;
                break;
            case ItemType.TalismanAegis:
                NoFallDamage = true;
                ActiveGravity = 15.0f;
                MaxHealth += 10f;
                DamageResistance = 0.7f;
                break;
            case ItemType.TalismanArchangel:
                MaxHealth += 14f;
                regenRate += 3.5f;
                ActiveSpeedMultiplier += 0.2f;
                break;
            case ItemType.TalismanCobra:
                ActiveSpeedMultiplier += 0.5f;
                DamageBoost += 0.45f;
                ActiveJumpForce = 11.0f;
                break;
            case ItemType.TalismanPhoenix:
                MaxHealth += 2f;
                regenRate += 1.0f;
                break;
        }

        Health = Math.Min(Health, MaxHealth);
    }

    public bool TryAddItem(ItemType type, int count = 1)
    {
        if (type == ItemType.None) return false;

        for (int i = 0; i < 9; i++)
        {
            if (Hotbar[i].Type == type && Hotbar[i].Count < 64)
            {
                int add = Math.Min(64 - Hotbar[i].Count, count);
                Hotbar[i].Count += add;
                count -= add;
                if (count <= 0) return true;
            }
        }
        for (int i = 0; i < 27; i++)
        {
            if (MainInventory[i].Type == type && MainInventory[i].Count < 64)
            {
                int add = Math.Min(64 - MainInventory[i].Count, count);
                MainInventory[i].Count += add;
                count -= add;
                if (count <= 0) return true;
            }
        }
        for (int i = 0; i < 9; i++)
        {
            if (Hotbar[i].IsEmpty)
            {
                Hotbar[i] = new ItemStack(type, count);
                return true;
            }
        }
        for (int i = 0; i < 27; i++)
        {
            if (MainInventory[i].IsEmpty)
            {
                MainInventory[i] = new ItemStack(type, count);
                return true;
            }
        }
        return false;
    }

    private void SneakEdgeProtection(World world)
    {
        float hw = PlayerWidth / 2.0f;
        int feetY = (int)MathF.Floor(Position.Y - 0.1f);

        float[] cornersX = { Position.X - hw + 0.01f, Position.X + hw - 0.01f };
        float[] cornersZ = { Position.Z - hw + 0.01f, Position.Z + hw - 0.01f };

        bool hasPX = false, hasNX = false, hasPZ = false, hasNZ = false;

        foreach (float cz in cornersZ)
        {
            if (world.IsSolid((int)MathF.Floor(Position.X + hw), feetY, (int)MathF.Floor(cz))) hasPX = true;
            if (world.IsSolid((int)MathF.Floor(Position.X - hw), feetY, (int)MathF.Floor(cz))) hasNX = true;
        }
        foreach (float cx in cornersX)
        {
            if (world.IsSolid((int)MathF.Floor(cx), feetY, (int)MathF.Floor(Position.Z + hw))) hasPZ = true;
            if (world.IsSolid((int)MathF.Floor(cx), feetY, (int)MathF.Floor(Position.Z - hw))) hasNZ = true;
        }

        if (!hasPX) Position.X = MathF.Floor(Position.X + hw) - hw;
        if (!hasNX) Position.X = MathF.Floor(Position.X - hw) + 1 + hw;
        if (!hasPZ) Position.Z = MathF.Floor(Position.Z + hw) - hw;
        if (!hasNZ) Position.Z = MathF.Floor(Position.Z - hw) + 1 + hw;
    }
}