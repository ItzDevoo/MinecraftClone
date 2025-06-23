using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using DevCraft.Utilities;

namespace DevCraft.Rendering.Meshers;

class BlockOutlineMesher
{
    public VertexPositionTextureLight[] Vertices { get; internal set; } = [];

    public void GenerateMesh(FacesState visibleFaces, Vector3 position, Vector3 direction)
    {
        List<VertexPositionTextureLight> vertexList = [];
        
        // Render outlines on ALL visible faces instead of just the dominant axis
        foreach (Faces face in visibleFaces.GetFaces())
        {
            for (int i = 0; i < 6; i++)
            {
                VertexPositionTextureLightColor cubeVertex = Cube.Faces[(byte)face][i];
                VertexPositionTextureLight vertex = cubeVertex; // Implicit conversion

                vertex.Position += position;
                vertexList.Add(vertex);
            }
        }

        Vertices = [.. vertexList];
    }

    public void Flush()
    {
        Vertices = [];
    }
}

