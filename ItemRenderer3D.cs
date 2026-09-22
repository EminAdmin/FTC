using System;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public static class ItemRenderer3D
{
    private static float animTime = 0f;

    public static void Update(float dt)
    {
        animTime += dt;
    }

    public static void RenderFirstPersonHands3D(Player player, int sw, int sh)
    {
        if (player == null) return;

        Camera3D handCam = new()
        {
            Position = Vector3.Zero,
            Target = new Vector3(0, 0, 1),
            Up = new Vector3(0, 1, 0),
            FovY = 65.0f,
            Projection = CameraProjection.Perspective
        };

        // 3D-пасс для отрисовки объемных рук и предметов от 1-го лица
        Raylib.BeginMode3D(handCam);

        float speed = player.Velocity.Length();
        float bob = (player.IsGrounded && speed > 0.5f) ? MathF.Sin(animTime * 10f) * 0.025f : 0f;
        float bobSide = (player.IsGrounded && speed > 0.5f) ? MathF.Cos(animTime * 5f) * 0.015f : 0f;

        if (player.IsEating)
        {
            bob += MathF.Sin(player.EatTimer * 25f) * 0.02f;
        }

        // 1. ПРАВАЯ РУКА / ОСНОВНОЙ ПРЕДМЕТ
        ItemStack mainStack = player.GetSelectedStack();
        float swingAngle = player.IsSwinging ? MathF.Sin(player.HandSwingProgress * MathF.PI) : 0f;

        Vector3 mainHandPos = new(0.42f + bobSide, -0.32f + bob - swingAngle * 0.15f, 0.65f - swingAngle * 0.1f);
        RenderItem3D(mainStack, mainHandPos, swingAngle, false);

        // 2. ЛЕВАЯ РУКА / OFFHAND ПРЕДМЕТ (СФЕРА / ТАЛИСМАН / ТОТЕМ)
        if (!player.OffHand.IsEmpty)
        {
            Vector3 offHandPos = new(-0.42f - bobSide, -0.32f + bob, 0.65f);
            RenderItem3D(player.OffHand, offHandPos, 0f, true);
        }

        Raylib.EndMode3D();
    }

    public static void RenderItem3D(ItemStack stack, Vector3 position, float swingProgress, bool isOffhand)
    {
        if (stack.IsEmpty)
        {
            DrawVoxelArm(position, isOffhand, swingProgress);
            return;
        }

        ItemType type = stack.Type;

        if (type >= ItemType.SphereAres && type <= ItemType.SphereTitan)
        {
            Draw3DSphere(type, position, isOffhand);
        }
        else if (type >= ItemType.TalismanCrusher && type <= ItemType.TalismanLeviathan)
        {
            Draw3DTalisman(type, position, isOffhand);
        }
        else if (type == ItemType.DiamondSword || type == ItemType.NetheriteSword)
        {
            Draw3DSword(type, position, swingProgress, isOffhand);
        }
        else if (stack.IsPlaceable)
        {
            Draw3DBlockCube(type, position, swingProgress, isOffhand);
        }
        else
        {
            Draw3DExtrudedItem(type, position, swingProgress, isOffhand);
        }
    }

    private static void DrawVoxelArm(Vector3 pos, bool isOffhand, float swing)
    {
        Vector3 armPos = pos + new Vector3(0, -0.05f, 0);

        Color skinCol = new(198, 137, 101, 255);
        Color sleeveCol = isOffhand ? new Color(40, 80, 160, 255) : new Color(20, 140, 80, 255);

        Raylib.DrawCube(armPos, 0.14f, 0.28f, 0.14f, skinCol);
        Raylib.DrawCube(armPos + new Vector3(0, -0.15f, 0), 0.15f, 0.18f, 0.15f, sleeveCol);
        Raylib.DrawCubeWires(armPos, 0.142f, 0.282f, 0.142f, new Color(130, 80, 50, 255));
    }

    private static void Draw3DSphere(ItemType type, Vector3 pos, bool isOffhand)
    {
        float spin = animTime * 80f;
        float pulse = 1.0f + MathF.Sin(animTime * 6f) * 0.05f;

        (Color coreCol, Color glowCol, Color auraCol) = type switch
        {
            ItemType.SphereAres => (new Color(255, 230, 60, 255), new Color(240, 50, 20, 255), new Color(255, 60, 30, 160)),
            ItemType.SphereScythian => (new Color(220, 255, 255, 255), new Color(30, 220, 240, 255), new Color(0, 240, 255, 160)),
            ItemType.SphereAstraea => (new Color(255, 255, 255, 255), new Color(60, 90, 240, 255), new Color(80, 140, 255, 160)),
            ItemType.SphereChaos => (new Color(255, 120, 255, 255), new Color(160, 20, 220, 255), new Color(200, 30, 255, 160)),
            ItemType.SphereEris => (new Color(255, 245, 120, 255), new Color(250, 150, 20, 255), new Color(255, 190, 20, 160)),
            ItemType.SphereTitan => (new Color(220, 240, 255, 255), new Color(110, 130, 150, 255), new Color(160, 180, 210, 160)),
            _ => (Color.White, Color.Gold, Color.Yellow)
        };

        // Внутреннее светящееся ядро сферы
        Raylib.DrawSphere(pos, 0.09f * pulse, coreCol);

        // Внешняя кристаллическая оболочка сферы
        Raylib.DrawSphereWires(pos, 0.11f * pulse, 8, 8, glowCol);
        Raylib.DrawCubeWires(pos, 0.13f * pulse, 0.13f * pulse, 0.13f * pulse, auraCol);

        // Вращающиеся орбитальные рунические кольца
        int ringCount = 8;
        for (int i = 0; i < ringCount; i++)
        {
            float angle = (i / (float)ringCount) * MathF.PI * 2f + spin * Raylib.DEG2RAD;
            float r = 0.16f;
            Vector3 ringPos = pos + new Vector3(MathF.Cos(angle) * r, MathF.Sin(angle * 2f) * 0.04f, MathF.Sin(angle) * r);
            Raylib.DrawCube(ringPos, 0.022f, 0.022f, 0.022f, coreCol);
        }
    }

    private static void Draw3DTalisman(ItemType type, Vector3 pos, bool isOffhand)
    {
        float floatOffset = MathF.Sin(animTime * 4f) * 0.015f;
        Vector3 tPos = pos + new Vector3(0, floatOffset, 0);

        (Color frameCol, Color gemCol, Color runeCol) = type switch
        {
            ItemType.TalismanCobra => (new Color(20, 100, 40, 255), new Color(40, 230, 90, 255), new Color(255, 30, 60, 255)),
            ItemType.TalismanPhoenix => (Color.Gold, new Color(255, 80, 20, 255), new Color(255, 240, 100, 255)),
            ItemType.TalismanArchangel => (Color.Gold, new Color(240, 245, 255, 255), new Color(140, 220, 255, 255)),
            ItemType.TalismanLeviathan => (new Color(20, 70, 180, 255), new Color(0, 240, 255, 255), Color.White),
            ItemType.TalismanPunisher => (new Color(40, 10, 15, 255), new Color(220, 20, 40, 255), Color.White),
            ItemType.TalismanCrusher => (Color.Gold, new Color(190, 30, 30, 255), Color.Gold),
            ItemType.TalismanHarmony => (new Color(30, 180, 90, 255), Color.White, new Color(40, 240, 120, 255)),
            ItemType.TalismanAegis => (new Color(200, 200, 210, 255), new Color(30, 80, 180, 255), Color.Gold),
            _ => (Color.Gold, Color.Lime, Color.White)
        };

        // Золотая оправа амулета (3D-медальон)
        Raylib.DrawCube(tPos, 0.20f, 0.24f, 0.035f, frameCol);
        Raylib.DrawCubeWires(tPos, 0.204f, 0.244f, 0.037f, Color.Gold);

        // Выступающий граненый драгоценный камень в центре
        Raylib.DrawCube(tPos + new Vector3(0, 0, 0.02f), 0.11f, 0.11f, 0.025f, gemCol);
        Raylib.DrawCubeWires(tPos + new Vector3(0, 0, 0.02f), 0.112f, 0.112f, 0.026f, runeCol);

        // Объемное верхнее кольцо подвески
        Raylib.DrawCube(tPos + new Vector3(0, 0.13f, 0), 0.045f, 0.045f, 0.025f, Color.Gold);

        // Уникальные 3D-детали талисманов
        if (type == ItemType.TalismanArchangel || type == ItemType.TalismanPhoenix)
        {
            Color wingCol = (type == ItemType.TalismanArchangel) ? Color.White : new Color(255, 120, 20, 255);
            Raylib.DrawCube(tPos + new Vector3(-0.13f, 0.03f, 0), 0.07f, 0.16f, 0.02f, wingCol);
            Raylib.DrawCube(tPos + new Vector3(0.13f, 0.03f, 0), 0.07f, 0.16f, 0.02f, wingCol);
        }
        else if (type == ItemType.TalismanCobra)
        {
            Raylib.DrawCube(tPos + new Vector3(-0.04f, 0.04f, 0.03f), 0.022f, 0.022f, 0.02f, new Color(255, 20, 50, 255));
            Raylib.DrawCube(tPos + new Vector3(0.04f, 0.04f, 0.03f), 0.022f, 0.022f, 0.02f, new Color(255, 20, 50, 255));
        }
    }

    private static void Draw3DSword(ItemType type, Vector3 pos, float swing, bool isOffhand)
    {
        Color bladeCol = (type == ItemType.NetheriteSword) ? new Color(60, 55, 65, 255) : new Color(90, 230, 225, 255);
        Color edgeCol = (type == ItemType.NetheriteSword) ? new Color(110, 100, 120, 255) : new Color(200, 255, 255, 255);

        Vector3 sPos = pos + new Vector3(0, 0.05f - swing * 0.1f, -swing * 0.08f);

        // 3D Клинок меча
        Raylib.DrawCube(sPos + new Vector3(0, 0.18f, 0), 0.045f, 0.45f, 0.015f, bladeCol);
        Raylib.DrawCubeWires(sPos + new Vector3(0, 0.18f, 0), 0.047f, 0.452f, 0.017f, edgeCol);

        // 3D Гарда меча
        Raylib.DrawCube(sPos + new Vector3(0, -0.04f, 0), 0.15f, 0.035f, 0.035f, Color.Gold);

        // 3D Рукоять и навершие
        Raylib.DrawCube(sPos + new Vector3(0, -0.12f, 0), 0.03f, 0.13f, 0.03f, new Color(110, 65, 25, 255));
        Raylib.DrawCube(sPos + new Vector3(0, -0.19f, 0), 0.045f, 0.04f, 0.04f, Color.Gold);
    }

    private static void Draw3DBlockCube(ItemType type, Vector3 pos, float swing, bool isOffhand)
    {
        float size = 0.22f;
        Vector3 bPos = pos + new Vector3(0, 0, 0);

        Color blockCol = type switch
        {
            ItemType.Obsidian or ItemType.TrapBox => new Color(30, 20, 45, 255),
            ItemType.CryingObsidian or ItemType.TrapBoxCobra => new Color(70, 20, 110, 255),
            ItemType.Glowstone => new Color(250, 210, 100, 255),
            ItemType.DiamondOre => new Color(90, 230, 225, 255),
            ItemType.Grass => new Color(92, 175, 54, 255),
            ItemType.Stone or ItemType.StoneBricks => new Color(130, 130, 132, 255),
            ItemType.CraftingTable => new Color(180, 135, 80, 255),
            ItemType.Chest or ItemType.MysticChest => new Color(200, 60, 240, 255),
            _ => new Color(150, 150, 150, 255)
        };

        Raylib.DrawCube(bPos, size, size, size, blockCol);
        Raylib.DrawCubeWires(bPos, size + 0.002f, size + 0.002f, size + 0.002f, Color.Black);
    }

    private static void Draw3DExtrudedItem(ItemType type, Vector3 pos, float swing, bool isOffhand)
    {
        Color itemCol = type switch
        {
            ItemType.EnderPearl => new Color(40, 220, 180, 255),
            ItemType.GoldenApple => Color.Gold,
            ItemType.EnchantedGoldenApple => new Color(255, 105, 180, 255),
            ItemType.TotemOfUndying => Color.Gold,
            ItemType.Plast => new Color(134, 96, 67, 255),
            ItemType.MoneyVoucher50k => new Color(50, 220, 100, 255),
            ItemType.MoneyVoucher250k => new Color(255, 180, 20, 255),
            _ => Color.White
        };

        Raylib.DrawCube(pos, 0.18f, 0.18f, 0.035f, itemCol);
        Raylib.DrawCubeWires(pos, 0.182f, 0.182f, 0.037f, Color.Gold);
    }
}