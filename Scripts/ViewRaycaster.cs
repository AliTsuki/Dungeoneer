﻿using System.Collections.Generic;

using UnityEngine;

public class ViewRaycaster : MonoBehaviour
{
    // Editor fields
    public GameObject[] LightBlockingObjects;
    public LayerMask Layer;
    public float RayVertexOffset = 0.01f;
    public float EdgePenetration = 0.4f;
    public float MaxRayDistance = 20;
    public bool DrawDebugRays = true;
    // Private fields
    private Mesh mesh;
    private MeshFilter meshFilter;

    // Angled vertex struct
    public struct AngledVert
    {
        public Vector3 vert;
        public float angle;
        public Vector2 uv;
    }

    // Struct containing a parent transform and all the vertices contained within the parent object
    public struct VertPlusParentTransform
    {
        public Transform transform;
        public Vector3[] vertices;
    }
    

    // Start is called before the first frame update
    public void Start()
    {
        //sceneObjects = FindGameObjectsWithLayer(layerMask.value);
        mesh = new Mesh();
        meshFilter = this.GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    public void Update()
    {
        // Clear the mesh out
        mesh.Clear();
        // Create a new list for vertices and their parent transforms
        List<VertPlusParentTransform> VertsPlusParentTransforms = new List<VertPlusParentTransform>();
        // Get all the vertices we will be raycasting toward
        int NumberOfTotalVertices = 0;
        for(int i = 0; i < LightBlockingObjects.Length; i++)
        {
            Vector3[] ObjectVertices = LightBlockingObjects[i].GetComponent<CompositeCollider2D>().CreateMesh(true, true).vertices;
            NumberOfTotalVertices += ObjectVertices.Length;
            VertsPlusParentTransforms.Add(new VertPlusParentTransform(){ transform = LightBlockingObjects[i].transform, vertices = ObjectVertices});
        }
        VertPlusParentTransform[] VertsPlusParentTransformsArray = VertsPlusParentTransforms.ToArray();
        // Set up arrays
        AngledVert[] angledverts = new AngledVert[(NumberOfTotalVertices * 2)];
        Vector3[] verts = new Vector3[(NumberOfTotalVertices * 2) + 1];
        Vector2[] uvs = new Vector2[(NumberOfTotalVertices * 2) + 1];
        // Get current world position of this raycaster
        Vector3 pos = this.transform.position;
        // Set up first slot of arrays to local position of this raycaster
        verts[0] = this.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
        uvs[0] = new Vector2(verts[0].x, verts[0].y);

        int vertCount = 0;
        
        // Loop through all objects containing verts of meshes we wish to raycast against
        for(int i = 0; i < VertsPlusParentTransformsArray.Length; i++)
        {
            // Get vertices for this particular object
            Vector3[] vertices = VertsPlusParentTransformsArray[i].vertices;
            // Loop through all vertices in this particular object
            foreach(Vector3 vertexLocalPos in vertices)
            {
                // Get this particular vertex's world position
                Vector2 vertexWorldPos = VertsPlusParentTransformsArray[i].transform.localToWorldMatrix.MultiplyPoint3x4(vertexLocalPos);
                // Get relative positions including offset
                float YMinus = vertexWorldPos.y - pos.y - RayVertexOffset;
                float YPlus = vertexWorldPos.y - pos.y + RayVertexOffset;
                float XMinus = vertexWorldPos.x - pos.x - RayVertexOffset;
                float XPlus = vertexWorldPos.x - pos.x + RayVertexOffset;
                // Get angles
                float angle1 = Mathf.Atan2((YMinus), (XMinus));
                float angle2 = Mathf.Atan2((YPlus), (XPlus));
                // Cast rays
                RaycastHit2D hitMinus = Physics2D.Raycast(pos, new Vector2(XMinus, YMinus), MaxRayDistance, Layer);
                RaycastHit2D hitPlus = Physics2D.Raycast(pos, new Vector2(XPlus, YPlus), MaxRayDistance, Layer);
                // Get rays that time out and reset them to last position instead of zero position, or make rays penetrate walls some distance
                if(hitMinus.point == Vector2.zero)
                {
                    hitMinus.point = new Vector2(XMinus, YMinus).normalized * MaxRayDistance;
                }
                else
                {
                    hitMinus.point += new Vector2(XMinus, YMinus).normalized * EdgePenetration;
                }
                if(hitPlus.point == Vector2.zero)
                {
                    hitPlus.point = new Vector2(XPlus, YPlus).normalized * MaxRayDistance;
                }
                else
                {
                    hitPlus.point += new Vector2(XMinus, YMinus).normalized * EdgePenetration;
                }
                // Draw debug rays on screen
                if(DrawDebugRays == true)
                {
                    Debug.DrawLine(pos, hitMinus.point, Color.red);
                    Debug.DrawLine(pos, hitPlus.point, Color.green);
                }
                // Store first hit data to angledverts array
                angledverts[(vertCount * 2)].vert = this.transform.worldToLocalMatrix.MultiplyPoint3x4(hitMinus.point);
                angledverts[(vertCount * 2)].angle = angle1;
                angledverts[(vertCount * 2)].uv = new Vector2(angledverts[(vertCount * 2)].vert.x, angledverts[(vertCount * 2)].vert.y);
                // Store second hit data to angledverts array
                angledverts[(vertCount * 2) + 1].vert = this.transform.worldToLocalMatrix.MultiplyPoint3x4(hitPlus.point);
                angledverts[(vertCount * 2) + 1].angle = angle2;
                angledverts[(vertCount * 2) + 1].uv = new Vector2(angledverts[(vertCount * 2) + 1].vert.x, angledverts[(vertCount * 2) + 1].vert.y);
                // Increment vertex count
                vertCount++;
            }
        }
        // Sort angled vert array
        System.Array.Sort(angledverts, delegate(AngledVert one, AngledVert two)
        {
            return one.angle.CompareTo(two.angle);
        });
        // Get verts and uvs for mesh
        for(int i = 0; i < angledverts.Length; i++)
        {
            verts[i + 1] = angledverts[i].vert;
            uvs[i + 1] = angledverts[i].uv;
        }
        // Fix uv offsets
        for(int i = 0; i < uvs.Length; i++)
        {
            uvs[i] = new Vector2(uvs[i].x + 0.5f, uvs[i].y + 0.5f);
        }
        // Get triangles for mesh
        List<int> triangleList = new List<int>() { 0, 1, verts.Length - 1 };
        for(int i = verts.Length - 1; i > 0; i--)
        {
            triangleList.Add(0);
            triangleList.Add(i);
            triangleList.Add(i - 1);
        }
        int[] tris = triangleList.ToArray();
        // Assign verts, uvs, and triangles to mesh
        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        meshFilter.mesh = mesh;
    }
}