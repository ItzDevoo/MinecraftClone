using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DevCraft.Rendering;

public struct VertexPositionTextureLightColor(Vector3 position, Vector2 textureCoordinate, float light, Color color) : IVertexType
{
    public Vector3 Position { get; set; } = position;
    public Vector2 TextureCoordinate { get; set; } = textureCoordinate;
    public float Light { get; set; } = light;
    public Color Color { get; set; } = color;

    static readonly VertexDeclaration vertexDeclaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(sizeof(float) * 5, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
        new VertexElement(sizeof(float) * 6, VertexElementFormat.Color, VertexElementUsage.Color, 0)
    );

    public readonly VertexDeclaration VertexDeclaration => vertexDeclaration;

    // Implicit conversion from the old vertex structure for compatibility
    public static implicit operator VertexPositionTextureLightColor(VertexPositionTextureLight vertex)
    {
        return new VertexPositionTextureLightColor(vertex.Position, vertex.TextureCoordinate, vertex.Light, Color.White);
    }
}
