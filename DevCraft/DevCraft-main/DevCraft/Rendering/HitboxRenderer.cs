using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DevCraft.World.Chunks;
using DevCraft.World.Blocks;
using DevCraft.Utilities;
using DevCraft.MathUtilities;

namespace DevCraft.Rendering;

class HitboxRenderer
{
    readonly GraphicsDevice graphics;
    readonly Effect effect;
    readonly DynamicVertexBuffer buffer;
    readonly Texture2D whiteTexture;

    public HitboxRenderer(GraphicsDevice graphics, Effect effect)
    {
        this.graphics = graphics;
        this.effect = effect;
        
        buffer = new DynamicVertexBuffer(graphics, typeof(VertexPositionTextureLight),
            10000, BufferUsage.WriteOnly);
            
        // Create a 1x1 white texture for the wireframe lines
        whiteTexture = Util.GetColoredTexture(graphics, 1, 1, Color.White);
    }

    public void RenderPlayerHitbox(BoundingBox playerBound, Matrix view, Matrix projection)
    {
        var vertices = CreateWireframeCube(playerBound, Color.Red);
        
        if (vertices.Length == 0) return;

        buffer.SetData(vertices);
        graphics.SetVertexBuffer(buffer);        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
        effect.Parameters["Texture"].SetValue(whiteTexture);
        effect.Parameters["Alpha"].SetValue(0.8f);

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphics.DrawPrimitives(PrimitiveType.LineList, 0, vertices.Length / 2);
        }
    }

    public void RenderBlockHitboxes(Region region, Camera camera, BlockMetadataProvider blockMetadata)
    {
        List<VertexPositionTextureLight> allVertices = new();
        
        // Get chunks within render distance
        Vec3<int> playerChunk = Chunk.WorldToChunkCoords(camera.Position);
        
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {                for (int z = -3; z <= 3; z++)
                {
                    Vec3<int> chunkIndex = new(playerChunk.X + x, playerChunk.Y + y, playerChunk.Z + z);
                    
                    Chunk chunk = region[chunkIndex];
                    if (chunk == null) continue;
                    
                    // Only render hitboxes for blocks near the player
                    foreach (Vec3<byte> blockIndex in chunk.GetVisibleBlocks())
                    {
                        Block block = chunk[blockIndex.X, blockIndex.Y, blockIndex.Z];
                        if (block.IsEmpty) continue;
                        
                        Vector3 blockWorldPos = Chunk.BlockIndexToWorldPosition(chunk.Position, blockIndex);
                        BoundingBox blockBounds = new(blockWorldPos, blockWorldPos + Vector3.One);
                        
                        // Only render if close to camera
                        if (Vector3.Distance(camera.Position, blockWorldPos) > 20f) continue;
                        
                        var vertices = CreateWireframeCube(blockBounds, Color.Blue);
                        allVertices.AddRange(vertices);
                    }
                }
            }
        }

        if (allVertices.Count == 0) return;

        var vertexArray = allVertices.ToArray();
        if (vertexArray.Length > buffer.VertexCount)
        {
            // Buffer too small, skip this frame
            return;
        }

        buffer.SetData(vertexArray);
        graphics.SetVertexBuffer(buffer);        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["View"].SetValue(camera.View);
        effect.Parameters["Projection"].SetValue(camera.Projection);
        effect.Parameters["Texture"].SetValue(whiteTexture);
        effect.Parameters["Alpha"].SetValue(0.5f);

        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphics.DrawPrimitives(PrimitiveType.LineList, 0, vertexArray.Length / 2);
        }
    }

    private static VertexPositionTextureLight[] CreateWireframeCube(BoundingBox bounds, Color color)
    {
        Vector3 min = bounds.Min;
        Vector3 max = bounds.Max;

        // Define the 8 corners of the cube
        Vector3[] corners = [
            new(min.X, min.Y, min.Z), // 0: bottom-front-left
            new(max.X, min.Y, min.Z), // 1: bottom-front-right  
            new(max.X, max.Y, min.Z), // 2: top-front-right
            new(min.X, max.Y, min.Z), // 3: top-front-left
            new(min.X, min.Y, max.Z), // 4: bottom-back-left
            new(max.X, min.Y, max.Z), // 5: bottom-back-right
            new(max.X, max.Y, max.Z), // 6: top-back-right
            new(min.X, max.Y, max.Z)  // 7: top-back-left
        ];

        // Define the 12 edges of the cube (24 vertices for 12 lines)
        List<VertexPositionTextureLight> vertices = new();

        // Bottom face edges
        AddLine(vertices, corners[0], corners[1], color);
        AddLine(vertices, corners[1], corners[5], color);
        AddLine(vertices, corners[5], corners[4], color);
        AddLine(vertices, corners[4], corners[0], color);

        // Top face edges  
        AddLine(vertices, corners[3], corners[2], color);
        AddLine(vertices, corners[2], corners[6], color);
        AddLine(vertices, corners[6], corners[7], color);
        AddLine(vertices, corners[7], corners[3], color);

        // Vertical edges
        AddLine(vertices, corners[0], corners[3], color);
        AddLine(vertices, corners[1], corners[2], color);
        AddLine(vertices, corners[5], corners[6], color);
        AddLine(vertices, corners[4], corners[7], color);

        return vertices.ToArray();
    }    private static void AddLine(List<VertexPositionTextureLight> vertices, Vector3 start, Vector3 end, Color color)
    {
        // Use maximum brightness (15 for both skylight and blocklight)
        const int maxLight = 15 + 397 * 15; // skylight + prime * blocklight
        
        vertices.Add(new VertexPositionTextureLight
        {
            Position = start,
            TextureCoordinate = Vector2.Zero,
            Light = maxLight
        });
        
        vertices.Add(new VertexPositionTextureLight
        {
            Position = end,
            TextureCoordinate = Vector2.Zero,
            Light = maxLight
        });
    }
}
