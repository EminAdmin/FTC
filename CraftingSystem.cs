using System;
using System.Collections.Generic;

namespace FunTimeCobra;

public class CraftRecipe
{
    public ItemType[,] Pattern = new ItemType[3, 3];
    public ItemStack Result;
    public string Name;

    public CraftRecipe(string name, ItemStack result, ItemType[,] pattern)
    {
        Name = name;
        Result = result;
        Pattern = pattern;
    }

    public bool Matches(ItemStack[] grid)
    {
        for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
        {
            ItemType required = Pattern[r, c];
            ItemType actual = grid[r * 3 + c].Type;
            if (required != actual) return false;
        }
        return true;
    }
}

public static class CraftingManager
{
    public static readonly List<CraftRecipe> Recipes = new();

    static CraftingManager()
    {
        RegisterFunTimeRecipes();
    }

    private static void RegisterFunTimeRecipes()
    {
        // 1. Крафт Трапки (Обсидиан по кругу, внутри Эндер-жемчуг)
        Recipes.Add(new CraftRecipe("Трапка FunTime", new ItemStack(ItemType.TrapBox, 2), new ItemType[3, 3] {
            { ItemType.Obsidian, ItemType.Obsidian, ItemType.Obsidian },
            { ItemType.Obsidian, ItemType.EnderPearl, ItemType.Obsidian },
            { ItemType.Obsidian, ItemType.Obsidian, ItemType.Obsidian }
        }));

        // 2. Крафт Трапки Кобры (Плачущий обсидиан + Изумруд в центре)
        Recipes.Add(new CraftRecipe("Трапка Кобры", new ItemStack(ItemType.TrapBoxCobra, 1), new ItemType[3, 3] {
            { ItemType.Obsidian, ItemType.Emerald, ItemType.Obsidian },
            { ItemType.Emerald, ItemType.TrapBox, ItemType.Emerald },
            { ItemType.Obsidian, ItemType.Emerald, ItemType.Obsidian }
        }));

        // 3. Зачарованное золотое яблоко (Яблоко + 8 золотых блоков/слитков)
        Recipes.Add(new CraftRecipe("Чарка (Зачарованное яблоко)", new ItemStack(ItemType.EnchantedGoldenApple, 1), new ItemType[3, 3] {
            { ItemType.GoldIngot, ItemType.GoldIngot, ItemType.GoldIngot },
            { ItemType.GoldIngot, ItemType.GoldenApple, ItemType.GoldIngot },
            { ItemType.GoldIngot, ItemType.GoldIngot, ItemType.GoldIngot }
        }));

        // 4. Сфера Ареса (Сила III)
        Recipes.Add(new CraftRecipe("Сфера Ареса", new ItemStack(ItemType.SphereAres, 1), new ItemType[3, 3] {
            { ItemType.RedstoneOre, ItemType.Diamond, ItemType.RedstoneOre },
            { ItemType.Diamond, ItemType.NetherStar, ItemType.Diamond },
            { ItemType.RedstoneOre, ItemType.Diamond, ItemType.RedstoneOre }
        }));

        // 5. Сфера Скифа (Скорость IV)
        Recipes.Add(new CraftRecipe("Сфера Скифа", new ItemStack(ItemType.SphereScythian, 1), new ItemType[3, 3] {
            { ItemType.Glowstone, ItemType.Diamond, ItemType.Glowstone },
            { ItemType.Diamond, ItemType.ChorusFruit, ItemType.Diamond },
            { ItemType.Glowstone, ItemType.Diamond, ItemType.Glowstone }
        }));

        // 6. Талисман Феникса (Тотем + Сфера Ареса + Золото)
        Recipes.Add(new CraftRecipe("Талисман Феникса", new ItemStack(ItemType.TalismanPhoenix, 1), new ItemType[3, 3] {
            { ItemType.GoldIngot, ItemType.TotemOfUndying, ItemType.GoldIngot },
            { ItemType.TotemOfUndying, ItemType.NetherStar, ItemType.TotemOfUndying },
            { ItemType.GoldIngot, ItemType.TotemOfUndying, ItemType.GoldIngot }
        }));

        // 7. Пласт земли / стены
        Recipes.Add(new CraftRecipe("Пласт земли", new ItemStack(ItemType.Plast, 4), new ItemType[3, 3] {
            { ItemType.Dirt, ItemType.Dirt, ItemType.Dirt },
            { ItemType.Dirt, ItemType.EnderPearl, ItemType.Dirt },
            { ItemType.Dirt, ItemType.Dirt, ItemType.Dirt }
        }));

        // 8. Алмазный меч
        Recipes.Add(new CraftRecipe("Алмазный меч", new ItemStack(ItemType.DiamondSword, 1), new ItemType[3, 3] {
            { ItemType.None, ItemType.Diamond, ItemType.None },
            { ItemType.None, ItemType.Diamond, ItemType.None },
            { ItemType.None, ItemType.Stick, ItemType.None }
        }));
    }

    public static ItemStack MatchRecipe(ItemStack[] matrix)
    {
        foreach (var r in Recipes)
        {
            if (r.Matches(matrix))
                return r.Result;
        }
        return new ItemStack();
    }
}