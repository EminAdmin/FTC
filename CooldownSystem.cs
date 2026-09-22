using System;
using System.Collections.Generic;

namespace FunTimeCobra;

public enum CooldownType
{
    EnderPearl,
    GoldenApple,
    EnchantedGoldenApple,
    TrapBox,
    Plast,
    DisorientBomb,
    Harpoon,
    ChorusFruit,
    MysticSummon
}

public class CooldownSystem
{
    private readonly Dictionary<CooldownType, float> _timers = new();
    private readonly Dictionary<CooldownType, float> _maxTimers = new();

    public const float CD_EnderPearl = 14.0f;
    public const float CD_GoldenApple = 4.0f;
    public const float CD_EnchantedGoldenApple = 45.0f;
    public const float CD_TrapBox = 12.0f;
    public const float CD_Plast = 8.0f;
    public const float CD_DisorientBomb = 20.0f;
    public const float CD_Harpoon = 15.0f;
    public const float CD_Chorus = 5.0f;
    public const float CD_MysticSummon = 180.0f;

    public void Update(float dt)
    {
        var keys = new List<CooldownType>(_timers.Keys);
        foreach (var key in keys)
        {
            _timers[key] -= dt;
            if (_timers[key] <= 0f)
            {
                _timers.Remove(key);
                _maxTimers.Remove(key);
            }
        }
    }

    public bool IsOnCooldown(CooldownType type) => _timers.ContainsKey(type) && _timers[type] > 0f;

    public float GetRemaining(CooldownType type) => _timers.TryGetValue(type, out float val) ? val : 0f;

    public float GetProgress(CooldownType type)
    {
        if (!_timers.TryGetValue(type, out float cur) || !_maxTimers.TryGetValue(type, out float max) || max <= 0f)
            return 0f;
        return Math.Clamp(cur / max, 0f, 1f);
    }

    public void Trigger(CooldownType type, float duration)
    {
        _timers[type] = duration;
        _maxTimers[type] = duration;
    }

    public static CooldownType? GetCooldownType(ItemType item) => item switch
    {
        ItemType.EnderPearl => CooldownType.EnderPearl,
        ItemType.GoldenApple => CooldownType.GoldenApple,
        ItemType.EnchantedGoldenApple => CooldownType.EnchantedGoldenApple,
        ItemType.TrapBox or ItemType.TrapBoxCobra or ItemType.TrapBoxNetherite or ItemType.TrapBoxCryo => CooldownType.TrapBox,
        ItemType.Plast => CooldownType.Plast,
        ItemType.DisorientBomb => CooldownType.DisorientBomb,
        ItemType.Harpoon => CooldownType.Harpoon,
        ItemType.ChorusFruit => CooldownType.ChorusFruit,
        ItemType.MysticSummon => CooldownType.MysticSummon,
        _ => null
    };
}