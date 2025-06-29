using System;
using Microsoft.Xna.Framework;

using DevCraft.MathUtilities;
using DevCraft.World.Blocks;
using DevCraft.World.Chunks;

namespace DevCraft.World.Generation;

public enum BiomeType : byte
{
    River,
    Forest,
    Mountain
}

record TerrainData(int[,] TerrainLevel, BiomeType[,] BiomesData, int MaxElevation);

class TerrainGenerator
{
    readonly FastNoiseLite terrain;
    readonly FastNoiseLite forest;
    readonly FastNoiseLite mountain;
    readonly FastNoiseLite river;

    readonly ushort bedrock, grass, stone, dirt, snow,
           leaves, birch, oak, water,
           sand, sandstone;

    public TerrainGenerator(int seed, BlockMetadataProvider blockMetadata)
    {
        bedrock = blockMetadata.GetBlockIndex("bedrock");
        grass = blockMetadata.GetBlockIndex("grass_side");
        stone = blockMetadata.GetBlockIndex("stone");
        dirt = blockMetadata.GetBlockIndex("dirt");
        snow = blockMetadata.GetBlockIndex("snow");
        leaves = blockMetadata.GetBlockIndex("leaves");
        birch = blockMetadata.GetBlockIndex("birch_log");
        oak = blockMetadata.GetBlockIndex("oak_log");
        water = blockMetadata.GetBlockIndex("water");
        sand = blockMetadata.GetBlockIndex("sand");
        sandstone = blockMetadata.GetBlockIndex("sandstone_top");

        terrain = new FastNoiseLite(seed);
        terrain.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        terrain.SetFrequency(0.001f);

        forest = new FastNoiseLite(seed);
        forest.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        forest.SetFractalType(FastNoiseLite.FractalType.Ridged);

        mountain = new FastNoiseLite(seed);
        mountain.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        mountain.SetFractalType(FastNoiseLite.FractalType.Ridged);
        mountain.SetFrequency(0.005f);

        river = new FastNoiseLite(seed);
        river.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        river.SetFractalType(FastNoiseLite.FractalType.Ridged);
        river.SetFrequency(0.001f);
    }

    public TerrainData GenerateTerrainData(Vector3 position, Vec2<int> cacheIndex)
    {
        int maxElevation = int.MinValue;

        var terrainLevel = new int[Chunk.Size, Chunk.Size];
        var biomes = new BiomeType[Chunk.Size, Chunk.Size];

        for (int x = 0; x < Chunk.Size; x++)
        {
            for (int z = 0; z < Chunk.Size; z++)
            {
                (int height, BiomeType biome) = GetHeight(position, x, z);
                terrainLevel[x, z] = height;
                biomes[x, z] = biome;

                if (maxElevation < height)
                {
                    maxElevation = height;
                }
            }
        }

        return new TerrainData(terrainLevel, biomes, maxElevation);
    }

    (int, BiomeType) GetHeight(Vector3 position, int x, int z)
    {
        float xVal = x + position.X;
        float zVal = z + position.Z;

        int height;
        BiomeType biome;
        float noise = Math.Abs(terrain.GetNoise(xVal, zVal) + 0.5f);

        if (noise < 0.05f)
        {
            height = (int)Math.Abs(8 * (river.GetNoise(xVal, zVal) + 0.8f)) + 30;
            biome = BiomeType.River;
        }
        else if (noise < 1.2f)
        {
            height = (int)Math.Abs(10 * (forest.GetNoise(xVal, zVal) + 0.8f)) + 30;
            biome = BiomeType.Forest;
        }
        else
        {
            height = (int)Math.Abs(30 * (mountain.GetNoise(xVal, zVal) + 0.8f)) + 30;
            biome = BiomeType.Mountain;
        }

        return (height, biome);
    }

    public ushort Fill(int terrainHeight, int currentY, BiomeType biome, Random rnd)
    {
        switch (biome)
        {
            case BiomeType.River:
                {
                    if (currentY < -100)
                    {
                        return bedrock;
                    }

                    if (currentY == terrainHeight - 1)
                    {
                        return sand;
                    }
                    else if (currentY > terrainHeight - 6)
                    {
                        if (rnd.Next(0, 2) == 0)
                            return sandstone;
                        return sand;
                    }

                    ushort texture = stone;

                    if (rnd.Next(0, 2) == 0)
                        texture = dirt;

                    return texture;
                }

            case BiomeType.Forest:
                {
                    if (currentY < -100)
                    {
                        return bedrock;
                    }

                    if (currentY == terrainHeight - 1)
                    {
                        return grass;
                    }

                    ushort texture = stone;

                    if (rnd.Next(0, 2) == 0)
                        texture = dirt;

                    return texture;
                }

            case BiomeType.Mountain:
                {
                    if (currentY < -100)
                    {
                        return bedrock;
                    }

                    if (currentY < 50 && currentY == terrainHeight - 1)
                    {
                        return grass;
                    }

                    if (currentY >= 60 && currentY == terrainHeight - 1)
                    {
                        return snow;
                    }

                    return stone;
                }
        }

        return 0;
    }
}
