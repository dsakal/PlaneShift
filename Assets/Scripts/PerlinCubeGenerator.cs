using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinCubeGenerator : MonoBehaviour
{
    public int gridSize = 10;
    public float scale = 1.0f;
    public float maxHeight = 5.0f;

    private MeshFilter meshFilter;
    private Mesh mesh;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter component not found!");
            return;
        }

        GenerateCube();
    }

    void GenerateCube()
    {
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        Vector3[] vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[gridSize * gridSize * 6];

        for (int x = 0; x <= gridSize; x++)
        {
            for (int z = 0; z <= gridSize; z++)
            {
                float xCoord = (float)x / gridSize * scale;
                float zCoord = (float)z / gridSize * scale;
                float y = Mathf.PerlinNoise(xCoord, zCoord) * maxHeight;

                vertices[x * (gridSize + 1) + z] = new Vector3(x, y, z);
                uv[x * (gridSize + 1) + z] = new Vector2((float)x / gridSize, (float)z / gridSize);

                if (x < gridSize && z < gridSize)
                {
                    int vertexIndex = x * (gridSize + 1) + z;
                    int triangleIndex = x * gridSize + z;

                    triangles[triangleIndex * 6] = vertexIndex;
                    triangles[triangleIndex * 6 + 1] = vertexIndex + 1;
                    triangles[triangleIndex * 6 + 2] = vertexIndex + gridSize + 1;
                    triangles[triangleIndex * 6 + 3] = vertexIndex + 1;
                    triangles[triangleIndex * 6 + 4] = vertexIndex + gridSize + 2;
                    triangles[triangleIndex * 6 + 5] = vertexIndex + gridSize + 1;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}

