using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNodeVisualController : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock block;

    private Vector3 s = Vector3.zero;
    private Vector3 e = Vector3.zero;

    private void Awake() {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        block = new MaterialPropertyBlock();
    }

    void Update() {
        Debug.DrawLine(s, e, Color.red);
    }

    public void GeneratePathQuad(Vector3 start, Vector3 end, float thickness){

        // Generate a quad for the mesh
        Vector3[] vertices = new Vector3[4];

        Vector3 xDir = end - start;

        Vector3 normal      = Vector3.back; // Direction towards camera
        //Debug.DrawLine(transform.position, transform.position + normal, Color.blue);

        Vector3 yDir        = Vector3.Cross(normal, xDir);
        Vector3 yDirNorm    = yDir.normalized;

        // Generate Quad vertex positions
        {
            vertices[0] = -xDir - yDirNorm * thickness * 0.5f;
            vertices[1] =  xDir - yDirNorm * thickness * 0.5f;
            vertices[2] = -xDir + yDirNorm * thickness * 0.5f;
            vertices[3] =  xDir + yDirNorm * thickness * 0.5f;

        }

        int[] triangles = new int[6];

        // Generate triangles
        {
            // 1st triangle
            triangles[0] = 0;
            triangles[1] = 2;
            triangles[2] = 3;

            // 2nd triangle 
            triangles[3] = 0;
            triangles[4] = 3;
            triangles[5] = 1;
        }

        Vector2[] UVs = new Vector2[4];

        // Generate UV coordinates
        {
            UVs[0] = new Vector2(0, 0);
            UVs[1] = new Vector2(1, 0);
            UVs[2] = new Vector2(0, 1);
            UVs[3] = new Vector2(1, 1);
        }

        Mesh outputMesh = new Mesh();
        outputMesh.vertices  = vertices;
        outputMesh.triangles = triangles;
        outputMesh.uv = UVs;
        outputMesh.RecalculateNormals();

        meshFilter.mesh = outputMesh;

        //meshRenderer.GetPropertyBlock(block);  // Get previous properties
        block.SetFloat("_PathLength", xDir.magnitude); // Overwrite this specific property
        meshRenderer.SetPropertyBlock(block);  // Overwrite properties

        transform.position = 0.5f*(start + end);
    }
}
