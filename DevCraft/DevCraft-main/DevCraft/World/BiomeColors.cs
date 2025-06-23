using Microsoft.Xna.Framework;

namespace DevCraft.World;

/// <summary>
/// Handles biome-based block coloring, particularly for grass and leaves
/// </summary>
public static class BiomeColors
{
    /// <summary>
    /// Standard grass colors for different biome types
    /// </summary>
    public static readonly Color[] GrassColors = new[]
    {
        new Color(91, 181, 91),   // Plains (default green)
        new Color(138, 177, 94),  // Forest (darker green)
        new Color(115, 165, 115), // Taiga (cooler green)
        new Color(189, 178, 95),  // Desert (yellowish)
        new Color(128, 180, 151), // Swamp (murky green)
        new Color(102, 142, 134), // Dark Forest (very dark)
        new Color(80, 112, 80),   // Mountains (alpine)
        new Color(144, 186, 144)  // Meadow (bright green)
    };

    /// <summary>
    /// Standard leaf colors for different biome types
    /// </summary>
    public static readonly Color[] LeafColors = new[]
    {
        new Color(102, 142, 64),  // Plains
        new Color(91, 128, 91),   // Forest
        new Color(95, 132, 95),   // Taiga
        new Color(174, 164, 115), // Desert
        new Color(106, 156, 106), // Swamp
        new Color(86, 122, 86),   // Dark Forest
        new Color(70, 102, 70),   // Mountains
        new Color(118, 156, 118)  // Meadow
    };

    /// <summary>
    /// Gets the grass color for a given world position
    /// Currently uses a simple noise-based biome system
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldZ">World Z coordinate</param>
    /// <returns>Grass color for this location</returns>
    public static Color GetGrassColor(int worldX, int worldZ)
    {
        // Simple noise-based biome selection
        // In a real implementation, this would use proper biome generation
        int biomeIndex = GetBiomeIndex(worldX, worldZ);
        return GrassColors[biomeIndex];
    }

    /// <summary>
    /// Gets the leaf color for a given world position
    /// </summary>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldZ">World Z coordinate</param>
    /// <returns>Leaf color for this location</returns>
    public static Color GetLeafColor(int worldX, int worldZ)
    {
        int biomeIndex = GetBiomeIndex(worldX, worldZ);
        return LeafColors[biomeIndex];
    }

    /// <summary>
    /// Simple biome index calculation based on world coordinates
    /// </summary>
    private static int GetBiomeIndex(int worldX, int worldZ)
    {
        // Use a simple hash-based approach for now
        // This creates varied but consistent biome distribution
        int hash = (worldX * 73856093 ^ worldZ * 19349663) & 0x7FFFFFFF;
        
        // Create some larger biome regions by reducing resolution
        hash = ((worldX / 64) * 73856093 ^ (worldZ / 64) * 19349663) & 0x7FFFFFFF;
        
        return hash % GrassColors.Length;
    }

    /// <summary>
    /// Checks if a block type should use biome coloring
    /// </summary>
    /// <param name="blockType">The block type name</param>
    /// <returns>True if the block should be colored by biome</returns>
    public static bool ShouldUseBiomeColoring(string blockType)
    {
        return blockType switch
        {
            "grass_top" => true,
            "grass_side" => true,
            "leaves" => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the appropriate biome color for a block type at a specific location
    /// </summary>
    /// <param name="blockType">The block type name</param>
    /// <param name="worldX">World X coordinate</param>
    /// <param name="worldZ">World Z coordinate</param>
    /// <returns>The biome color, or white if no coloring should be applied</returns>
    public static Color GetBiomeColor(string blockType, int worldX, int worldZ)
    {
        return blockType switch
        {
            "grass_top" or "grass_side" => GetGrassColor(worldX, worldZ),
            "leaves" => GetLeafColor(worldX, worldZ),
            _ => Color.White
        };
    }
}
