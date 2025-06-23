using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Linq;

using DevCraft.World.Blocks;
using DevCraft.World.Chunks;
using DevCraft.MathUtilities;

namespace DevCraft.Persistence;

internal class ChunkSaveData
{
    public Vec3<int> ChunkIndex { get; set; }
    public Dictionary<string, ushort> BlockModifications { get; set; } = new();
}

internal class BlockModification
{
    public Vec3<int> ChunkIndex { get; set; }
    public Vec3<byte> BlockIndex { get; set; }
    public Block Block { get; set; }

    public BlockModification(Vec3<int> chunkIndex, Vec3<byte> blockIndex, Block block)
    {
        ChunkIndex = chunkIndex;
        BlockIndex = blockIndex;
        Block = block;
    }
}

public class FilePersistenceService : IDisposable
{
    readonly string saveDirectory;
    readonly string chunksDirectory;
    readonly BlockMetadataProvider blockMetadata;    readonly ConcurrentQueue<BlockModification> saveQueue = new();
    readonly ConcurrentDictionary<Vec3<int>, ChunkSaveData> chunkDataCache = new();
    readonly Task flushTask;
    readonly object fileLock = new();

    public FilePersistenceService(MainGame game, string saveName, BlockMetadataProvider blockMetadata)
    {
        this.blockMetadata = blockMetadata;

        // Set up directory structure
        saveDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Saves", saveName);
        chunksDirectory = Path.Combine(saveDirectory, "chunks");
        
        Initialize();

        // Start background save task
        flushTask = Task.Run(async () =>
        {
            const int delay = 2;
            TimeSpan delaySpan = TimeSpan.FromSeconds(delay);

            while (game.State == GameState.Running || !saveQueue.IsEmpty)
            {
                await FlushModificationsAsync();
                await Task.Delay(delaySpan);
            }
        });
    }    public void Initialize()
    {
        try
        {
            // Ensure directories exist
            Directory.CreateDirectory(saveDirectory);
            Directory.CreateDirectory(chunksDirectory);
            
            // Clean up any old SQLite database files
            CleanupOldDatabaseFiles();
            
            Console.WriteLine($"File persistence initialized at: {saveDirectory}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing file persistence: {ex.Message}");
            throw;
        }
    }

    public void AddDelta(Vec3<int> chunkIndex, Vec3<byte> blockIndex, Block block)
    {
        saveQueue.Enqueue(new BlockModification(chunkIndex, blockIndex, block));
    }

    public Block[,,] ApplyDelta(Chunk chunk, Block[,,] buffer)
    {
        try
        {
            string chunkFileName = GetChunkFileName(chunk.Index);
            string chunkFilePath = Path.Combine(chunksDirectory, chunkFileName);

            if (!File.Exists(chunkFilePath))
            {
                return buffer; // No modifications for this chunk
            }

            lock (fileLock)
            {
                string jsonContent = File.ReadAllText(chunkFilePath);
                var chunkData = JsonSerializer.Deserialize<ChunkSaveData>(jsonContent);

                if (chunkData?.BlockModifications == null || chunkData.BlockModifications.Count == 0)
                {
                    return buffer;
                }

                // Initialize buffer if needed
                if (buffer == null)
                {
                    buffer = Chunk.GetBlockArray();
                }

                // Apply all block modifications
                foreach (var modification in chunkData.BlockModifications)
                {
                    if (TryParseBlockKey(modification.Key, out Vec3<byte> blockIndex))
                    {
                        var block = new Block(modification.Value);
                        buffer[blockIndex.X, blockIndex.Y, blockIndex.Z] = block;

                        // Add light sources
                        if (!block.IsEmpty && blockMetadata.IsLightSource(block))
                        {
                            chunk.AddLightSource(blockIndex.X, blockIndex.Y, blockIndex.Z, block);
                        }
                    }
                }

                return buffer;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying delta for chunk {chunk.Index}: {ex.Message}");
            return buffer;
        }
    }

    Task FlushModificationsAsync()
    {
        if (saveQueue.IsEmpty) return Task.CompletedTask;

        return Task.Run(() =>
        {
            try
            {
                // Group modifications by chunk
                var chunkModifications = new Dictionary<Vec3<int>, List<BlockModification>>();

                while (saveQueue.TryDequeue(out BlockModification modification))
                {
                    if (!chunkModifications.ContainsKey(modification.ChunkIndex))
                    {
                        chunkModifications[modification.ChunkIndex] = new List<BlockModification>();
                    }
                    chunkModifications[modification.ChunkIndex].Add(modification);
                }

                // Save each chunk's modifications
                foreach (var kvp in chunkModifications)
                {
                    SaveChunkModifications(kvp.Key, kvp.Value);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error flushing modifications: {ex.Message}");
            }
        });
    }

    void SaveChunkModifications(Vec3<int> chunkIndex, List<BlockModification> modifications)
    {
        try
        {
            string chunkFileName = GetChunkFileName(chunkIndex);
            string chunkFilePath = Path.Combine(chunksDirectory, chunkFileName);

            lock (fileLock)
            {
                ChunkSaveData chunkData;

                // Load existing data or create new
                if (File.Exists(chunkFilePath))
                {
                    string existingContent = File.ReadAllText(chunkFilePath);
                    chunkData = JsonSerializer.Deserialize<ChunkSaveData>(existingContent) ?? new ChunkSaveData();
                }
                else
                {
                    chunkData = new ChunkSaveData();
                }

                chunkData.ChunkIndex = chunkIndex;

                // Apply modifications
                foreach (var modification in modifications)
                {
                    string blockKey = GetBlockKey(modification.BlockIndex);
                    
                    if (modification.Block.IsEmpty)
                    {
                        // Remove block (set to air)
                        chunkData.BlockModifications.Remove(blockKey);
                    }
                    else
                    {
                        // Add or update block
                        chunkData.BlockModifications[blockKey] = modification.Block.Value;
                    }
                }

                // Save to file
                string jsonContent = JsonSerializer.Serialize(chunkData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                File.WriteAllText(chunkFilePath, jsonContent);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving chunk modifications for {chunkIndex}: {ex.Message}");
        }
    }

    string GetChunkFileName(Vec3<int> chunkIndex)
    {
        return $"chunk_{chunkIndex.X}_{chunkIndex.Y}_{chunkIndex.Z}.json";
    }

    string GetBlockKey(Vec3<byte> blockIndex)
    {
        return $"{blockIndex.X},{blockIndex.Y},{blockIndex.Z}";
    }

    bool TryParseBlockKey(string key, out Vec3<byte> blockIndex)
    {
        blockIndex = default;
        
        try
        {
            string[] parts = key.Split(',');
            if (parts.Length == 3)
            {
                byte x = byte.Parse(parts[0]);
                byte y = byte.Parse(parts[1]);
                byte z = byte.Parse(parts[2]);
                blockIndex = new Vec3<byte>(x, y, z);
                return true;
            }
        }
        catch
        {
            // Parsing failed
        }

        return false;
    }

    public void DeleteChunkData(Vec3<int> chunkIndex)
    {
        try
        {
            string chunkFileName = GetChunkFileName(chunkIndex);
            string chunkFilePath = Path.Combine(chunksDirectory, chunkFileName);
            
            if (File.Exists(chunkFilePath))
            {
                File.Delete(chunkFilePath);
                Console.WriteLine($"Deleted chunk data for {chunkIndex}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting chunk data for {chunkIndex}: {ex.Message}");
        }
    }

    public long GetSaveSize()
    {
        try
        {
            var dirInfo = new DirectoryInfo(saveDirectory);
            return dirInfo.GetFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
        }
        catch
        {
            return 0;
        }
    }

    public int GetModifiedChunkCount()
    {
        try
        {
            return Directory.GetFiles(chunksDirectory, "*.json").Length;
        }
        catch
        {
            return 0;
        }
    }

    public void CleanupOldDatabaseFiles()
    {
        try
        {
            // Remove old SQLite database file if it exists
            string dbPath = Path.Combine(saveDirectory, "data.db");
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                Console.WriteLine($"Cleaned up old database file: {dbPath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cleaning up old database files: {ex.Message}");
        }
    }

    private bool disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {            if (disposing)
            {
                try
                {
                    // Wait for the flush task to complete
                    flushTask?.Wait(TimeSpan.FromSeconds(10));
                    
                    // Final flush of any remaining data
                    FlushModificationsAsync().Wait(TimeSpan.FromSeconds(5));
                    
                    Console.WriteLine("File persistence service disposed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing file persistence service: {ex.Message}");
                }
            }
            disposed = true;
        }
    }
}
