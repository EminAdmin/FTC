using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_cs;

namespace FunTimeCobra;

public struct Particle
{
    public Vector3 Position;
    public Vector3 Velocity;
    public Color Color;
    public float Size;
    public float Life;
    public float MaxLife;
}

public static class ParticleSystem
{
    private static readonly List<Particle> particles = new(512);
    private static readonly Random rand = new();

    public static void SpawnBlockBreak(Vector3 pos, Color col, int count = 16)
    {
        for (int i = 0; i < count; i++)
        {
            float vx = ((float)rand.NextDouble() - 0.5f) * 3.5f;
            float vy = (float)rand.NextDouble() * 3.0f + 1.2f;
            float vz = ((float)rand.NextDouble() - 0.5f) * 3.5f;

            particles.Add(new Particle
            {
                Position = pos + new Vector3((float)rand.NextDouble() * 0.8f + 0.1f, (float)rand.NextDouble() * 0.8f + 0.1f, (float)rand.NextDouble() * 0.8f + 0.1f),
                Velocity = new Vector3(vx, vy, vz),
                Color = col,
                Size = (float)rand.NextDouble() * 0.08f + 0.05f,
                Life = 0.8f,
                MaxLife = 0.8f
            });
        }
    }

    public static void SpawnAuraParticle(Vector3 playerPos, Color col, float spread = 0.6f)
    {
        float angle = (float)rand.NextDouble() * MathF.PI * 2f;
        float r = (float)rand.NextDouble() * spread;
        Vector3 offset = new Vector3(MathF.Cos(angle) * r, (float)rand.NextDouble() * 1.6f, MathF.Sin(angle) * r);

        particles.Add(new Particle
        {
            Position = playerPos + offset,
            Velocity = new Vector3(0, 0.4f + (float)rand.NextDouble() * 0.5f, 0),
            Color = col,
            Size = 0.07f,
            Life = 0.6f,
            MaxLife = 0.6f
        });
    }

    public static void SpawnTotemEffect(Vector3 pos)
    {
        for (int i = 0; i < 64; i++)
        {
            float angle = (float)rand.NextDouble() * MathF.PI * 2f;
            float speed = (float)rand.NextDouble() * 4f + 2f;
            Color c = (i % 2 == 0) ? new Color(255, 215, 0, 255) : new Color(50, 240, 120, 255);

            particles.Add(new Particle
            {
                Position = pos + new Vector3(0, 1f, 0),
                Velocity = new Vector3(MathF.Cos(angle) * speed, ((float)rand.NextDouble() - 0.2f) * 4f, MathF.Sin(angle) * speed),
                Color = c,
                Size = 0.12f,
                Life = 1.2f,
                MaxLife = 1.2f
            });
        }
    }

    public static void SpawnBeaconBeam(Vector3 basePos, Color col)
    {
        for (int y = 0; y < 40; y += 2)
        {
            particles.Add(new Particle
            {
                Position = basePos + new Vector3(((float)rand.NextDouble() - 0.5f) * 0.4f, y + (float)rand.NextDouble(), ((float)rand.NextDouble() - 0.5f) * 0.4f),
                Velocity = new Vector3(0, 8f, 0),
                Color = col,
                Size = 0.25f,
                Life = 0.4f,
                MaxLife = 0.4f
            });
        }
    }

    public static void Update(float dt)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.Life -= dt;
            if (p.Life <= 0)
            {
                particles.RemoveAt(i);
                continue;
            }

            p.Velocity.Y -= 9.8f * dt * 0.5f;
            p.Position += p.Velocity * dt;
            particles[i] = p;
        }
    }

    public static void Render()
    {
        foreach (var p in particles)
        {
            float alpha = p.Life / p.MaxLife;
            Color c = new Color(p.Color.R, p.Color.G, p.Color.B, (byte)(p.Color.A * alpha));
            Raylib.DrawCube(p.Position, p.Size, p.Size, p.Size, c);
        }
    }
}