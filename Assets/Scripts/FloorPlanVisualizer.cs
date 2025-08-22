using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

/// <summary>
/// Visualizes a given floor plan by extruding it and generating a mesh.
/// The corners of the floor plan are provided via the transforms of empties so that they can be seen in the editor before generating (remember to turn on the visibility of gizmos).
/// For convinience, a circular floor plan with the set number of vertices can be generated in a given radius around the transform of this object. Remember to generate the mesh after. 
/// </summary>
public class FloorPlanVisualizer : MonoBehaviour
{

    // variables to generate the building
    public bool generate = true;
    [Range(1f, 30f)] public float height = 10f;
    public List<GameObject> floorPlanCorners;
    public Material material;

    [Space(25)]

    // variables to generate corners for testing
    public bool generateCorners = false;
    [Range(3, 30)] public int generateCornerAmount = 5;
    [Range(1f, 10f)] public float radius = 10f;
    private GameObject cornerMarkerPrefab;

    private GameObject lastMeshObject = null;
    

    void Start()
    {
        cornerMarkerPrefab = Resources.Load<GameObject>("Prefabs/CornerMarkerPrefab");

        if (cornerMarkerPrefab == null) {
            Debug.LogError("Corner marker prefab not found");
        }
    }

    void Update()
    {
        if (generateCorners)
        {
            generateCorners = false;
            GenerateCorners();
        }

        if (generate)
        {
            generate = false;
            GenerateBuilding();
        }
    }

    /// <summary>
    /// Generates the building mesh and assigns it to the building GameObject.
    /// </summary>
    public void GenerateBuilding()
    {
        if (lastMeshObject != null)
            Destroy(lastMeshObject);

        // convert the transforms into a list of 3d coordinates
        List<Vector3> vertices = new List<Vector3>();
        foreach (var corner in floorPlanCorners)
        {
            vertices.Add(corner.transform.position);
        }

        lastMeshObject = FloorPlanMeshGenerator.GenerateBuilding(vertices, height, material);
        lastMeshObject.transform.SetParent(transform, true);
    }

    /// <summary>
    /// Generates the specified amount of corners in a circular placement around the center. Removes previous corners.
    /// </summary>
    public void GenerateCorners()
    {
        // remove existing corners
        foreach (var corner in floorPlanCorners)
        {
            Destroy(corner);
        }
        floorPlanCorners.Clear();

        // rotate a vector pointing forward with the length of the radius
        Vector3 point = radius * Vector3.forward;

        for (int i = 0; i < generateCornerAmount; i++)
        {
            float angle = i / (float)generateCornerAmount * 360f;
            
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
            
            var newCorner = Instantiate(cornerMarkerPrefab, rotation * point, Quaternion.identity);
            newCorner.transform.SetParent(transform, true);

            floorPlanCorners.Add(newCorner);
        }
        
    }
}
