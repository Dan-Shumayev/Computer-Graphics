﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class CCMeshData
{
    public List<Vector3> points; // Original mesh points
    public List<Vector4> faces; // Original mesh quad faces
    public List<Vector4> edges; // Original mesh edges
    public List<Vector3> facePoints; // Face points, as described in the Catmull-Clark algorithm
    public List<Vector3> edgePoints; // Edge points, as described in the Catmull-Clark algorithm
    public List<Vector3> newPoints; // New locations of the original mesh points, according to Catmull-Clark
}


public static class CatmullClark
{
    // Returns a QuadMeshData representing the input mesh after one iteration of Catmull-Clark subdivision.
    public static QuadMeshData Subdivide(QuadMeshData quadMeshData)
    {
        // Create and initialize a CCMeshData corresponding to the given QuadMeshData
        CCMeshData meshData = new CCMeshData();
        meshData.points = quadMeshData.vertices;
        meshData.faces = quadMeshData.quads;
        meshData.edges = GetEdges(meshData);
        meshData.facePoints = GetFacePoints(meshData);
        meshData.edgePoints = GetEdgePoints(meshData);
        meshData.newPoints = GetNewPoints(meshData);

        // Combine facePoints, edgePoints and newPoints into a subdivided QuadMeshData

        // Your implementation here...

        return new QuadMeshData();
    }

    // Returns a list of all edges in the mesh defined by given points and faces.
    // Each edge is represented by Vector4(p1, p2, f1, f2)
    // p1, p2 are the edge vertices
    // f1, f2 are faces incident to the edge. If the edge belongs to one face only, f2 is -1
    public static List<Vector4> GetEdges(CCMeshData mesh)
    {
        var edgeFaces = new Dictionary<Edge, Vector4>();

        void AddEdgeAndFace(Edge edge, int faceIndex)
        {
            if (!edgeFaces.ContainsKey(edge))
            {
                edgeFaces.Add(edge, new Vector4(edge.Start, edge.End, faceIndex, -1));
                return;
            }

            Vector4 edgeAndFaces = edgeFaces[edge];

            Debug.Assert((int)edgeAndFaces.w == -1);
            edgeAndFaces.w = faceIndex;

            edgeFaces[edge] = edgeAndFaces;
        }

        for (var faceIndex = 0; faceIndex < mesh.faces.Count; ++faceIndex)
        {
            Vector4 faceVertices = mesh.faces[faceIndex];

            AddEdgeAndFace(new Edge((int)faceVertices[0], (int)faceVertices[1]), faceIndex);
            AddEdgeAndFace(new Edge((int)faceVertices[1], (int)faceVertices[2]), faceIndex);
            AddEdgeAndFace(new Edge((int)faceVertices[2], (int)faceVertices[3]), faceIndex);
            AddEdgeAndFace(new Edge((int)faceVertices[3], (int)faceVertices[0]), faceIndex);
        }

        return edgeFaces.Values.ToList();
    }

    // Returns a list of "face points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetFacePoints(CCMeshData mesh)
    {
        IEnumerable<Vector3> Generate()
        {
            foreach (Vector4 face in mesh.faces)
            {
                Vector3 a = mesh.points[(int)face.x];
                Vector3 b = mesh.points[(int)face.y];
                Vector3 c = mesh.points[(int)face.z];
                Vector3 d = mesh.points[(int)face.w];

                yield return (a + b + c + d) / 4;
            }
        }

        return Generate().ToList();
    }

    // Returns a list of "edge points" for the given CCMeshData, as described in the Catmull-Clark algorithm 
    public static List<Vector3> GetEdgePoints(CCMeshData mesh)
    {
        IEnumerable<Vector3> Generate()
        {
            foreach (Vector4 edge in mesh.edges)
            {
                Vector3 a = mesh.points[(int)edge.x];
                Vector3 b = mesh.points[(int)edge.y];
                Vector3 c = mesh.facePoints[(int)edge.z];

                if ((int)edge.w == -1)
                {
                    yield return (a + b + c) / 3;
                    continue;
                }

                Vector3 d = mesh.facePoints[(int)edge.w];

                yield return (a + b + c + d) / 4;
            }
        }

        return Generate().ToList();
    }

    // Returns a list of new locations of the original points for the given CCMeshData, as described in the CC algorithm 
    public static List<Vector3> GetNewPoints(CCMeshData mesh)
    {
        List<HashSet<int>> vertexEdges = GetVertexEdges(mesh);

        IEnumerable<Vector3> Generate()
        {
            for (var vertexIndex = 0; vertexIndex < vertexEdges.Count; ++vertexIndex)
            {
                int n = vertexEdges[vertexIndex].Count;

                List<Vector3> neighboringFacePoints = vertexEdges[vertexIndex]
                    .SelectMany(edgeIndex => new[] { (int)mesh.edges[edgeIndex].z, (int)mesh.edges[edgeIndex].w })
                    .Where(faceIndex => faceIndex != -1)
                    .Distinct()
                    .Select(faceIndex => mesh.facePoints[faceIndex])
                    .ToList();
                Debug.Assert(neighboringFacePoints.Count == n);

                Vector3 f = neighboringFacePoints.Aggregate((a, b) => a + b) / neighboringFacePoints.Count;

                List<Vector3> neighboringEdgeMidpoints = vertexEdges[vertexIndex]
                    .Select(edgeIndex =>
                        (mesh.points[(int)mesh.edges[edgeIndex].x] + mesh.points[(int)mesh.edges[edgeIndex].y]) / 2)
                    .ToList();
                Debug.Assert(neighboringEdgeMidpoints.Count == n);

                Vector3 r = neighboringEdgeMidpoints.Aggregate((a, b) => a + b) / neighboringEdgeMidpoints.Count;

                yield return (f + 2 * r + (n - 3) * mesh.points[vertexIndex]) / n;
            }
        }

        return Generate().ToList();
    }

    private static List<HashSet<int>> GetVertexEdges(CCMeshData mesh)
    {
        var vertexEdges = new SortedDictionary<int, HashSet<int>>();

        void AddVertexAndEdge(int vertexIndex, int edgeIndex)
        {
            if (!vertexEdges.ContainsKey(vertexIndex))
            {
                vertexEdges.Add(vertexIndex, new HashSet<int>());
            }

            vertexEdges[vertexIndex].Add(edgeIndex);
        }

        for (var edgeIndex = 0; edgeIndex < mesh.edges.Count; ++edgeIndex)
        {
            AddVertexAndEdge((int)mesh.edges[edgeIndex].x, edgeIndex);
            AddVertexAndEdge((int)mesh.edges[edgeIndex].y, edgeIndex);
        }

        return vertexEdges.Values.ToList();
    }
}

internal readonly struct Edge : IEquatable<Edge>
{
    public Edge(int start, int end)
    {
        Start = start;
        End = end;
    }

    public int Start { get; }

    public int End { get; }

    public bool Equals(Edge other)
    {
        return (Start == other.Start && End == other.End) || (Start == other.End && End == other.Start);
    }

    public override bool Equals(object obj)
    {
        return obj is Edge other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Start ^ End;
    }
}
