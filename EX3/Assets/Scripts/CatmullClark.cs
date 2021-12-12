using System;
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

        List<Vector3> allNewPoints =
            meshData.facePoints.Concat(meshData.edgePoints).Concat(meshData.newPoints).ToList();

        var newFaces = new List<Vector4>();

        // Calculates the vertices of a new face in the subdivided mesh,
        // given the index of a face in the original mesh and the indices of two of the face's
        // edges.
        void AddNewFace(int originalFaceIndex, int firstEdgeIndex, int secondEdgeIndex)
        {
            // The face points list is parallel to the faces list.
            int facePointIndex = originalFaceIndex;

            // The edge points list is parallel to the edges list.
            int firstEdgePointIndex = firstEdgeIndex + meshData.facePoints.Count;
            int secondEdgePointIndex = secondEdgeIndex + meshData.facePoints.Count;

            Vector4 firstEdge = meshData.edges[firstEdgeIndex];
            Vector4 secondEdge = meshData.edges[secondEdgeIndex];

            // The original point is the common point of the two given edges
            int originalPointIndex = new[] { (int)firstEdge.x, (int)firstEdge.y }
                .Intersect(new[] { (int)secondEdge.x, (int)secondEdge.y })
                .First();

            int newPointIndex = originalPointIndex + meshData.facePoints.Count + meshData.edgePoints.Count;

            // We have the indices of all points, so now we can construct the face by going clockwise.
            newFaces.Add(new Vector4(newPointIndex, secondEdgePointIndex, facePointIndex, firstEdgePointIndex));
        }

        int[,] faceEdges = GetFaceEdges(meshData);
        for (var faceIndex = 0; faceIndex < faceEdges.GetLength(0); ++faceIndex)
        {
            AddNewFace(faceIndex, faceEdges[faceIndex, 0], faceEdges[faceIndex, 1]);
            AddNewFace(faceIndex, faceEdges[faceIndex, 1], faceEdges[faceIndex, 2]);
            AddNewFace(faceIndex, faceEdges[faceIndex, 2], faceEdges[faceIndex, 3]);
            AddNewFace(faceIndex, faceEdges[faceIndex, 3], faceEdges[faceIndex, 0]);
        }

        return new QuadMeshData(allNewPoints, newFaces);
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

        // The new point derivation algorithm as seen in the TA
        IEnumerable<Vector3> Generate()
        {
            for (var vertexIndex = 0; vertexIndex < vertexEdges.Count; ++vertexIndex)
            {
                int n = vertexEdges[vertexIndex].Count;

                // Find the neighboring face points of a vertex:
                List<Vector3> neighboringFacePoints = vertexEdges[vertexIndex]
                    .SelectMany(edgeIndex => new[] { (int)mesh.edges[edgeIndex].z, (int)mesh.edges[edgeIndex].w })
                    .Where(faceIndex => faceIndex != -1) // 1. Map each incident edge to its neighboring faces
                    .Distinct() // 2. No need to count faces twice
                    .Select(faceIndex => mesh.facePoints[faceIndex]) // 3. Map each face to its face point
                    .ToList();
                Debug.Assert(neighboringFacePoints.Count == n);

                Vector3 f = neighboringFacePoints.Aggregate((a, b) => a + b) / neighboringFacePoints.Count;

                // Find the midpoints of all incident edges
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

    /// <summary>
    /// For each face in <paramref name="mesh"/>, returns the indices in
    /// the <see cref="CCMeshData.edges"/> list of the face's edges.
    /// </summary>
    /// <param name="mesh">Mesh to operate on</param>
    /// <returns>2D array, with the first dimension containing the indices of faces,
    /// and the second dimension containing the edge indices, in the correct order.</returns>
    /// <remarks>The <see cref="CCMeshData.edges"/> array of the input mesh
    /// must be populated prior to calling this function.</remarks>
    private static int[,] GetFaceEdges(CCMeshData mesh)
    {
        var faceEdges = new int[mesh.faces.Count, 4];

        void AddFaceAndEdge(int faceIndex, int edgeIndex)
        {
            // Create a list of the face's edges, as described by the original mesh.
            var edges = new List<Edge>
            {
                new Edge((int)mesh.faces[faceIndex][0], (int)mesh.faces[faceIndex][1]),
                new Edge((int)mesh.faces[faceIndex][1], (int)mesh.faces[faceIndex][2]),
                new Edge((int)mesh.faces[faceIndex][2], (int)mesh.faces[faceIndex][3]),
                new Edge((int)mesh.faces[faceIndex][3], (int)mesh.faces[faceIndex][0]),
            };

            // Find the relative position of the edge we have in the list of the face's edges.
            // All this is necessary because our edge may have its endpoints swapped.
            int relativePosition = edges.IndexOf(new Edge((int)mesh.edges[edgeIndex].x, (int)mesh.edges[edgeIndex].y));

            faceEdges[faceIndex, relativePosition] = edgeIndex;
        }

        for (var edgeIndex = 0; edgeIndex < mesh.edges.Count; ++edgeIndex)
        {
            AddFaceAndEdge((int)mesh.edges[edgeIndex].z, edgeIndex);

            if ((int)mesh.edges[edgeIndex].w != -1)
            {
                AddFaceAndEdge((int)mesh.edges[edgeIndex].w, edgeIndex);
            }
        }

        return faceEdges;
    }

    /// <summary>
    /// For each vertex in <paramref name="mesh"/>, returns the indices in
    /// the <see cref="CCMeshData.edges"/> list of the vertex's incident edges.
    /// </summary>
    /// <param name="mesh">Mesh to operate on</param>
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

/// <summary>
/// Describes an edge in a <see cref="CCMeshData"/> mesh.
/// </summary>
internal readonly struct Edge : IEquatable<Edge>
{
    public Edge(int start, int end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Index inside <see cref="CCMeshData.points"/> of the edge's starting point.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// Index inside <see cref="CCMeshData.points"/> of the edge's ending point.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// Determines whether two edges are the same.
    /// </summary>
    /// <param name="other">The other edge to compare against</param>
    /// <remarks>Two edges are considered equal if they have the same endpoints,
    /// regardless of order.</remarks>
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
