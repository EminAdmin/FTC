using System;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace FunTimeCobra;

public static class SoundManager
{
    private static Sound sndBreak;
    private static Sound sndPlace;
    private static Sound sndJump;
    private static Sound sndTeleport;
    private static Sound sndTotem;
    private static Sound sndMystic;
    private static bool isInitialized = false;

    public static void Initialize()
    {
        try
        {
            Raylib.InitAudioDevice();
            if (Raylib.IsAudioDeviceReady())
            {
                sndBreak = GenerateNoiseSound(0.08f);
                sndPlace = GenerateToneSound(0.06f, 180f);
                sndJump = GenerateToneSound(0.12f, 320f, true);
                sndTeleport = GenerateMagicSound(0.25f);
                sndTotem = GenerateTotemSound(0.6f);
                sndMystic = GenerateMysticFanfare(0.8f);
                isInitialized = true;
            }
        }
        catch { }
    }

    public static void PlayBreak() { if (isInitialized) Raylib.PlaySound(sndBreak); }
    public static void PlayPlace() { if (isInitialized) Raylib.PlaySound(sndPlace); }
    public static void PlayJump() { if (isInitialized) Raylib.PlaySound(sndJump); }
    public static void PlayTeleport() { if (isInitialized) Raylib.PlaySound(sndTeleport); }
    public static void PlayTotem() { if (isInitialized) Raylib.PlaySound(sndTotem); }
    public static void PlayMystic() { if (isInitialized) Raylib.PlaySound(sndMystic); }

    private static Sound GenerateToneSound(float duration, float freq, bool slide = false)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float f = slide ? freq * (1f + t * 3.5f) : freq;
            float envelope = 1f - (float)i / sampleCount;
            samples[i] = (short)(MathF.Sin(2f * MathF.PI * f * t) * envelope * 12000);
        }
        return CreateSoundFromSamples(samples, sampleRate);
    }

    private static Sound GenerateNoiseSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];
        Random rnd = new();

        for (int i = 0; i < sampleCount; i++)
        {
            float envelope = 1f - (float)i / sampleCount;
            float noise = ((float)rnd.NextDouble() * 2f - 1f);
            samples[i] = (short)(noise * envelope * 16000);
        }
        return CreateSoundFromSamples(samples, sampleRate);
    }

    private static Sound GenerateMagicSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float f = 400f + MathF.Sin(t * 50f) * 250f;
            float envelope = 1f - (float)i / sampleCount;
            samples[i] = (short)(MathF.Sin(2f * MathF.PI * f * t) * envelope * 14000);
        }
        return CreateSoundFromSamples(samples, sampleRate);
    }

    private static Sound GenerateTotemSound(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float f = 220f + t * 800f;
            float envelope = MathF.Sin(t / duration * MathF.PI);
            samples[i] = (short)((MathF.Sin(2f * MathF.PI * f * t) + MathF.Sin(2f * MathF.PI * (f * 1.5f) * t)) * envelope * 15000);
        }
        return CreateSoundFromSamples(samples, sampleRate);
    }

    private static Sound GenerateMysticFanfare(float duration)
    {
        int sampleRate = 44100;
        int sampleCount = (int)(sampleRate * duration);
        short[] samples = new short[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float f = (t < 0.25f) ? 440f : ((t < 0.5f) ? 554f : 659f);
            float envelope = 1f - (t / duration);
            samples[i] = (short)(MathF.Sin(2f * MathF.PI * f * t) * envelope * 14000);
        }
        return CreateSoundFromSamples(samples, sampleRate);
    }

    private static Sound CreateSoundFromSamples(short[] samples, int sampleRate)
    {
        Wave wave = new()
        {
            FrameCount = (uint)samples.Length,
            SampleRate = (uint)sampleRate,
            SampleSize = 16,
            Channels = 1
        };

        unsafe
        {
            wave.Data = (void*)Raylib.MemAlloc((uint)(samples.Length * sizeof(short)));
            Marshal.Copy(samples, 0, (IntPtr)wave.Data, samples.Length);
        }

        Sound snd = Raylib.LoadSoundFromWave(wave);
        Raylib.UnloadWave(wave);
        return snd;
    }
}