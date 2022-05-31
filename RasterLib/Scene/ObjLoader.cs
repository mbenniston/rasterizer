using System.Collections.Generic;
using System.IO;
using RasterLib.LinearMath;

namespace RasterLib.Scene;

/// <summary>
///     Contains functions to load a simple mesh from an Obj file.
///     Does not support mtl files or any other primitive than triangles
/// </summary>
public static class ObjLoader
{
    private struct Face
    {
        public int[] vertices = new int[3];
        public int[] normals = new int[3];
        public int[] texCoords = new int[3];

        public Face()
        {

        }
    }

    public static Mesh Load(string fileName)
    {
        StreamReader reader = new(File.Open(fileName, FileMode.Open));

        List<Face> faces = new();
        List<Vector3> vertices = new();
        List<Vector3> normals = new();
        List<Vector2> texCoords = new();

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (line.StartsWith("v "))
            {
                string[] parts = line.Split(' ');

                vertices.Add(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])));
            }
            else if (line.StartsWith("vn "))
            {
                string[] parts = line.Split(' ');

                normals.Add(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])));

            }
            else if (line.StartsWith("vt "))
            {
                string[] parts = line.Split(' ');

                texCoords.Add(new Vector2(
                                float.Parse(parts[1]),
                                float.Parse(parts[2])));

            }
            else if (line.StartsWith("f "))
            {
                string[] parts = line.Split(' ');
                Face face = new();
                face.vertices[0] = int.Parse(parts[1].Split('/')[0]) - 1;
                face.vertices[1] = int.Parse(parts[2].Split('/')[0]) - 1;
                face.vertices[2] = int.Parse(parts[3].Split('/')[0]) - 1;

                face.texCoords[0] = int.Parse(parts[1].Split('/')[1]) - 1;
                face.texCoords[1] = int.Parse(parts[2].Split('/')[1]) - 1;
                face.texCoords[2] = int.Parse(parts[3].Split('/')[1]) - 1;

                face.normals[0] = int.Parse(parts[1].Split('/')[2]) - 1;
                face.normals[1] = int.Parse(parts[2].Split('/')[2]) - 1;
                face.normals[2] = int.Parse(parts[3].Split('/')[2]) - 1;

                faces.Add(face);
            }
        }

        reader.Close();

        var meshVertices = new Mesh.Vertex[faces.Count * 3];

        for (int i = 0; i < faces.Count; i++)
        {
            Face face = faces[i];
            meshVertices[i * 3] = new Mesh.Vertex(vertices[face.vertices[0]], normals[face.normals[0]], texCoords[face.texCoords[0]]);
            meshVertices[i * 3 + 1] = new Mesh.Vertex(vertices[face.vertices[1]], normals[face.normals[1]], texCoords[face.texCoords[1]]);
            meshVertices[i * 3 + 2] = new Mesh.Vertex(vertices[face.vertices[2]], normals[face.normals[2]], texCoords[face.texCoords[2]]);
        }

        return new Mesh(meshVertices);
    }
}