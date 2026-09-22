using System;
using System.Numerics;

namespace FunTimeCobra;

public class Chest
{
    public Vector3 Position;
    public ItemStack[] Inventory = new ItemStack[27];

    public Chest(Vector3 pos)
    {
        Position = pos;
    }

    public static Chest GenerateMysticLoot(Vector3 pos, ItemRarity tier)
    {
        Chest c = new(pos);
        Random r = new();

        c.Inventory[0] = new ItemStack(ItemType.GoldenApple, r.Next(16, 33));
        c.Inventory[1] = new ItemStack(ItemType.EnderPearl, r.Next(16, 33));
        c.Inventory[2] = new ItemStack(ItemType.Obsidian, 64);
        c.Inventory[3] = new ItemStack(ItemType.EnchantedGoldenApple, r.Next(2, 6));

        switch (tier)
        {
            case ItemRarity.Mythic:
                c.Inventory[4] = new ItemStack(ItemType.SphereChaos, 1);
                c.Inventory[5] = new ItemStack(ItemType.TalismanCobra, 1);
                c.Inventory[6] = new ItemStack(ItemType.TalismanLeviathan, 1);
                c.Inventory[7] = new ItemStack(ItemType.TrapBoxCobra, 6);
                c.Inventory[8] = new ItemStack(ItemType.NetheriteIngot, r.Next(4, 9));
                c.Inventory[9] = new ItemStack(ItemType.MoneyVoucher250k, 1);
                break;

            case ItemRarity.Legendary:
                c.Inventory[4] = new ItemStack(ItemType.SphereScythian, 1);
                c.Inventory[5] = new ItemStack(ItemType.TalismanArchangel, 1);
                c.Inventory[6] = new ItemStack(ItemType.TalismanPhoenix, 2);
                c.Inventory[7] = new ItemStack(ItemType.TrapBox, 8);
                c.Inventory[8] = new ItemStack(ItemType.Diamond, 32);
                c.Inventory[9] = new ItemStack(ItemType.MoneyVoucher50k, 2);
                break;

            case ItemRarity.Epic:
                c.Inventory[4] = new ItemStack(ItemType.SphereAres, 1);
                c.Inventory[5] = new ItemStack(ItemType.SphereTitan, 1);
                c.Inventory[6] = new ItemStack(ItemType.TalismanCrusher, 1);
                c.Inventory[7] = new ItemStack(ItemType.Plast, 8);
                c.Inventory[8] = new ItemStack(ItemType.TotemOfUndying, 2);
                break;

            default:
                c.Inventory[4] = new ItemStack(ItemType.SphereEris, 1);
                c.Inventory[5] = new ItemStack(ItemType.TalismanHarmony, 1);
                c.Inventory[6] = new ItemStack(ItemType.DiamondOre, 32);
                c.Inventory[7] = new ItemStack(ItemType.IronIngot, 48);
                break;
        }

        return c;
    }
}