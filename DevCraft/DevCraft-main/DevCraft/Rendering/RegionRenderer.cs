using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

using DevCraft.Persistence;
using DevCraft.World.Chunks;
using DevCraft.Rendering.Meshers;

namespace DevCraft.Rendering;

class RegionRenderer
{
    readonly Region region;
    readonly GraphicsDevice graphics;
    readonly ScreenshotTaker screenshotTaker;

    readonly TextureAtlas atlas;
    readonly Effect effect;
    readonly DynamicVertexBuffer buffer;
    readonly ChunkMesher chunkMesher;

    public RegionRenderer(Region region, GraphicsDevice graphics, Effect effect, ChunkMesher chunkMesher,
        ScreenshotTaker screenshotTaker, TextureAtlas atlas)
    {
        this.region = region;
        this.graphics = graphics;
        this.effect = effect;
        this.chunkMesher = chunkMesher;
        this.screenshotTaker = screenshotTaker;
        this.atlas = atlas;

        buffer = new DynamicVertexBuffer(graphics, typeof(VertexPositionTextureLightColor),
                    (int)2e4, BufferUsage.WriteOnly);
    }

    public void Render(Camera camera, float lightIntensity)
    {
        Vector3 chunkMax = new(Chunk.Size);

        HashSet<Chunk> visibleChunks = [];

        // Set improved render states for cleaner block rendering
        graphics.DepthStencilState = DepthStencilState.Default;
        graphics.RasterizerState = RasterizerState.CullCounterClockwise;
        graphics.SamplerStates[0] = SamplerState.PointClamp;

        effect.Parameters["World"].SetValue(Matrix.Identity);
        effect.Parameters["View"].SetValue(camera.View);
        effect.Parameters["Projection"].SetValue(camera.Projection);
        effect.Parameters["Texture"].SetValue(atlas.Texture);
        effect.Parameters["LightIntensity"].SetValue(lightIntensity);

        graphics.Clear(Color.Lerp(Color.SkyBlue, Color.Black, 1f - lightIntensity));

        //Drawing opaque blocks
        foreach (Chunk chunk in region.GetActiveChunks())
        {
            if (chunk.IsEmpty || !chunk.IsReady) continue;

            BoundingBox chunkBounds = new(chunk.Position, chunkMax + chunk.Position);

            bool isChunkVisible = false;
            if (camera.Frustum.Intersects(chunkBounds))
            {
                visibleChunks.Add(chunk);
                isChunkVisible = true;
            }

            if (isChunkVisible)
            {
                effect.Parameters["Alpha"].SetValue(1f);

                var vertices = chunkMesher.GetVertices(chunk.Index);

                if (vertices.Length == 0) continue;

                buffer.SetData(vertices);
                graphics.SetVertexBuffer(buffer);

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Length / 3);
                }
            }
        }

        //Drawing transparent blocks
        foreach (Chunk chunk in visibleChunks)
        {
            var vertices = chunkMesher.GetTransparentVertices(chunk.Index);
            if (vertices.Length == 0) continue;

            effect.Parameters["Alpha"].SetValue(0.7f);

            buffer.SetData(vertices);
            graphics.SetVertexBuffer(buffer);

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphics.DrawPrimitives(PrimitiveType.TriangleList, 0, vertices.Length / 3);
            }

        }

        if (screenshotTaker.TakeScreenshot)
        {
            screenshotTaker.Screenshot(DateTime.Now.ToString());
        }
    }
}