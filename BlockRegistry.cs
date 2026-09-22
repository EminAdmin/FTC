using System.Collections.Generic;

namespace FunTimeCobra;

public enum BlockRenderType
{
    Opaque,        // Обычные непрозрачные блоки
    Cutout,        // Листва, трава, маскированная прозрачность
    Translucent,   // Стекло, окрашенное стекло, лед
    Liquid         // Вода, лава
}

public class BlockProperties
{
    public string Name;
    public float Hardness;
    public bool IsSolid;
    public bool IsTransparent;
    public BlockRenderType RenderType;
    public ItemType DropItem;

    public BlockProperties(string name, float hardness, bool solid, bool transparent, BlockRenderType renderType, ItemType drop)
    {
        Name = name;
        Hardness = hardness;
        IsSolid = solid;
        IsTransparent = transparent;
        RenderType = renderType;
        DropItem = drop;
    }
}

public static class BlockRegistry
{
    private static readonly Dictionary<ItemType, BlockProperties> Registry = new();

    static BlockRegistry()
    {
        // 1.16.5 Базовые блоки
        Register(ItemType.Grass, "Блок травы", 0.6f, true, false, BlockRenderType.Opaque, ItemType.Dirt);
        Register(ItemType.Dirt, "Земля", 0.5f, true, false, BlockRenderType.Opaque, ItemType.Dirt);
        Register(ItemType.Stone, "Камень", 1.5f, true, false, BlockRenderType.Opaque, ItemType.Cobblestone);
        Register(ItemType.Cobblestone, "Булыжник", 2.0f, true, false, BlockRenderType.Opaque, ItemType.Cobblestone);
        Register(ItemType.Sand, "Песок", 0.5f, true, false, BlockRenderType.Opaque, ItemType.Sand);
        Register(ItemType.Log, "Дубовое бревно", 2.0f, true, false, BlockRenderType.Opaque, ItemType.Log);
        Register(ItemType.Planks, "Дубовые доски", 2.0f, true, false, BlockRenderType.Opaque, ItemType.Planks);
        Register(ItemType.Leaves, "Листва", 0.2f, true, true, BlockRenderType.Cutout, ItemType.Stick);
        Register(ItemType.Bedrock, "Бедрок", float.PositiveInfinity, true, false, BlockRenderType.Opaque, ItemType.None);
        Register(ItemType.Glowstone, "Светокамень", 0.3f, true, false, BlockRenderType.Opaque, ItemType.Glowstone);

        // 1.16.5 Адские блоки и Чернокамень (Nether Update)
        Register(ItemType.Obsidian, "Обсидиан", 50.0f, true, false, BlockRenderType.Opaque, ItemType.Obsidian);
        Register(ItemType.CryingObsidian, "Плачущий обсидиан", 50.0f, true, false, BlockRenderType.Opaque, ItemType.CryingObsidian);
        Register(ItemType.NetheriteOre, "Древние обломки", 30.0f, true, false, BlockRenderType.Opaque, ItemType.NetheriteScrap);

        // Руды
        Register(ItemType.CoalOre, "Угольная руда", 3.0f, true, false, BlockRenderType.Opaque, ItemType.Coal);
        Register(ItemType.IronOre, "Железная руда", 3.0f, true, false, BlockRenderType.Opaque, ItemType.IronOre);
        Register(ItemType.GoldOre, "Золотая руда", 3.0f, true, false, BlockRenderType.Opaque, ItemType.GoldOre);
        Register(ItemType.DiamondOre, "Алмазная руда", 3.0f, true, false, BlockRenderType.Opaque, ItemType.Diamond);
        Register(ItemType.EmeraldOre, "Изумрудная руда", 3.0f, true, false, BlockRenderType.Opaque, ItemType.Emerald);
        Register(ItemType.RedstoneOre, "Редстоун руда", 3.0f, true, false, BlockRenderType.Opaque, ItemType.RedstoneOre);

        // Интерактивные и специальные
        Register(ItemType.CraftingTable, "Верстак", 2.5f, true, false, BlockRenderType.Opaque, ItemType.CraftingTable);
        Register(ItemType.Chest, "Сундук", 2.5f, true, true, BlockRenderType.Opaque, ItemType.Chest);
        Register(ItemType.MysticChest, "§6Мистический Сундук", 5.0f, true, true, BlockRenderType.Opaque, ItemType.Chest);
        Register(ItemType.Beacon, "Маяк", 3.0f, true, true, BlockRenderType.Translucent, ItemType.Beacon);
        Register(ItemType.StoneBricks, "Каменные кирпичи", 1.5f, true, false, BlockRenderType.Opaque, ItemType.StoneBricks);
        Register(ItemType.Snow, "Снег", 0.2f, true, false, BlockRenderType.Opaque, ItemType.Snow);
    }

    public static void Register(ItemType type, string name, float hardness, bool solid, bool transparent, BlockRenderType renderType, ItemType drop)
    {
        Registry[type] = new BlockProperties(name, hardness, solid, transparent, renderType, drop);
    }

    public static bool IsSolid(ItemType t) => Registry.TryGetValue(t, out var p) && p.IsSolid;
    public static bool IsTransparent(ItemType t) => !Registry.TryGetValue(t, out var p) || p.IsTransparent;
    public static BlockRenderType GetRenderType(ItemType t) => Registry.TryGetValue(t, out var p) ? p.RenderType : BlockRenderType.Opaque;
    public static float GetHardness(ItemType t) => Registry.TryGetValue(t, out var p) ? p.Hardness : 1.0f;
    public static string GetName(ItemType t) => Registry.TryGetValue(t, out var p) ? p.Name : t.ToString();
}