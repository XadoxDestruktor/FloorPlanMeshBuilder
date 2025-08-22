using NUnit.Framework.Internal;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.LightAnchor;

/// <summary>
/// Utility class to generate a simple building. It is given the floor plan of the building and creates a prism mesh by extruding the floor plan and generating the faces.
/// </summary>
public static class FloorPlanMeshGenerator
{
    /// <summary>
    /// Generates a builing mesh according to the given floor plan corners and assigns it to a game object.
    /// </summary>
    /// <param name="floorPlanCorners">Corners of the floor plan in 3D space</param>
    /// <param name="height">Height of the generated building</param>
    /// <returns>A reference to the created GameObject</returns>
    public static GameObject GenerateBuilding(List<Vector3> floorPlanCorners, float height, Material material = null)
    {
        var displacedFloorPlanCorners = CalculateDisplacedVertices(floorPlanCorners, height);

        var wallTriangles = CalculatePrismSideFaces(floorPlanCorners.Count);

        // possible next steps: implement ear clipping algorithm to triangulate concave roof and floor
        // if (hasRoof) roofTriangles = CalculateRoofTriangles(roofHeight);
        // if (hasFloor) floorTriangles = CalculateFloorTriangles();

        Mesh mesh;

        { 
            var vertices = new List<Vector3>();
            vertices.AddRange(floorPlanCorners);
            vertices.AddRange(displacedFloorPlanCorners);

            var triangles = new List<int>();
            triangles.AddRange(wallTriangles);

            mesh = GenerateMesh(vertices, triangles);
        }

        return GenerateGameObject(mesh, material);
    }

    /// <summary>
    /// Calculates row of vertices displaced by the given height in the direction of Vector3.up
    /// </summary>
    /// <param name="vertices">Vertices to be displaced</param>
    /// <param name="height">Height of displacement</param>
    /// <returns>A list of displaced vertices</returns>
    public static List<Vector3> CalculateDisplacedVertices(List<Vector3> vertices, float height)
    {
        return CalculateDisplacedVertices(vertices, height, Vector3.up);
    }

    /// <summary>
    /// Calculates row of vertices displaced by the given height in the given direction
    /// </summary>
    /// <param name="vertices">Vertices to be displaced</param>
    /// <param name="height">Height of displacement</param>
    /// <param name="upDirection">Direction of displacement</param>
    /// <returns>A list of displaced vertices</returns>
    public static List<Vector3> CalculateDisplacedVertices(List<Vector3> vertices, float height, Vector3 upDirection)
    {
        List<Vector3> displacedVertices = new List<Vector3>();

        foreach (var vertex in vertices)
        {
            displacedVertices.Add(vertex + upDirection * height);
        }

        return displacedVertices;
    }

    /// <summary>
    /// Calculates the indices of the triangles.
    /// Assumes that the lower vertices are added before the upper (displaced) vertices, otherwise the faces are inverted.
    /// Thus, the prism / building is built from ground up.
    /// </summary>
    /// <param name="lowerVerticesCount">Amount of floor plan corners</param>
    /// <param name="triangles">Output list of triangles</param>
    /// <returns>List of triangles</returns>
    public static List<int> CalculatePrismSideFaces(int lowerVerticesCount)
    {
        var triangles = new List<int>();

        for (int i = 0; i < lowerVerticesCount; i++)
        {
            int nextElement = (i == lowerVerticesCount - 1) ? 0 : i + 1;

            triangles.AddRange(new int[]{
                i, nextElement, i + lowerVerticesCount,
                i + lowerVerticesCount, nextElement, nextElement + lowerVerticesCount
            });
        }

        return triangles;
    }

    /// <summary>
    /// Calculates the normal for a triangle with given coordinates. Vertices have to be oriented counter clockwise.
    /// </summary>
    /// <param name="pointA">Point A</param>
    /// <param name="pointB">Point B</param>
    /// <param name="pointC">Point C</param>
    /// <returns>Normal of the face</returns>
    public static Vector3 GetNormal(Vector3 pointA, Vector3 pointB, Vector3 pointC)
    {
        var AB = pointB - pointA;
        var AC = pointC - pointA;
        return Vector3.Cross(AB, AC);
    }

    /// <summary>
    /// Generate a 3D mesh to be assigned to a GameObject
    /// </summary>
    /// <param name="vertices">Vertices of the mesh</param>
    /// <param name="triangles">Triangles of the mesh</param>
    /// <returns>The generated mesh object</returns>
    public static Mesh GenerateMesh(List<Vector3> vertices, List<int> triangles)
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();

        // Recalculate normals for lighting
        mesh.RecalculateNormals();

        return mesh;
    }

    /// <summary>
    /// Assigns the given mesh to a new GameObject
    /// </summary>
    /// <param name="mesh">Mesh to be assigned to the new GameObject</param>
    /// <param name="name">Name to be assigned to the new GameObject</param>
    /// <returns>A new GameObject with the given mesh</returns>
    public static GameObject GenerateGameObject(Mesh mesh, Material material = null, string name = "Building")
    {
        var meshObject = new GameObject("Building");

        MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

        meshFilter.mesh = mesh;

        if (material != null)
            meshRenderer.material = material;

        return meshObject;
    }
}