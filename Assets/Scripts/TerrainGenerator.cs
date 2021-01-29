﻿using System.Collections.Generic;
using UnityEngine;
using TriangleNet.Geometry;
using TriangleNet.Meshing;
using TriangleNet.Topology;
using JetBrains.Annotations;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Genral settings")]
    [Range(1, 1000)] public int sizeX;
    [Range(1, 1000)] public int sizeY;
    [Range(1, 1000)] public int skylandRadius;
    [Range(1, 1000)] public int skylandExtraEdgePoints;
    [Range(1, 100)] public int skylandDeclinePrecent;


    [Header("Skyland")]
    public bool isSkyland;

    [Header("Point Distribution")]
    [Range(4, 6000)] public int pointDensity;

    [Header("Height curve")]
    public AnimationCurve heightCurve;
    public AnimationCurve skylandDropCurve;
    [Header("Seed")]
    public float Seed;
    [Header("Terrain Color Gradient")]
    public Gradient colorGradient;

    private Polygon polygon;
    private TriangleNet.Mesh mesh;
    private UnityEngine.Mesh terrainMesh;
    private Color[] colors;
    private MeshFilter filter;
    private MeshRenderer meshRenderer;


    // Start is called before the first frame update
    public void Start()
    {
        Initiate();
    }

    public void Update()
    {

    }
    public void Initiate()
    {
        filter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();


        polygon = new Polygon();
        terrainMesh = new UnityEngine.Mesh();

        //Make random points to the mesh
        if (!isSkyland)
            for (int i = 0; i < pointDensity; i++)
            {
                var x = Random.Range(.0f, sizeX) - (sizeX / 2);
                var y = Random.Range(.0f, sizeY) - (sizeY / 2);
                polygon.Add(new TriangleNet.Geometry.Vertex(x, y));
            }
        else if (isSkyland)
        {
            for (int i = 0; i < pointDensity; i++)
            {
                float offsetFromCenter = (float)skylandRadius * Mathf.Sqrt(Random.Range(.0f, 1));
                float randomAngle = Random.Range(.0f, 1) * 2 * Mathf.PI;
                var x = offsetFromCenter * Mathf.Cos(randomAngle);
                var y = offsetFromCenter * Mathf.Sin(randomAngle);
                polygon.Add(new TriangleNet.Geometry.Vertex(x, y));
            }
            float angle = 0;

            for (int i = 0; i < skylandExtraEdgePoints; i++)
            {
                ;
                angle += 2 * Mathf.PI / skylandExtraEdgePoints;
                var x = (float)skylandRadius * Mathf.Cos(angle);
                var y = (float)skylandRadius * Mathf.Sin(angle);
                polygon.Add(new TriangleNet.Geometry.Vertex(x, y));
            }
        }
        ConstraintOptions constraints = new ConstraintOptions();
        constraints.ConformingDelaunay = true;

        //Put some triangles to the mesh
        mesh = polygon.Triangulate(constraints) as TriangleNet.Mesh;
        Debug.Log("We made some triangles");

        GenerateMesh();
    }

    public void GenerateMesh()
    {

        Debug.Log("Starting generate mesh function");
        List<Vector3> verticies = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        List<float> heights = new List<float>();



        IEnumerator<Triangle> triangleEnum = mesh.Triangles.GetEnumerator();

        for (int i = 0; i < mesh.Triangles.Count; i++)
        {

            if (!triangleEnum.MoveNext())
            {
                break;
            }

            Triangle currentTriangle = triangleEnum.Current;

            //Debug.Log("Going to noise generation loop");
            if (!isSkyland)
            {
                for (int j = 0; j < 3; j++)
                {
                    heights.Add(heightCurve.Evaluate(Noise.GenerateNoise((float)currentTriangle.vertices[j].x, (float)currentTriangle.vertices[j].y, Seed)) * 200);
                }
            }
            else if (isSkyland)
            {
                for (int j = 0; j < 3; j++)
                {
                    float precentDistanceFromTheEdge = 1 - Mathf.Sqrt(Mathf.Pow((float)currentTriangle.vertices[j].x, 2) + Mathf.Pow((float)currentTriangle.vertices[j].y, 2)) / (float)skylandRadius;

                    if (precentDistanceFromTheEdge > (float)skylandDeclinePrecent / 100)
                        heights.Add(heightCurve.Evaluate(Noise.GenerateNoise((float)currentTriangle.vertices[j].x, (float)currentTriangle.vertices[j].y, Seed)) * 200);


                    else if (precentDistanceFromTheEdge < 0.00001)
                    {
                        heights.Add(0);
                    }
                    else
                    {
                        heights.Add(skylandDropCurve.Evaluate(precentDistanceFromTheEdge * 100 / skylandDeclinePrecent) * heightCurve.Evaluate(Noise.GenerateNoise((float)currentTriangle.vertices[j].x, (float)currentTriangle.vertices[j].y, Seed)) * 200);
                        Debug.Log("not zero");
                    }


                }
            }




            //Debug.Log("Heights" + heights[0] + " " +heights[1] + " " +heights[2]);
            Vector3 v0 = new Vector3((float)currentTriangle.vertices[2].x, (float)heights[2], (float)currentTriangle.vertices[2].y);
            Vector3 v1 = new Vector3((float)currentTriangle.vertices[1].x, (float)heights[1], (float)currentTriangle.vertices[1].y);
            Vector3 v2 = new Vector3((float)currentTriangle.vertices[0].x, (float)heights[0], (float)currentTriangle.vertices[0].y);

            heights.Clear();

            triangles.Add(verticies.Count);
            triangles.Add(verticies.Count + 1);
            triangles.Add(verticies.Count + 2);

            verticies.Add(v0);
            verticies.Add(v1);
            verticies.Add(v2);

            var normal = Vector3.Cross(v1 - v0, v2 - v0);

            for (int x = 0; x < 3; x++)
            {
                normals.Add(normal);
                uvs.Add(Vector3.zero);
            }

        }
        //Colors

        List<Color> ColorList = new List<Color>();


        IEnumerator<Triangle> triangleEnum2 = mesh.Triangles.GetEnumerator();
        for (int i = 0; i < mesh.Triangles.Count; i++)
        {
            if (!triangleEnum2.MoveNext())
            {
                break;
            }

            Triangle current = triangleEnum2.Current;

            float avgHeight = 0;

            for (int j = 0; j < 3; j++)
            {
                avgHeight += Noise.GenerateNoise((float)current.vertices[j].x, (float)current.vertices[j].y, Seed) + 0.5f;
            }

            avgHeight /= 3;

            for (int k = 0; k < 3; k++)
            {
                ColorList.Add(colorGradient.Evaluate(avgHeight));
            }
        }


        terrainMesh.vertices = verticies.ToArray();
        terrainMesh.normals = normals.ToArray();
        terrainMesh.triangles = triangles.ToArray();
        terrainMesh.uv = uvs.ToArray();
        terrainMesh.colors = new Color[verticies.Count];
        terrainMesh.colors = ColorList.ToArray();
        //meshRenderer.material.color = Color.green;






        foreach (Vector3 meshVertex in terrainMesh.vertices)
        {
            polygon.Add(new TriangleNet.Geometry.Vertex(meshVertex.x, meshVertex.z));
        }


        filter.mesh = terrainMesh;






    }
}
