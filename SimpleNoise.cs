using System;

namespace FunTimeCobra;

public static class SimpleNoise
{
    private static readonly int[] Permutation = new int[512];

    static SimpleNoise()
    {
        Random rand = new(20261337);
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;
        for (int i = 255; i > 0; i--)
        {
            int swapIndex = rand.Next(i + 1);
            (p[i], p[swapIndex]) = (p[swapIndex], p[i]);
        }
        for (int i = 0; i < 512; i++) Permutation[i] = p[i & 255];
    }

    public static float Calc2D(float x, float y)
    {
        int X = (int)Math.Floor(x) & 255;
        int Y = (int)Math.Floor(y) & 255;

        x -= (float)Math.Floor(x);
        y -= (float)Math.Floor(y);

        float u = Fade(x);
        float v = Fade(y);

        int A = Permutation[X] + Y;
        int B = Permutation[X + 1] + Y;

        return Lerp(v, Lerp(u, Grad(Permutation[A], x, y),
                               Grad(Permutation[B], x - 1, y)),
                       Lerp(u, Grad(Permutation[A + 1], x, y - 1),
                               Grad(Permutation[B + 1], x - 1, y - 1)));
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float Lerp(float t, float a, float b) => a + t * (b - a);
    private static float Grad(int hash, float x, float y)
    {
        int h = hash & 7;
        float u = h < 4 ? x : y;
        float v = h < 4 ? y : x;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}