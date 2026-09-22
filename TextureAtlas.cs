using System;
using Raylib_cs;

namespace FunTimeCobra;

public static class TextureAtlas
{
    public const int TileSize = 16;
    public const int TilesPerRow = 16;

    public const int GrassTop = 0;
    public const int GrassSide = 1;
    public const int Dirt = 2;
    public const int Stone = 3;
    public const int Cobble = 4;
    public const int Sand = 5;
    public const int LogSide = 6;
    public const int LogTop = 7;
    public const int Planks = 8;
    public const int Leaves = 9;
    public const int Coal = 10;
    public const int Iron = 11;
    public const int Diamond = 12;
    public const int Gold = 13;
    public const int Redstone = 14;
    public const int Emerald = 15;
    public const int NetheriteOre = 16;
    public const int Brick = 17;
    public const int StoneBricks = 18;
    public const int Obsidian = 19;
    public const int CryingObsidian = 20;
    public const int Glowstone = 21;
    public const int Snow = 22;
    public const int Bedrock = 23;
    public const int Chest = 24;
    public const int MysticChest = 25;
    public const int Beacon = 26;

    public const int IconEnderPearl = 27;
    public const int IconGoldenApple = 28;
    public const int IconEnchantedGoldenApple = 29;
    public const int IconTotem = 30;
    public const int IconTrapBox = 31;
    public const int IconTrapBoxCobra = 32;
    public const int IconPlast = 33;
    public const int IconMysticSummon = 34;

    public const int IconDiamondSword = 35;
    public const int IconNetheriteSword = 36;
    public const int IconDiamondPickaxe = 37;
    public const int IconNetheritePickaxe = 38;

    public const int IconSphereAres = 40;
    public const int IconSphereScythian = 41;
    public const int IconSphereAstraea = 42;
    public const int IconSphereChaos = 43;
    public const int IconSphereEris = 44;
    public const int IconSphereTitan = 45;

    public const int IconTalismanCrusher = 48;
    public const int IconTalismanPunisher = 49;
    public const int IconTalismanHarmony = 50;
    public const int IconTalismanAegis = 51;
    public const int IconTalismanArchangel = 52;
    public const int IconTalismanCobra = 53;
    public const int IconTalismanPhoenix = 54;
    public const int IconTalismanLeviathan = 55;

    public const int IconVoucher50k = 56;
    public const int IconVoucher250k = 57;

    public static Texture2D AtlasTexture;
    public static int AtlasW, AtlasH;

    public static void Initialize()
    {
        int tileCount = 64;
        int rows = (tileCount + TilesPerRow - 1) / TilesPerRow;
        AtlasW = TilesPerRow * TileSize;
        AtlasH = rows * TileSize;

        Image atlas = Raylib.GenImageColor(AtlasW, AtlasH, Color.Blank);

        GenGrassTop(atlas);
        GenGrassSide(atlas);
        GenDirt(atlas);
        GenStone(atlas);
        GenCobble(atlas);
        GenSand(atlas);
        GenLogSide(atlas);
        GenLogTop(atlas);
        GenPlanks(atlas);
        GenLeaves(atlas);
        GenOre(atlas, Coal, new Color(30, 30, 30, 255), 16);
        GenOre(atlas, Iron, new Color(205, 175, 140, 255), 26);
        GenOre(atlas, Diamond, new Color(90, 230, 225, 255), 36);
        GenOre(atlas, Gold, new Color(245, 215, 60, 255), 48);
        GenOre(atlas, Redstone, new Color(235, 30, 30, 255), 58);
        GenOre(atlas, Emerald, new Color(30, 220, 90, 255), 68);
        GenNetheriteOre(atlas);
        GenBrick(atlas);
        GenStoneBricks(atlas);
        GenObsidian(atlas, Obsidian, new Color(25, 20, 38, 255));
        GenObsidian(atlas, CryingObsidian, new Color(60, 20, 90, 255));
        GenGlowstone(atlas);
        GenSnow(atlas);
        GenBedrock(atlas);
        GenChest(atlas, Chest, new Color(170, 120, 55, 255));
        GenChest(atlas, MysticChest, new Color(180, 50, 230, 255));
        GenBeacon(atlas);

        GenPearl(atlas, IconEnderPearl);
        GenApple(atlas, IconGoldenApple, new Color(255, 215, 0, 255));
        GenApple(atlas, IconEnchantedGoldenApple, new Color(255, 105, 180, 255));
        GenTotemIcon(atlas, IconTotem);
        GenTrapIcon(atlas, IconTrapBox, new Color(40, 30, 50, 255));
        GenTrapIcon(atlas, IconTrapBoxCobra, new Color(30, 120, 60, 255));
        GenPlastIcon(atlas, IconPlast);
        GenMysticSummonIcon(atlas, IconMysticSummon);

        GenSword(atlas, IconDiamondSword, new Color(90, 230, 225, 255));
        GenSword(atlas, IconNetheriteSword, new Color(60, 55, 65, 255));
        GenPickaxe(atlas, IconDiamondPickaxe, new Color(90, 230, 225, 255));
        GenPickaxe(atlas, IconNetheritePickaxe, new Color(60, 55, 65, 255));

        // Оригинальные аутентичные текстуры Сфер FunTime
        GenSphereAres(atlas, IconSphereAres);
        GenSphereScythian(atlas, IconSphereScythian);
        GenSphereAstraea(atlas, IconSphereAstraea);
        GenSphereChaos(atlas, IconSphereChaos);
        GenSphereEris(atlas, IconSphereEris);
        GenSphereTitan(atlas, IconSphereTitan);

        // Оригинальные аутентичные текстуры Талисманов FunTime
        GenTalismanCrusher(atlas, IconTalismanCrusher);
        GenTalismanPunisher(atlas, IconTalismanPunisher);
        GenTalismanHarmony(atlas, IconTalismanHarmony);
        GenTalismanAegis(atlas, IconTalismanAegis);
        GenTalismanArchangel(atlas, IconTalismanArchangel);
        GenTalismanCobra(atlas, IconTalismanCobra);
        GenTalismanPhoenix(atlas, IconTalismanPhoenix);
        GenTalismanLeviathan(atlas, IconTalismanLeviathan);

        GenVoucherIcon(atlas, IconVoucher50k, new Color(50, 220, 100, 255));
        GenVoucherIcon(atlas, IconVoucher250k, new Color(255, 180, 20, 255));

        AtlasTexture = Raylib.LoadTextureFromImage(atlas);
        Raylib.UnloadImage(atlas);
        Raylib.SetTextureFilter(AtlasTexture, TextureFilter.Point);
        Raylib.SetTextureWrap(AtlasTexture, TextureWrap.Clamp);
    }

    public static int GetTileIndex(ItemType type, int face = 0) => type switch
    {
        ItemType.Grass => face == 0 ? GrassTop : (face == 1 ? Dirt : GrassSide),
        ItemType.Dirt => Dirt,
        ItemType.Stone => Stone,
        ItemType.Cobblestone => Cobble,
        ItemType.Sand => Sand,
        ItemType.Log => face <= 1 ? LogTop : LogSide,
        ItemType.Planks => Planks,
        ItemType.Leaves => Leaves,
        ItemType.CoalOre => Coal,
        ItemType.IronOre => Iron,
        ItemType.DiamondOre => Diamond,
        ItemType.GoldOre => Gold,
        ItemType.RedstoneOre => Redstone,
        ItemType.EmeraldOre => Emerald,
        ItemType.NetheriteOre => NetheriteOre,
        ItemType.Brick => Brick,
        ItemType.StoneBricks => StoneBricks,
        ItemType.Obsidian => Obsidian,
        ItemType.CryingObsidian => CryingObsidian,
        ItemType.Glowstone => Glowstone,
        ItemType.Snow => Snow,
        ItemType.Bedrock => Bedrock,
        ItemType.Chest => Chest,
        ItemType.MysticChest => MysticChest,
        ItemType.Beacon => Beacon,

        ItemType.EnderPearl => IconEnderPearl,
        ItemType.GoldenApple => IconGoldenApple,
        ItemType.EnchantedGoldenApple => IconEnchantedGoldenApple,
        ItemType.TotemOfUndying => IconTotem,
        ItemType.TrapBox => IconTrapBox,
        ItemType.TrapBoxCobra => IconTrapBoxCobra,
        ItemType.Plast => IconPlast,
        ItemType.MysticSummon => IconMysticSummon,

        ItemType.DiamondSword => IconDiamondSword,
        ItemType.NetheriteSword => IconNetheriteSword,
        ItemType.DiamondPickaxe => IconDiamondPickaxe,
        ItemType.NetheritePickaxe => IconNetheritePickaxe,

        ItemType.SphereAres => IconSphereAres,
        ItemType.SphereScythian => IconSphereScythian,
        ItemType.SphereAstraea => IconSphereAstraea,
        ItemType.SphereChaos => IconSphereChaos,
        ItemType.SphereEris => IconSphereEris,
        ItemType.SphereTitan => IconSphereTitan,

        ItemType.TalismanCrusher => IconTalismanCrusher,
        ItemType.TalismanPunisher => IconTalismanPunisher,
        ItemType.TalismanHarmony => IconTalismanHarmony,
        ItemType.TalismanAegis => IconTalismanAegis,
        ItemType.TalismanArchangel => IconTalismanArchangel,
        ItemType.TalismanCobra => IconTalismanCobra,
        ItemType.TalismanPhoenix => IconTalismanPhoenix,
        ItemType.TalismanLeviathan => IconTalismanLeviathan,
        ItemType.MoneyVoucher50k => IconVoucher50k,
        ItemType.MoneyVoucher250k => IconVoucher250k,
        _ => Stone
    };

    public static (float u0, float v0, float u1, float v1) GetUVs(int tile)
    {
        int col = tile % TilesPerRow;
        int row = tile / TilesPerRow;
        float u0 = (float)(col * TileSize) / AtlasW;
        float u1 = (float)(col * TileSize + TileSize) / AtlasW;
        float v0 = (float)(row * TileSize) / AtlasH;
        float v1 = (float)(row * TileSize + TileSize) / AtlasH;
        return (u0, v0, u1, v1);
    }

    public static Rectangle GetSourceRect(ItemType type)
    {
        int tile = GetTileIndex(type, 0);
        int col = tile % TilesPerRow;
        int row = tile / TilesPerRow;
        return new Rectangle(col * TileSize, row * TileSize, TileSize, TileSize);
    }

    private static float H(int x, int y, int s)
    {
        int h = x * 374761393 + y * 668265263 + s * 144665;
        h = (h ^ (h >> 13)) * 1274126177;
        h ^= h >> 16;
        return (h & 0x7fffffff) / (float)0x7fffffff;
    }

    private static int Cl(int v) => Math.Clamp(v, 0, 255);
    private static Color Shade(Color c, float f) =>
        new Color((byte)Cl((int)(c.R * f)), (byte)Cl((int)(c.G * f)), (byte)Cl((int)(c.B * f)), c.A);

    private static void Px(Image atlas, int tile, int x, int y, Color c)
    {
        int ox = (tile % TilesPerRow) * TileSize + x;
        int oy = (tile / TilesPerRow) * TileSize + y;
        Raylib.ImageDrawPixel(ref atlas, ox, oy, c);
    }

    private static void GenGrassTop(Image a)
    {
        Color baseC = new(92, 175, 54, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Px(a, GrassTop, x, y, Shade(baseC, 0.85f + H(x, y, 1) * 0.3f));
    }

    private static void GenDirt(Image a)
    {
        Color baseC = new(134, 96, 67, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Px(a, Dirt, x, y, Shade(baseC, 0.82f + H(x, y, 3) * 0.36f));
    }

    private static void GenGrassSide(Image a)
    {
        Color grass = new(92, 175, 54, 255);
        Color dirt = new(134, 96, 67, 255);
        for (int x = 0; x < 16; x++)
        {
            int boundary = 3 + (int)(H(x, 0, 5) * 3);
            for (int y = 0; y < 16; y++)
            {
                Color c = (y < boundary) ? Shade(grass, 0.85f + H(x, y, 6) * 0.3f) : Shade(dirt, 0.82f + H(x, y, 9) * 0.36f);
                Px(a, GrassSide, x, y, c);
            }
        }
    }

    private static void GenStone(Image a)
    {
        Color baseC = new(125, 125, 128, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Px(a, Stone, x, y, Shade(baseC, 0.88f + H(x, y, 11) * 0.25f));
    }

    private static void GenCobble(Image a)
    {
        Color baseC = new(120, 120, 122, 255);
        Color mortar = new(70, 70, 72, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            bool isMortar = (x % 4 == 0) || (y % 4 == 0);
            Px(a, Cobble, x, y, isMortar ? mortar : Shade(baseC, 0.8f + H(x, y, 14) * 0.4f));
        }
    }

    private static void GenSand(Image a)
    {
        Color baseC = new(222, 210, 160, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Px(a, Sand, x, y, Shade(baseC, 0.9f + H(x, y, 15) * 0.2f));
    }

    private static void GenLogSide(Image a)
    {
        Color baseC = new(105, 80, 50, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            float f = 0.8f + H(x, 1, 17) * 0.35f;
            if (x % 5 == 0) f *= 0.8f;
            Px(a, LogSide, x, y, Shade(baseC, f));
        }
    }

    private static void GenLogTop(Image a)
    {
        Color light = new(170, 140, 90, 255);
        Color bark = new(105, 80, 50, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            float d = MathF.Sqrt((x - 7.5f) * (x - 7.5f) + (y - 7.5f) * (y - 7.5f));
            Px(a, LogTop, x, y, d > 6.5f ? bark : light);
        }
    }

    private static void GenPlanks(Image a)
    {
        Color baseC = new(180, 145, 90, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = Shade(baseC, 0.9f + H(x, y, 21) * 0.2f);
            if (y % 4 == 3) c = Shade(c, 0.72f);
            Px(a, Planks, x, y, c);
        }
    }

    private static void GenLeaves(Image a)
    {
        Color baseC = new(60, 120, 45, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Px(a, Leaves, x, y, Shade(baseC, 0.75f + H(x, y, 22) * 0.5f));
    }

    private static void GenOre(Image a, int tile, Color oreColor, int seed)
    {
        Color baseC = new(125, 125, 128, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = Shade(baseC, 0.85f + H(x, y, seed) * 0.3f);
            if (H(x / 2, y / 2, seed + 1) > 0.65f && H(x, y, seed + 2) > 0.35f)
                c = Shade(oreColor, 0.85f + H(x, y, seed + 3) * 0.3f);
            Px(a, tile, x, y, c);
        }
    }

    private static void GenNetheriteOre(Image a)
    {
        Color baseC = new(80, 60, 55, 255);
        Color scrap = new(140, 110, 80, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = (x % 3 == y % 3) ? scrap : Shade(baseC, 0.85f + H(x, y, 77) * 0.3f);
            Px(a, NetheriteOre, x, y, c);
        }
    }

    private static void GenBrick(Image a)
    {
        Color brick = new(155, 75, 65, 255);
        Color mortar = new(185, 182, 175, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            int off = (y / 4 % 2) * 4;
            bool isMortar = (y % 4 == 3) || ((x + off) % 8 == 7);
            Px(a, Brick, x, y, isMortar ? mortar : Shade(brick, 0.85f + H(x, y, 41) * 0.3f));
        }
    }

    private static void GenStoneBricks(Image a)
    {
        Color brick = new(130, 130, 132, 255);
        Color mortar = new(80, 80, 82, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            int off = (y / 8 % 2) * 8;
            bool isMortar = (y % 8 == 7) || ((x + off) % 16 == 15);
            Px(a, StoneBricks, x, y, isMortar ? mortar : Shade(brick, 0.88f + H(x, y, 42) * 0.25f));
        }
    }

    private static void GenObsidian(Image a, int tile, Color baseC)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = Shade(baseC, 0.8f + H(x, y, 43) * 0.5f);
            if (H(x, y, 44) > 0.85f) c = new Color(140, 50, 200, 255);
            Px(a, tile, x, y, c);
        }
    }

    private static void GenGlowstone(Image a)
    {
        Color baseC = new(240, 200, 110, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Px(a, Glowstone, x, y, Shade(baseC, 0.75f + H(x, y, 45) * 0.5f));
    }

    private static void GenSnow(Image a)
    {
        Color baseC = new(240, 246, 250, 255);
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
            Px(a, Snow, x, y, Shade(baseC, 0.95f + H(x, y, 46) * 0.1f));
    }

    private static void GenBedrock(Image a)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = H(x / 2, y / 2, 47) > 0.5f ? new Color(50, 50, 52, 255) : new Color(90, 90, 95, 255);
            Px(a, Bedrock, x, y, Shade(c, 0.8f + H(x, y, 48) * 0.4f));
        }
    }

    private static void GenChest(Image a, int tile, Color baseC)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = Shade(baseC, 0.88f + H(x, y, 49) * 0.25f);
            if (x == 0 || y == 0 || x == 15 || y == 15) c = Shade(c, 0.55f);
            if (y >= 6 && y <= 9 && x >= 6 && x <= 9) c = new Color(255, 215, 0, 255);
            Px(a, tile, x, y, c);
        }
    }

    private static void GenBeacon(Image a)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = (x >= 4 && x <= 11 && y >= 4 && y <= 11) ? new Color(180, 255, 255, 255) : new Color(40, 60, 70, 220);
            Px(a, Beacon, x, y, c);
        }
    }

    private static void GenPearl(Image a, int tile)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            float d = MathF.Sqrt((x - 7.5f) * (x - 7.5f) + (y - 7.5f) * (y - 7.5f));
            if (d <= 6.2f)
            {
                Color c = (d < 3.2f) ? new Color(120, 255, 230, 255) : new Color(15, 85, 75, 255);
                if (x == 5 && y == 5) c = Color.White;
                Px(a, tile, x, y, c);
            }
        }
    }

    private static void GenApple(Image a, int tile, Color col)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            float dist = MathF.Sqrt((x - 7.5f) * (x - 7.5f) + (y - 8.5f) * (y - 8.5f));
            if (dist <= 5.8f)
            {
                Color c = (dist < 3.5f) ? Shade(col, 1.25f) : col;
                if (x == 7 && y <= 3) c = new Color(100, 60, 20, 255);
                if (x == 5 && y == 6) c = Color.White;
                Px(a, tile, x, y, c);
            }
        }
    }

    private static void GenTotemIcon(Image a, int tile)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            if (x >= 3 && x <= 12 && y >= 2 && y <= 14)
            {
                Color c = Color.Gold;
                if ((x == 5 || x == 10) && (y == 4 || y == 5)) c = new Color(50, 240, 120, 255);
                if (y >= 8 && y <= 10 && (x == 3 || x == 12)) c = new Color(255, 170, 0, 255);
                Px(a, tile, x, y, c);
            }
        }
    }

    private static void GenTrapIcon(Image a, int tile, Color baseC)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = (x == 2 || x == 13 || y == 2 || y == 13 || x == y || x == 15 - y) ? Color.Gold : baseC;
            if (x >= 6 && x <= 9 && y >= 6 && y <= 9) c = new Color(220, 40, 40, 255);
            Px(a, tile, x, y, c);
        }
    }

    private static void GenPlastIcon(Image a, int tile)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            Color c = (y >= 3 && y <= 12 && x >= 2 && x <= 13) ? new Color(134, 96, 67, 255) : Color.Blank;
            if (c != Color.Blank && (x == 2 || x == 13 || y == 3 || y == 12)) c = Color.Gold;
            if (c != Color.Blank) Px(a, tile, x, y, c);
        }
    }

    private static void GenMysticSummonIcon(Image a, int tile)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            int m = Math.Abs(x - 7) + Math.Abs(y - 7);
            Color c = (m <= 3) ? Color.Gold : (m <= 6 ? new Color(180, 40, 220, 255) : Color.Blank);
            if (c != Color.Blank && m == 0) c = Color.White;
            if (c != Color.Blank) Px(a, tile, x, y, c);
        }
    }

    private static void GenSword(Image a, int tile, Color blade)
    {
        for (int i = 0; i < 11; i++)
        {
            Px(a, tile, 13 - i, 2 + i, blade);
            Px(a, tile, 12 - i, 3 + i, Shade(blade, 1.25f));
        }
        Px(a, tile, 3, 12, Color.Gold);
        Px(a, tile, 2, 13, new Color(120, 70, 20, 255));
        Px(a, tile, 1, 14, Color.Gold);
    }

    private static void GenPickaxe(Image a, int tile, Color head)
    {
        for (int i = 0; i < 8; i++)
        {
            Px(a, tile, 3 + i, 12 - i, new Color(120, 70, 20, 255));
        }
        for (int x = 7; x <= 14; x++) Px(a, tile, x, 2, head);
        for (int y = 2; y <= 9; y++) Px(a, tile, 13, y, head);
    }

    // ==========================================
    // ТОЧНЫЕ АУТЕНТИЧНЫЕ ТЕКСТУРЫ СФЕР FUNTIME
    // ==========================================
    private static void GenSphereBase(Image a, int tile, Color outer, Color inner, Color core, Color highlight)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            float dx = x - 7.5f, dy = y - 7.5f;
            float dist = MathF.Sqrt(dx * dx + dy * dy);

            if (dist <= 6.5f)
            {
                Color c = (dist < 2.5f) ? core : ((dist < 4.8f) ? inner : outer);
                float nx = dx / 6.5f, ny = dy / 6.5f;
                float nz = MathF.Sqrt(Math.Max(0f, 1.0f - nx * nx - ny * ny));
                float light = Math.Clamp(nx * -0.3f + ny * -0.5f + nz * 0.8f, 0.35f, 1.15f);
                c = Shade(c, light);

                if (dist > 5.8f) c = Shade(outer, 0.45f); // Граница сферы
                if (x >= 4 && x <= 5 && y >= 4 && y <= 5) c = highlight; // Сверкающий блик

                Px(a, tile, x, y, c);
            }
        }
    }

    private static void GenSphereAres(Image a, int tile)
    {
        GenSphereBase(a, tile, new Color(160, 10, 10, 255), new Color(240, 50, 30, 255), new Color(255, 210, 60, 255), Color.White);
        // Огненные руны Ареса
        Px(a, tile, 7, 5, new Color(255, 240, 120, 255));
        Px(a, tile, 7, 9, new Color(255, 240, 120, 255));
        Px(a, tile, 5, 7, new Color(255, 240, 120, 255));
        Px(a, tile, 9, 7, new Color(255, 240, 120, 255));
    }

    private static void GenSphereScythian(Image a, int tile)
    {
        GenSphereBase(a, tile, new Color(10, 120, 200, 255), new Color(40, 220, 240, 255), new Color(180, 255, 255, 255), Color.White);
        // Руны скорости Скифа (молнии)
        Px(a, tile, 6, 6, new Color(230, 255, 255, 255));
        Px(a, tile, 7, 7, new Color(255, 255, 255, 255));
        Px(a, tile, 8, 8, new Color(230, 255, 255, 255));
        Px(a, tile, 8, 9, new Color(100, 255, 255, 255));
    }

    private static void GenSphereAstraea(Image a, int tile)
    {
        GenSphereBase(a, tile, new Color(20, 30, 150, 255), new Color(60, 90, 240, 255), new Color(190, 210, 255, 255), Color.White);
        // Полумесяц Астреи
        for (int y = 5; y <= 10; y++) Px(a, tile, 6, y, Color.Gold);
        Px(a, tile, 7, 5, Color.Gold);
        Px(a, tile, 7, 10, Color.Gold);
    }

    private static void GenSphereChaos(Image a, int tile)
    {
        GenSphereBase(a, tile, new Color(50, 10, 80, 255), new Color(160, 20, 220, 255), new Color(255, 120, 255, 255), Color.White);
        // Глаз Сингулярности Хаоса
        Px(a, tile, 7, 7, new Color(255, 255, 255, 255));
        Px(a, tile, 8, 7, new Color(255, 255, 255, 255));
        Px(a, tile, 6, 7, new Color(200, 50, 255, 255));
        Px(a, tile, 9, 7, new Color(200, 50, 255, 255));
    }

    private static void GenSphereEris(Image a, int tile)
    {
        GenSphereBase(a, tile, new Color(180, 80, 10, 255), new Color(250, 160, 20, 255), new Color(255, 245, 120, 255), Color.White);
        // Золотые спирали бура Эрида
        Px(a, tile, 7, 6, Color.White);
        Px(a, tile, 8, 7, Color.White);
        Px(a, tile, 7, 8, Color.White);
        Px(a, tile, 6, 7, Color.White);
    }

    private static void GenSphereTitan(Image a, int tile)
    {
        GenSphereBase(a, tile, new Color(60, 70, 80, 255), new Color(130, 150, 170, 255), new Color(220, 240, 255, 255), Color.White);
        // Щитовая руна Титана
        for (int x = 6; x <= 9; x++) Px(a, tile, x, 6, Color.Gold);
        for (int y = 6; y <= 9; y++) Px(a, tile, 7, y, Color.Gold);
    }

    // ==============================================
    // ТОЧНЫЕ АУТЕНТИЧНЫЕ ТЕКСТУРЫ ТАЛИСМАНОВ FUNTIME
    // ==============================================
    private static void GenTalismanFrame(Image a, int tile, Color border, Color background)
    {
        for (int y = 0; y < 16; y++)
        for (int x = 0; x < 16; x++)
        {
            int manhattan = Math.Abs(x - 7) + Math.Abs(y - 7);
            if (manhattan <= 6)
            {
                Color c = (manhattan == 6 || manhattan == 5) ? border : background;
                if (x == 7 && y == 1) c = Color.Gold; // Верхнее ушко кулона
                Px(a, tile, x, y, c);
            }
        }
    }

    private static void GenTalismanCobra(Image a, int tile)
    {
        GenTalismanFrame(a, tile, new Color(20, 100, 40, 255), new Color(15, 40, 20, 255));
        // Змеиная голова Кобры с ядовитыми рубиновыми глазами
        Px(a, tile, 7, 5, new Color(40, 220, 90, 255));
        Px(a, tile, 8, 5, new Color(40, 220, 90, 255));
        Px(a, tile, 6, 6, new Color(255, 30, 60, 255)); // Глаз 1
        Px(a, tile, 9, 6, new Color(255, 30, 60, 255)); // Глаз 2
        Px(a, tile, 7, 7, new Color(30, 180, 70, 255));
        Px(a, tile, 8, 7, new Color(30, 180, 70, 255));
        Px(a, tile, 7, 9, new Color(255, 230, 200, 255)); // Клыки
        Px(a, tile, 8, 9, new Color(255, 230, 200, 255));
    }

    private static void GenTalismanPhoenix(Image a, int tile)
    {
        GenTalismanFrame(a, tile, new Color(255, 170, 0, 255), new Color(90, 20, 10, 255));
        // Пылающие крылья и сердце Феникса
        Px(a, tile, 7, 7, new Color(255, 255, 120, 255));
        Px(a, tile, 8, 7, new Color(255, 255, 120, 255));
        Px(a, tile, 5, 6, new Color(255, 90, 20, 255));
        Px(a, tile, 10, 6, new Color(255, 90, 20, 255));
        Px(a, tile, 4, 7, new Color(255, 50, 10, 255));
        Px(a, tile, 11, 7, new Color(255, 50, 10, 255));
    }

    private static void GenTalismanArchangel(Image a, int tile)
    {
        GenTalismanFrame(a, tile, Color.Gold, new Color(30, 35, 70, 255));
        // Белоснежные крылья и святой нимб Архангела
        Px(a, tile, 7, 4, Color.Gold);
        Px(a, tile, 8, 4, Color.Gold);
        for (int y = 6; y <= 8; y++)
        {
            Px(a, tile, 5, y, Color.White);
            Px(a, tile, 10, y, Color.White);
        }
        Px(a, tile, 7, 7, new Color(180, 240, 255, 255));
        Px(a, tile, 8, 7, new Color(180, 240, 255, 255));
    }

    private static void GenTalismanLeviathan(Image a, int tile)
    {
        GenTalismanFrame(a, tile, new Color(20, 70, 180, 255), new Color(5, 20, 60, 255));
        // Трезубец и жемчужина Левиафана
        Px(a, tile, 7, 5, new Color(0, 240, 255, 255));
        Px(a, tile, 8, 5, new Color(0, 240, 255, 255));
        Px(a, tile, 5, 5, new Color(0, 200, 240, 255));
        Px(a, tile, 10, 5, new Color(0, 200, 240, 255));
        Px(a, tile, 7, 8, Color.White);
    }

    private static void GenTalismanPunisher(Image a, int tile)
    {
        GenTalismanFrame(a, tile, new Color(40, 10, 15, 255), new Color(180, 20, 40, 255));
        // Кровавый крест Карателя
        for (int y = 4; y <= 10; y++) Px(a, tile, 7, y, new Color(255, 240, 240, 255));
        for (int x = 5; x <= 9; x++) Px(a, tile, x, 6, new Color(255, 240, 240, 255));
    }

    private static void GenTalismanCrusher(Image a, int tile)
    {
        GenTalismanFrame(a, tile, Color.Gold, new Color(80, 50, 20, 255));
        // Молот Крушителя с рубином
        for (int x = 5; x <= 9; x++) Px(a, tile, x, 5, new Color(200, 40, 40, 255));
        Px(a, tile, 7, 6, Color.Gold);
        Px(a, tile, 7, 7, Color.Gold);
        Px(a, tile, 7, 8, Color.Gold);
    }

    private static void GenTalismanHarmony(Image a, int tile)
    {
        GenTalismanFrame(a, tile, new Color(30, 180, 90, 255), new Color(15, 50, 30, 255));
        // Цветок Гармонии (Инь-Ян)
        Px(a, tile, 7, 6, Color.White);
        Px(a, tile, 8, 7, Color.White);
        Px(a, tile, 7, 8, new Color(40, 240, 120, 255));
        Px(a, tile, 6, 7, new Color(40, 240, 120, 255));
    }

    private static void GenTalismanAegis(Image a, int tile)
    {
        GenTalismanFrame(a, tile, new Color(200, 200, 210, 255), new Color(20, 60, 140, 255));
        // Бастионный щит Эгиды
        for (int x = 6; x <= 9; x++) Px(a, tile, x, 6, Color.Gold);
        Px(a, tile, 7, 7, Color.Gold);
        Px(a, tile, 8, 7, Color.Gold);
        Px(a, tile, 7, 8, Color.Gold);
    }

    private static void GenVoucherIcon(Image a, int tile, Color ribbon)
    {
        for (int y = 4; y <= 11; y++)
        for (int x = 2; x <= 13; x++)
        {
            Color c = new Color(245, 240, 220, 255);
            if (x == 2 || x == 13 || y == 4 || y == 11) c = Color.Gold;
            if (x == 7 || x == 8) c = ribbon;
            Px(a, tile, x, y, c);
        }
    }
}