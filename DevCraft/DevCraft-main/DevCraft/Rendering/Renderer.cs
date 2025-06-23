using Microsoft.Xna.Framework.Graphics;

using DevCraft.Assets;
using DevCraft.Utilities;
using DevCraft.World.Chunks;
using DevCraft.Persistence;
using DevCraft.Rendering.Meshers;
using DevCraft.World.Blocks;

namespace DevCraft.Rendering;

class Renderer
{
    readonly RegionRenderer regionRenderer;
    readonly UIRenderer uiRenderer;
    readonly HitboxRenderer hitboxRenderer;
    readonly Region region;
    readonly BlockMetadataProvider blockMetadata;

    public Renderer(Region region, GraphicsDevice graphics, AssetServer assetServer,
        ScreenshotTaker screenshotTaker,
        ChunkMesher chunkMesher, BlockOutlineMesher blockOutlineMesher, BlockMetadataProvider blockMetadata)
    { 
        var effect = assetServer.Effect;
        
        regionRenderer = new RegionRenderer(region, graphics, effect, chunkMesher, screenshotTaker, assetServer.Atlas);
        uiRenderer = new UIRenderer(graphics, effect, blockOutlineMesher, assetServer);
        hitboxRenderer = new HitboxRenderer(graphics, effect);
        
        this.region = region;
        this.blockMetadata = blockMetadata;
    }

    public void Render(Camera camera, Time time, Player player, bool showHitboxes)
    {
        regionRenderer.Render(camera, time.LightIntensity);
        uiRenderer.Render();
        
        if (showHitboxes)
        {
            hitboxRenderer.RenderPlayerHitbox(player.Bound, camera.View, camera.Projection);
            hitboxRenderer.RenderBlockHitboxes(region, camera, blockMetadata);
        }
    }
}