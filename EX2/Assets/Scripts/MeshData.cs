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

    public MeshData()
    {
        vertices = new List<Vector3>();
        triangles = new List<int>();
    }

    /// <summary>
    /// Returns a Unity Mesh of this MeshData that can be rendered.
    /// </summary>
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

    /// <summary>
    /// Calculates surface normals for each vertex, according to face orientation
    /// </summary>
    public void CalculateNormals()
    {
        Vector3[] surfaceNormals = SurfaceNormals.ToArray();

        // For each vertex, get the normals of all the surfaces it is contained in, then sum 'em up and normalize.
        Vector3[] newNormals = VertexFaces.Select(faces =>
            faces.Select(faceIndex => surfaceNormals[faceIndex]).Aggregate((a, b) => a + b).normalized).ToArray();
        Debug.Assert(newNormals.Length == vertices.Count);

        normals = newNormals;
    }

    /// <summary>
    /// Edits mesh such that each face has a unique set of 3 vertices
    /// </summary>
    public void MakeFlatShaded()
    {
        // Make it so that each face has a unique set of 3 vertices by duplicating
        // each original vertex for each face it is a part of.

        var newVertices = new List<Vector3>(triangles.Count * 3);
        var newTriangles = new List<int>(triangles.Count);
        foreach ((int p1, int p2, int p3) in FaceVertices)
        {
            newVertices.Add(vertices[p1]);
            newVertices.Add(vertices[p2]);
            newVertices.Add(vertices[p3]);

            newTriangles.Add(newVertices.Count - 3);
            newTriangles.Add(newVertices.Count - 2);
            newTriangles.Add(newVertices.Count - 1);
        }

        vertices = newVertices;
        triangles = newTriangles;
    }

    /// <summary>
    /// Returns a 3-tuple of vertex indices (in the <see cref="vertices"/> list) for each triangle in the mesh.
    /// </summary>
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

    /// <summary>
    /// Returns all faces that every vertex in the mesh belongs to.
    /// </summary>
    private IEnumerable<List<int>> VertexFaces
    {
        get
        {
            var facesPerVertex = new SortedDictionary<int, List<int>>();
            foreach (var item in FaceVertices.Select((faceVertices, face) => new { face, faceVertices }))
            {
                foreach (int vertex in new[]
                    { item.faceVertices.Item1, item.faceVertices.Item2, item.faceVertices.Item3 })
                {
                    if (!facesPerVertex.ContainsKey(vertex))
                    {
                        facesPerVertex[vertex] = new List<int>();
                    }

                    facesPerVertex[vertex].Add(item.face);
                }
            }

            return facesPerVertex.Values;
        }
    }

    /// <summary>
    /// Calculates the surface normal for each triangle in the mesh.
    /// </summary>
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
