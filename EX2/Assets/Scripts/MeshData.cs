using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MeshData
{
    public List<Vector3> vertices; // The vertices of the mesh 
    public List<int> triangles; // Indices of vertices that make up the mesh faces
    public Vector3[] normals; // The normals of the mesh, one per vertex

    // Class initializer
    public MeshData()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    // Returns a Unity Mesh of this MeshData that can be rendered
    public Mesh ToUnityMesh()
    {
        Mesh mesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            normals = normals
        };

        return mesh;
    }

    // Calculates surface normals for each vertex, according to face orientation
    public void CalculateNormals()
    {
        Vector3[] surfaceNormals = SurfaceNormals.ToArray();

        var facesPerVertex = new SortedDictionary<int, List<int>>();
        foreach (var item in FaceVertices.Select((faceVertices, face) => new { face, faceVertices }))
        {
            foreach (int vertex in new[] { item.faceVertices.Item1, item.faceVertices.Item2, item.faceVertices.Item3 })
            {
                if (!facesPerVertex.ContainsKey(vertex))
                {
                    facesPerVertex[vertex] = new List<int>();
                }

                facesPerVertex[vertex].Add(item.face);
            }
        }

        // For each vertex, get the normals of all the surfaces it is contained in, then sum 'em up and normalize.
        normals = facesPerVertex.Select(pair =>
            pair.Value.Select(faceIndex => surfaceNormals[faceIndex]).Aggregate((a, b) => a + b).normalized).ToArray();
        Debug.Assert(normals.Length == vertices.Count);
    }

    // Edits mesh such that each face has a unique set of 3 vertices
    public void MakeFlatShaded()
    {
        // Your implementation
    }

    private IEnumerable<Tuple<int, int, int>> FaceVertices
    {
        get
        {
            Debug.Assert(triangles.Count % 3 == 0);

            for (var index = 0; index < triangles.Count; index += 3)
            {
                yield return new Tuple<int, int, int>(
                    triangles[index + 0],
                    triangles[index + 1],
                    triangles[index + 2]);
            }
        }
    }

    private IEnumerable<Vector3> SurfaceNormals
    {
        get
        {
            foreach ((int p1, int p2, int p3) in FaceVertices)
            {
                yield return Vector3.Cross(vertices[p1] - vertices[p3], vertices[p2] - vertices[p3]).normalized;
            }
        }
    }
}
