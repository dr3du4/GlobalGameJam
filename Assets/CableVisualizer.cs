using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class CableVisualizer : MonoBehaviour
{
    [Header("Cable Visual Settings")]
    public Material cableMaterial;
    public float cableRadius = 0.05f;
    public int radialSegments = 8;
    public int lengthSegments = 50;
    
    private SplineContainer splineContainer;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    
    void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        
        // Add MeshFilter and MeshRenderer if they don't exist
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        // Set default material if none assigned
        if (cableMaterial == null)
        {
            cableMaterial = new Material(Shader.Find("Standard"));
            cableMaterial.color = Color.black;
        }
        
        meshRenderer.material = cableMaterial;
    }
    
    void LateUpdate()
    {
        if (splineContainer != null && splineContainer.Spline != null && splineContainer.Spline.Count > 1)
        {
            GenerateCableMesh();
        }
    }
    
    void GenerateCableMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Cable Mesh";
        
        Spline spline = splineContainer.Spline;
        int totalVertices = (lengthSegments + 1) * radialSegments;
        Vector3[] vertices = new Vector3[totalVertices];
        Vector3[] normals = new Vector3[totalVertices];
        Vector2[] uvs = new Vector2[totalVertices];
        
        // Generate vertices along the spline
        for (int i = 0; i <= lengthSegments; i++)
        {
            float t = i / (float)lengthSegments;
            
            // Get position and tangent on spline (in local space)
            Unity.Mathematics.float3 pos = spline.EvaluatePosition(t);
            Unity.Mathematics.float3 tan = spline.EvaluateTangent(t);
            Vector3 position = new Vector3(pos.x, pos.y, pos.z);
            Vector3 tangent = new Vector3(tan.x, tan.y, tan.z).normalized;
            
            // Get perpendicular vectors
            Vector3 up = Vector3.up;
            if (Vector3.Dot(tangent, up) > 0.99f)
            {
                up = Vector3.right;
            }
            Vector3 right = Vector3.Cross(tangent, up).normalized;
            up = Vector3.Cross(right, tangent).normalized;
            
            // Create circle of vertices around the spline
            for (int j = 0; j < radialSegments; j++)
            {
                float angle = (j / (float)radialSegments) * Mathf.PI * 2f;
                Vector3 offset = (Mathf.Cos(angle) * right + Mathf.Sin(angle) * up) * cableRadius;
                
                int vertexIndex = i * radialSegments + j;
                vertices[vertexIndex] = position + offset; // Already in local space
                normals[vertexIndex] = offset.normalized;
                uvs[vertexIndex] = new Vector2(j / (float)radialSegments, t);
            }
        }
        
        // Generate triangles
        int totalTriangles = lengthSegments * radialSegments * 6;
        int[] triangles = new int[totalTriangles];
        int triangleIndex = 0;
        
        for (int i = 0; i < lengthSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
                int current = i * radialSegments + j;
                int next = i * radialSegments + (j + 1) % radialSegments;
                int currentNext = (i + 1) * radialSegments + j;
                int nextNext = (i + 1) * radialSegments + (j + 1) % radialSegments;
                
                // First triangle
                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = currentNext;
                triangles[triangleIndex++] = next;
                
                // Second triangle
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = currentNext;
                triangles[triangleIndex++] = nextNext;
            }
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        meshFilter.mesh = mesh;
    }
}

