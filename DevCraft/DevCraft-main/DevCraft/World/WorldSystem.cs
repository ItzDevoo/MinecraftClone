using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

using DevCraft.GUI.Menus;
using DevCraft.MathUtilities;
using DevCraft.Persistence;
using DevCraft.Rendering.Meshers;
using DevCraft.Utilities;
using DevCraft.World.Blocks;
using DevCraft.World.Chunks;
using DevCraft.World.Generation;
using DevCraft.World.Lighting;

namespace DevCraft.World;
class WorldSystem : IDisposable
{
    Player player;
    readonly Region region;
    readonly ChunkModificationSystem chunkModSystem;

    readonly GameMenu gameMenu;
    readonly ChunkGenerator chunkGenerator;
    readonly BlockOutlineMesher blockOutlineMesher;

    readonly WorldGenerator worldGenerator;

    public WorldSystem(Region region, GameMenu gameMenu, FilePersistenceService persistenceService,
        Parameters parameters, BlockMetadataProvider blockMetadata,
        ChunkMesher chunkMesher, BlockOutlineMesher blockOutlineMesher)
    {
        this.region = region;
        this.gameMenu = gameMenu;

        chunkGenerator = new ChunkGenerator(parameters, persistenceService, blockMetadata);
        this.blockOutlineMesher = blockOutlineMesher;

        LightSystem lightSystem = new();

        chunkModSystem = new ChunkModificationSystem(persistenceService, blockMetadata, lightSystem,
            chunk => worldGenerator.PostToMesher(chunk));

        worldGenerator = new WorldGenerator(region, chunkGenerator, lightSystem, chunkMesher, Environment.ProcessorCount);
    }

    public void Init(Player player, Parameters parameters)
    {
        this.player = player;
        player.Flying = true;

        Console.WriteLine($"Initializing world system. Initial parameters position: {parameters.Position}");

        // Set player position BEFORE world generation
        if (parameters.Position == Vector3.Zero)
        {
            Console.WriteLine("New world detected, finding safe spawn position...");
            // For new worlds, find a proper spawn height
            Vector3 spawnPosition = FindSafeSpawnPosition();
            player.Position = spawnPosition;
            parameters.Position = spawnPosition; // Update parameters to persist the spawn position
            Console.WriteLine($"Set new spawn position to: {spawnPosition}");
        }
        else
        {
            Console.WriteLine($"Loading existing world, using saved position: {parameters.Position}");
            player.Position = parameters.Position;
        }

        // Now generate chunks around the properly set player position
        Console.WriteLine($"Generating world around player position: {player.Position}");
        worldGenerator.BulkGenerate(player.Position);

        Vec3<int> currentPlayerIndex = Chunk.WorldToChunkCoords(player.Position);
        player.Index = currentPlayerIndex;
        Console.WriteLine($"Player chunk index set to: {currentPlayerIndex}");
    }

    private Vector3 FindSafeSpawnPosition()
    {
        try
        {
            // Generate a small area around spawn to find safe height
            Vector3 basePosition = new Vector3(0, 0, 0);
            
            // Pre-generate a few chunks around spawn to determine terrain height
            List<Vec3<int>> spawnChunks = [];
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    spawnChunks.Add(new Vec3<int>(x, 0, z));
                    spawnChunks.Add(new Vec3<int>(x, 1, z)); // Include chunk above
                }
            }

            // Generate these chunks synchronously to find terrain height
            foreach (var chunkIndex in spawnChunks)
            {
                if (region[chunkIndex] == null)
                {
                    try
                    {
                        var chunk = chunkGenerator.GenerateChunk(chunkIndex);
                        region[chunkIndex] = chunk;
                        region.LinkChunk(chunk);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error generating spawn chunk {chunkIndex}: {ex.Message}");
                        continue;
                    }
                }
            }

            // Find the highest solid block at spawn coordinates
            int highestY = 50; // Fallback height
            Vec3<int> spawnChunkIndex = Chunk.WorldToChunkCoords(basePosition);
            Chunk spawnChunk = region[spawnChunkIndex];
            
            if (spawnChunk != null)
            {
                Vec3<byte> blockIndex = Chunk.WorldToBlockCoords(basePosition);
                
                // Search upward for the highest solid block
                for (int y = 0; y < 255; y++)
                {
                    Vec3<int> currentChunkIndex = Chunk.WorldToChunkCoords(new Vector3(0, y, 0));
                    Chunk currentChunk = region[currentChunkIndex];
                    
                    if (currentChunk != null)
                    {
                        Vec3<byte> currentBlockIndex = Chunk.WorldToBlockCoords(new Vector3(0, y, 0));
                        if (!currentChunk[currentBlockIndex.X, currentBlockIndex.Y, currentBlockIndex.Z].IsEmpty)
                        {
                            highestY = y;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"Warning: Could not find spawn chunk at {spawnChunkIndex}, using fallback height");
            }

            // Spawn 2 blocks above the highest solid block
            Vector3 spawnPosition = new Vector3(0, highestY + 2, 0);
            Console.WriteLine($"Found safe spawn position: {spawnPosition}");
            return spawnPosition;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error finding safe spawn position: {ex.Message}");
            // Fallback to a safe default position
            return new Vector3(0, 100, 0);
        }
    }

    public void Update(GameTime gameTime, bool exitedMenu)
    {
        UpdateEntities(gameTime, exitedMenu);

        Vec3<int> currentPlayerIndex = Chunk.WorldToChunkCoords(player.Position);

        if (player.Index != currentPlayerIndex)
        {
            worldGenerator.Update(player.Position);
            player.Index = currentPlayerIndex;
        }
    }

    public void UpdateEntities(GameTime gameTime, bool exitedMenu)
    {
        player.Update(gameTime);

        UpdateEntitiesPhysics(exitedMenu);

        if (player.UpdateOccured)
        {
            ProcessPlayerActions(exitedMenu);
        }

        chunkModSystem.Update();
    }

    void ProcessPlayerActions(bool exitedMenu)
    {
        if (exitedMenu) return;

        Raycaster raycaster = new(player.Camera.Position,
            player.Camera.Direction, 0.1f);

        const float maxDistance = 5.0f;

        Vector3 blockPosition = player.Camera.Position;
        Vec3<int> chunkIndex;// = Chunk.WorldToChunkCoords(blockPosition);
        Vec3<byte> blockIndex = Chunk.WorldToBlockCoords(blockPosition);
        Block block = Block.Empty;
        Chunk chunk = null;

        while (raycaster.Length(blockPosition) < maxDistance)
        {
            blockPosition = raycaster.Step();
            chunkIndex = Chunk.WorldToChunkCoords(blockPosition);
            blockIndex = Chunk.WorldToBlockCoords(blockPosition);

            chunk = region[chunkIndex];
            block = chunk[blockIndex.X, blockIndex.Y, blockIndex.Z];
            if (!block.IsEmpty) break;
        }

        if (chunk == null || block.IsEmpty)
        {
            blockOutlineMesher.Flush();
            return;
        }

        if (player.LeftClick)
        {
            chunkModSystem.Add(blockIndex, chunk, BlockInteractionMode.Remove);
        }
        else if (player.RightClick)
        {
            if ((blockPosition - player.Position).Length() > 1.1f)
            {
                chunkModSystem.Add(new Block(gameMenu.SelectedItem), blockIndex, player.Camera.Direction, chunk, BlockInteractionMode.Add);
            }
        }

        FacesState visibleFaces = chunk.GetVisibleFaces(blockIndex);
        blockOutlineMesher.GenerateMesh(visibleFaces, new Vector3(blockIndex.X, blockIndex.Y, blockIndex.Z) + chunk.Position, player.Camera.Direction);
    }

    void UpdateEntitiesPhysics(bool exitedMenu)
    {
        if (exitedMenu) return;

        var playerBox = player.Bound;
        Vector3 playerCenter = (playerBox.Min + playerBox.Max) * 0.5f;

        ReadOnlySpan<Vector3> collisionPoints = [
                // The eight corners
                new Vector3(playerBox.Min.X, playerBox.Min.Y, playerBox.Min.Z),
                    new Vector3(playerBox.Min.X, playerBox.Min.Y, playerBox.Max.Z),
                    new Vector3(playerBox.Min.X, playerBox.Max.Y, playerBox.Min.Z),
                    new Vector3(playerBox.Min.X, playerBox.Max.Y, playerBox.Max.Z),
                    new Vector3(playerBox.Max.X, playerBox.Min.Y, playerBox.Min.Z),
                    new Vector3(playerBox.Max.X, playerBox.Min.Y, playerBox.Max.Z),
                    new Vector3(playerBox.Max.X, playerBox.Max.Y, playerBox.Min.Z),
                    new Vector3(playerBox.Max.X, playerBox.Max.Y, playerBox.Max.Z),

                    // Centers of the bottom and top faces
                    new Vector3(playerCenter.X, playerBox.Min.Y, playerCenter.Z),
                    new Vector3(playerCenter.X, playerBox.Max.Y, playerCenter.Z),

                    // Centers of the left and right faces (horizontal sides)
                    new Vector3(playerBox.Min.X, playerCenter.Y, playerCenter.Z),
                    new Vector3(playerBox.Max.X, playerCenter.Y, playerCenter.Z),

                    // Centers of the front and back faces
                    new Vector3(playerCenter.X, playerCenter.Y, playerBox.Min.Z),
                    new Vector3(playerCenter.X, playerCenter.Y, playerBox.Max.Z)
        ];

        // A hashset of chunk and block indices
        var collisionIndices = new HashSet<(Vec3<int>, Vec3<byte>)>();
        foreach (var point in collisionPoints)
        {
            Vec3<int> chunkIndex = Chunk.WorldToChunkCoords(point);
            Vec3<byte> blockIndex = Chunk.WorldToBlockCoords(point);
            collisionIndices.Add((chunkIndex, blockIndex));
        }

        List<BoundingBox> collidableBlockBounds = [];

        foreach (var (chunkIndex, blockIndex) in collisionIndices)
        {
            Chunk chunk = region[chunkIndex];

            Block block = chunk[blockIndex.X, blockIndex.Y, blockIndex.Z];
            if (!block.IsEmpty)
            {
                Vector3 blockWorldPos = Chunk.BlockIndexToWorldPosition(chunk.Position, blockIndex);
                BoundingBox blockBounds = new(blockWorldPos, blockWorldPos + Vector3.One);
                collidableBlockBounds.Add(blockBounds);
            }
        }

        foreach (var bound in collidableBlockBounds)
        {
            player.Physics.ResolveCollision(bound);
        }
    }

    bool disposed;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        disposed = true;

        if (disposing)
        {
            worldGenerator.Dispose();
        }
    }
}
