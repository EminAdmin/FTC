using System;
using System.Collections.Generic;

namespace FunTimeCobra;

public class BPTask
{
    public string Id;
    public string Title;
    public int RequiredProgress;
    public int CurrentProgress;
    public int ExpReward;
    public bool IsCompleted => CurrentProgress >= RequiredProgress;

    public BPTask(string id, string title, int req, int exp)
    {
        Id = id;
        Title = title;
        RequiredProgress = req;
        CurrentProgress = 0;
        ExpReward = exp;
    }
}

public class BPRewardLevel
{
    public int Level;
    public ItemStack FreeReward;
    public ItemStack PremiumReward;
    public bool FreeClaimed;
    public bool PremiumClaimed;

    public BPRewardLevel(int lvl, ItemStack free, ItemStack prem)
    {
        Level = lvl;
        FreeReward = free;
        PremiumReward = prem;
        FreeClaimed = false;
        PremiumClaimed = false;
    }
}

public class BattlePass
{
    public int CurrentLevel = 1;
    public int CurrentExp = 0;
    public const int ExpPerLevel = 250;
    public const int MaxLevel = 30;
    public bool HasPremium = true; // Для рангов Sponsor/Cobra активно сразу

    public readonly List<BPTask> Tasks = new();
    public readonly List<BPRewardLevel> Rewards = new();

    public BattlePass()
    {
        InitTasks();
        InitRewards();
    }

    private void InitTasks()
    {
        Tasks.Add(new BPTask("break_obsidian", "Сломать 16 блоков обсидиана", 16, 120));
        Tasks.Add(new BPTask("throw_pearls", "Телепортироваться эндер-перлом 10 раз", 10, 80));
        Tasks.Add(new BPTask("eat_gapples", "Съесть 5 Золотых яблок", 5, 100));
        Tasks.Add(new BPTask("open_mystic", "Открыть Мистический сундук", 1, 300));
        Tasks.Add(new BPTask("place_traps", "Установить 3 Трапки", 3, 90));
        Tasks.Add(new BPTask("mine_diamonds", "Добыть 8 алмазной руды", 8, 150));
    }

    private void InitRewards()
    {
        for (int i = 1; i <= MaxLevel; i++)
        {
            ItemStack free = (i % 5 == 0) 
                ? new ItemStack(ItemType.GoldenApple, 8) 
                : new ItemStack(ItemType.Obsidian, 16);

            ItemStack prem = i switch
            {
                5 => new ItemStack(ItemType.SphereAres, 1),
                10 => new ItemStack(ItemType.TalismanHarmony, 1),
                15 => new ItemStack(ItemType.TrapBoxCobra, 4),
                20 => new ItemStack(ItemType.SphereScythian, 1),
                25 => new ItemStack(ItemType.TalismanArchangel, 1),
                30 => new ItemStack(ItemType.TalismanCobra, 1),
                _ => new ItemStack(ItemType.EnchantedGoldenApple, 2)
            };

            Rewards.Add(new BPRewardLevel(i, free, prem));
        }
    }

    public void AddProgress(string taskId, int amount, Player player)
    {
        foreach (var task in Tasks)
        {
            if (task.Id == taskId && !task.IsCompleted)
            {
                task.CurrentProgress = Math.Min(task.RequiredProgress, task.CurrentProgress + amount);
                if (task.IsCompleted)
                {
                    AddExp(task.ExpReward, player);
                    Program.AddChatMessage($"[Боевой Пропуск] Задание выполнено: {task.Title} (+{task.ExpReward} XP)!", Raylib_cs.Color.Gold);
                }
            }
        }
    }

    public void AddExp(int amount, Player player)
    {
        CurrentExp += amount;
        while (CurrentExp >= ExpPerLevel && CurrentLevel < MaxLevel)
        {
            CurrentExp -= ExpPerLevel;
            CurrentLevel++;
            Program.AddChatMessage($"[Боевой Пропуск] » Поздравляем! Достигнут {CurrentLevel} УРОВЕНЬ БП! (/bp)", Raylib_cs.Color.Lime);
            SoundManager.PlayTotem();
        }
    }

    public bool ClaimReward(int level, bool premium, Player player)
    {
        if (level > CurrentLevel) return false;
        var r = Rewards.Find(x => x.Level == level);
        if (r == null) return false;

        if (premium)
        {
            if (!HasPremium || r.PremiumClaimed) return false;
            if (player.TryAddItem(r.PremiumReward.Type, r.PremiumReward.Count))
            {
                r.PremiumClaimed = true;
                SoundManager.PlayPlace();
                return true;
            }
        }
        else
        {
            if (r.FreeClaimed) return false;
            if (player.TryAddItem(r.FreeReward.Type, r.FreeReward.Count))
            {
                r.FreeClaimed = true;
                SoundManager.PlayPlace();
                return true;
            }
        }
        return false;
    }
}