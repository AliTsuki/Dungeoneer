using System.Collections.Generic;

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
    public int NumRays = 0;
    // Private fields
    private Mesh mesh;
    private MeshFilter meshFilter;

    // Angled vertex struct
    private struct AngledVert
    {
        public Vector3 vert;
        public float angle;
        public Vector2 uv;
    }

    // Struct containing a parent transform and all the vertices contained within the parent object
    private struct VertPlusParentTransform
    {
        public Transform transform;
        public Vector3[] vertices;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        //sceneObjects = FindGameObjectsWithLayer(layerMask.value);
        this.mesh = new Mesh();
        this.meshFilter = this.GetComponent<MeshFilter>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Clear the mesh out
        this.mesh.Clear();
        // Create a new list for vertices and their parent transforms
        List<VertPlusParentTransform> VertsPlusParentTransforms = new List<VertPlusParentTransform>();
        // Get all the vertices we will be raycasting toward
        int NumberOfTotalVertices = 0;
        for(int i = 0; i < this.LightBlockingObjects.Length; i++)
        {
            Vector3[] ObjectVertices = this.LightBlockingObjects[i].GetComponent<CompositeCollider2D>().CreateMesh(true, true).vertices;
            NumberOfTotalVertices += ObjectVertices.Length;
            VertsPlusParentTransforms.Add(new VertPlusParentTransform(){ transform = this.LightBlockingObjects[i].transform, vertices = ObjectVertices });
        }
        VertPlusParentTransform[] VertsPlusParentTransformsArray = VertsPlusParentTransforms.ToArray();
        this.NumRays = NumberOfTotalVertices * 2;
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
                float YMinus = vertexWorldPos.y - pos.y - this.RayVertexOffset;
                float YPlus = vertexWorldPos.y - pos.y + this.RayVertexOffset;
                float XMinus = vertexWorldPos.x - pos.x - this.RayVertexOffset;
                float XPlus = vertexWorldPos.x - pos.x + this.RayVertexOffset;
                // Get angles
                float angle1 = Mathf.Atan2(YMinus, XMinus);
                float angle2 = Mathf.Atan2(YPlus, XPlus);
                // Cast rays
                RaycastHit2D hitMinus = Physics2D.Raycast(pos, new Vector2(XMinus, YMinus), this.MaxRayDistance, this.Layer);
                RaycastHit2D hitPlus = Physics2D.Raycast(pos, new Vector2(XPlus, YPlus), this.MaxRayDistance, this.Layer);
                // If rays time out, set them to where they would reach maximum instead of leaving them at zero position
                // Otherwise add an edge penetration distance to ray hit, this lets you see the edge of the wall without seeing through to the other side
                if(hitMinus.point == Vector2.zero)
                {
                    hitMinus.point = new Vector2(XMinus, YMinus).normalized * this.MaxRayDistance;
                }
                else
                {
                    hitMinus.point += new Vector2(XMinus, YMinus).normalized * this.EdgePenetration;
                }
                if(hitPlus.point == Vector2.zero)
                {
                    hitPlus.point = new Vector2(XPlus, YPlus).normalized * this.MaxRayDistance;
                }
                else
                {
                    hitPlus.point += new Vector2(XMinus, YMinus).normalized * this.EdgePenetration;
                }
                // Draw debug rays on screen
                if(this.DrawDebugRays == true)
                {
                    Debug.DrawLine(pos, hitMinus.point, Color.red);
                    Debug.DrawLine(pos, hitPlus.point, Color.green);
                }
                // Store first hit data to angledverts array
                angledverts[vertCount * 2].vert = this.transform.worldToLocalMatrix.MultiplyPoint3x4(hitMinus.point);
                angledverts[vertCount * 2].angle = angle1;
                angledverts[vertCount * 2].uv = new Vector2(angledverts[vertCount * 2].vert.x, angledverts[vertCount * 2].vert.y);
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
        this.mesh.vertices = verts;
        this.mesh.uv = uvs;
        this.mesh.triangles = tris;
        this.meshFilter.mesh = this.mesh;
    }
}
