using System;

namespace FunTimeCobra;

public enum ItemRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary,
    Mythic
}

public enum ItemType : ushort
{
    None = 0,

    // Блоки
    Grass = 1,
    Dirt = 2,
    Stone = 3,
    Cobblestone = 4,
    Sand = 5,
    Log = 6,
    Planks = 7,
    Leaves = 8,
    CoalOre = 9,
    IronOre = 10,
    DiamondOre = 11,
    GoldOre = 12,
    RedstoneOre = 13,
    EmeraldOre = 14,
    NetheriteOre = 15,
    Brick = 16,
    StoneBricks = 17,
    Obsidian = 18,
    CryingObsidian = 19,
    Glowstone = 20,
    Snow = 21,
    Bedrock = 22,
    Chest = 23,
    MysticChest = 24,
    EnderChest = 25,
    CraftingTable = 26,
    Beacon = 27,
    TNT = 28,

    // Ресурсы
    Stick = 40,
    Coal = 41,
    IronIngot = 42,
    GoldIngot = 43,
    Diamond = 44,
    Emerald = 45,
    NetheriteScrap = 46,
    NetheriteIngot = 47,
    NetherStar = 48,
    ChorusFruit = 49,

    // Оружие и броня
    DiamondSword = 60,
    NetheriteSword = 61,
    DiamondPickaxe = 62,
    NetheritePickaxe = 63,
    DiamondChestplate = 70,
    NetheriteChestplate = 71,

    // Еда и Зелья
    GoldenApple = 80,
    EnchantedGoldenApple = 81,
    TotemOfUndying = 82,
    EnderPearl = 83,

    // Спецпредметы FunTime
    TrapBox = 100,
    TrapBoxCobra = 101,
    TrapBoxNetherite = 102,
    TrapBoxCryo = 103,
    Plast = 104,
    MysticSummon = 105,
    DisorientBomb = 106,
    Harpoon = 107,
    MoneyVoucher50k = 108,
    MoneyVoucher250k = 109,

    // Сферы FunTime
    SphereAres = 120,
    SphereScythian = 121,
    SphereAstraea = 122,
    SphereChaos = 123,
    SphereEris = 124,
    SphereTitan = 125,

    // Талисманы FunTime
    TalismanCrusher = 130,
    TalismanPunisher = 131,
    TalismanHarmony = 132,
    TalismanAegis = 133,
    TalismanArchangel = 134,
    TalismanCobra = 135,
    TalismanPhoenix = 136,
    TalismanLeviathan = 137
}

public struct ItemStack
{
    public ItemType Type;
    public int Count;

    public ItemStack(ItemType type, int count = 1)
    {
        Type = type;
        Count = count;
    }

    public bool IsEmpty => Type == ItemType.None || Count <= 0;
    public bool IsPlaceable => Type >= ItemType.Grass && Type <= ItemType.TNT;
    public bool IsOffHandBuffItem => Type >= ItemType.SphereAres && Type <= ItemType.TalismanLeviathan;
    public bool IsFood => Type == ItemType.GoldenApple || Type == ItemType.EnchantedGoldenApple || Type == ItemType.ChorusFruit;
}

public static class BlockInfo
{
    public static float Hardness(ItemType t) => ItemData.GetHardness(t);
    public static bool Unbreakable(ItemType t) => ItemData.IsUnbreakable(t);
    public static ItemType Drop(ItemType t) => ItemData.GetDrop(t);
    public static string Name(ItemType t) => ItemData.GetName(t);
}

public static class ItemData
{
    public static float GetHardness(ItemType t) => t switch
    {
        ItemType.Leaves => 0.2f,
        ItemType.Sand or ItemType.Dirt or ItemType.Snow => 0.45f,
        ItemType.Grass => 0.5f,
        ItemType.Log or ItemType.Planks or ItemType.CraftingTable or ItemType.Chest or ItemType.MysticChest => 1.1f,
        ItemType.Stone or ItemType.Cobblestone or ItemType.StoneBricks => 1.5f,
        ItemType.CoalOre or ItemType.IronOre or ItemType.GoldOre or ItemType.RedstoneOre => 2.2f,
        ItemType.DiamondOre or ItemType.EmeraldOre or ItemType.Beacon => 3.0f,
        ItemType.Obsidian or ItemType.CryingObsidian or ItemType.EnderChest => 6.5f,
        ItemType.NetheriteOre => 5.0f,
        ItemType.Bedrock => float.PositiveInfinity,
        _ => 1.0f
    };

    public static bool IsUnbreakable(ItemType t) => t == ItemType.Bedrock;

    public static ItemType GetDrop(ItemType t) => t switch
    {
        ItemType.Grass => ItemType.Dirt,
        ItemType.Stone => ItemType.Cobblestone,
        ItemType.CoalOre => ItemType.Coal,
        ItemType.DiamondOre => ItemType.Diamond,
        ItemType.EmeraldOre => ItemType.Emerald,
        ItemType.Leaves => ItemType.Stick,
        _ => t
    };

    public static string GetName(ItemType t) => t switch
    {
        ItemType.Grass => "Блок травы",
        ItemType.Dirt => "Земля",
        ItemType.Stone => "Камень",
        ItemType.Cobblestone => "Булыжник",
        ItemType.Sand => "Песок",
        ItemType.Log => "Дубовое бревно",
        ItemType.Planks => "Дубовые доски",
        ItemType.Leaves => "Листва",
        ItemType.CoalOre => "Угольная руда",
        ItemType.IronOre => "Железная руда",
        ItemType.DiamondOre => "Алмазная руда",
        ItemType.GoldOre => "Золотая руда",
        ItemType.RedstoneOre => "Редстоун руда",
        ItemType.EmeraldOre => "Изумрудная руда",
        ItemType.NetheriteOre => "Древние обломки",
        ItemType.Brick => "Кирпичи",
        ItemType.StoneBricks => "Каменные кирпичи",
        ItemType.Obsidian => "Обсидиан",
        ItemType.CryingObsidian => "Плачущий обсидиан",
        ItemType.Glowstone => "Светокамень",
        ItemType.Snow => "Снег",
        ItemType.Bedrock => "Бедрок",
        ItemType.Chest => "Сундук",
        ItemType.MysticChest => "\u00A76Мистический Сундук",
        ItemType.EnderChest => "Эндер-Сундук",
        ItemType.CraftingTable => "Верстак",
        ItemType.Beacon => "Маяк",
        ItemType.TNT => "Динамит",

        ItemType.Stick => "Палка",
        ItemType.Coal => "Уголь",
        ItemType.IronIngot => "Железный слиток",
        ItemType.GoldIngot => "Золотой слиток",
        ItemType.Diamond => "Алмаз",
        ItemType.Emerald => "Изумруд",
        ItemType.NetheriteScrap => "Незеритовый скрап",
        ItemType.NetheriteIngot => "Незеритовый слиток",
        ItemType.NetherStar => "Звезда Незера",
        ItemType.ChorusFruit => "Хорус",

        ItemType.DiamondSword => "Алмазный меч [Острота V]",
        ItemType.NetheriteSword => "Незеритовый меч [Острота VI]",
        ItemType.DiamondPickaxe => "Алмазная кирка [Эффект V]",
        ItemType.NetheritePickaxe => "Незеритовая кирка [Эффект VI]",
        ItemType.DiamondChestplate => "Алмазный нагрудник",
        ItemType.NetheriteChestplate => "Незеритовый нагрудник",

        ItemType.GoldenApple => "Золотое яблоко",
        ItemType.EnchantedGoldenApple => "\u00A7dЗачарованное яблоко (Чарка)",
        ItemType.TotemOfUndying => "Тотем бессмертия",
        ItemType.EnderPearl => "Эндер-жемчуг",

        ItemType.TrapBox => "Трапка [3x3 Обсидиан]",
        ItemType.TrapBoxCobra => "\u00A76Трапка Кобры",
        ItemType.TrapBoxNetherite => "\u00A78Незеритовая Трапка",
        ItemType.TrapBoxCryo => "\u00A7bКрио-Трапка",
        ItemType.Plast => "\u00A7eПласт земли",
        ItemType.MysticSummon => "\u00A7dПризывной Мистик",
        ItemType.DisorientBomb => "\u00A7cДезориентатор",
        ItemType.Harpoon => "\u00A7aГарпун",
        ItemType.MoneyVoucher50k => "\u00A7aЧек на 50,000$",
        ItemType.MoneyVoucher250k => "\u00A76Чек на 250,000$",

        ItemType.SphereAres => "\u00A7cСфера Ареса [Сила III]",
        ItemType.SphereScythian => "\u00A7bСфера Скифа [Скорость IV]",
        ItemType.SphereAstraea => "\u00A79Сфера Астреи [Прыгучесть]",
        ItemType.SphereChaos => "\u00A7dСфера Хаоса [Баланс]",
        ItemType.SphereEris => "\u00A76Сфера Эрида [Бур V]",
        ItemType.SphereTitan => "\u00A77Сфера Титана [Защита IV]",

        ItemType.TalismanCrusher => "\u00A76Талисман Крушителя",
        ItemType.TalismanPunisher => "\u00A7cТалисман Карателя",
        ItemType.TalismanHarmony => "\u00A7aТалисман Гармонии",
        ItemType.TalismanAegis => "\u00A79Талисман Эгиды",
        ItemType.TalismanArchangel => "\u00A7fТалисман Архангела",
        ItemType.TalismanCobra => "\u00A72Талисман Кобры",
        ItemType.TalismanPhoenix => "\u00A76Талисман Феникса",
        ItemType.TalismanLeviathan => "\u00A71Талисман Левиафана",
        _ => ""
    };
}