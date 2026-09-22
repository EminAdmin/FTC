using System;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public static class RtpManager
{
    private static readonly Random Rand = new();

    public static void ExecuteRTP(Player player, World world)
    {
        // 1. Поиск случайной точки на расстоянии от 150 до 800 блоков от спавна
        int rangeMin = 150;
        int rangeMax = 800;
        
        int signX = Rand.Next(2) == 0 ? 1 : -1;
        int signZ = Rand.Next(2) == 0 ? 1 : -1;
        
        int targetX = signX * Rand.Next(rangeMin, rangeMax);
        int targetZ = signZ * Rand.Next(rangeMin, rangeMax);

        // Форсируем подгрузку/генерацию чанков в целевой области
        world.UpdateChunkStreaming(new Vector3(targetX, 32, targetZ), 32f);

        int yMax = world.GetSurfaceHeight(targetX, targetZ);
        if (yMax < 5) yMax = 35;

        // 2. Телепортация ровно на 15 блоков выше земли (без риска застрять)
        Vector3 targetPos = new(targetX + 0.5f, yMax + 15.0f, targetZ + 0.5f);

        player.Position = targetPos;
        player.Velocity = Vector3.Zero;

        // 3. Наложение эффекта Замедленного падения (Slow Falling) на 10 секунд
        player.AddStatusEffect(StatusEffectType.SlowFalling, 10.0f, 0);

        // 4. Сетевые партиклы и звук телепорта
        ParticleSystem.SpawnTotemEffect(targetPos);
        for (int i = 0; i < 30; i++)
        {
            ParticleSystem.SpawnAuraParticle(targetPos, new Color(240, 240, 255, 255), 1.5f);
        }

        SoundManager.PlayTeleport();
        Program.AddChatMessage($"[FunTime] » Вы успешно телепортированы на X: {targetX} Z: {targetZ}!", Color.Lime);
        Program.AddChatMessage("[FunTime] » На вас наложен эффект Замедленного падения (10с).", Color.SkyBlue);
    }
}