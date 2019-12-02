using System.Collections.Generic;

using UnityEngine;

public class ViewRaycaster : MonoBehaviour
{
    // Editor fields
    public GameObject TilemapGrid;
    public List<Transform> LightBlockingObjects = new List<Transform>();
    public int Layer = 10;
    public LayerMask RaycastLayer;
    public float RayVertexOffset = 0.01f;
    public float EdgePenetration = 0.5f;
    public float MaxRayDistance = 20;
    [Range(1, 4)]
    public int WallRayStep = 2;
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
    private struct VertTransform
    {
        public Transform transform;
        public Vector3[] vertices;
    }

    // Start is called before the first frame update
    private void Start()
    {
        this.mesh = new Mesh();
        this.meshFilter = this.GetComponent<MeshFilter>();
        foreach(Transform transform in this.TilemapGrid.transform)
        {
            if(transform.gameObject.activeSelf == true && transform.gameObject.layer == this.Layer && transform.GetComponent<CompositeCollider2D>() != null)
            {
                this.LightBlockingObjects.Add(transform);
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        // Clear the mesh out
        this.mesh.Clear();
        // Get current world position of this raycaster
        Vector3 pos = this.transform.position;
        // Create a new list for vertices
        List<Vector3> vertList = new List<Vector3>();
        List<VertTransform> vertTransformList = new List<VertTransform>();
        // Get all the vertices we will be raycasting toward
        int NumberOfTotalVertices = 0;
        float x = pos.x - this.MaxRayDistance;
        float y = pos.y - this.MaxRayDistance;
        // Bottom left to bottom right
        while(x < pos.x + this.MaxRayDistance)
        {
            vertList.Add(new Vector3(x, y, pos.z));
            NumberOfTotalVertices++;
            x += this.WallRayStep;
        }
        // Bottom right to top right
        while(y < pos.y + this.MaxRayDistance)
        {
            vertList.Add(new Vector3(x, y, pos.z));
            NumberOfTotalVertices++;
            y += this.WallRayStep;
        }
        // Top right to top left
        while(x > pos.x - this.MaxRayDistance)
        {
            vertList.Add(new Vector3(x, y, pos.z));
            NumberOfTotalVertices++;
            x -= this.WallRayStep;
        }
        // Top left to bottom left
        while(y > pos.y - this.MaxRayDistance)
        {
            vertList.Add(new Vector3(x, y, pos.z));
            NumberOfTotalVertices++;
            y -= this.WallRayStep;
        }
        for(int i = 0; i < this.LightBlockingObjects.Count; i++)
        {
            Vector3[] ObjectVertices = this.LightBlockingObjects[i].GetComponent<CompositeCollider2D>().CreateMesh(true, true).vertices;
            List<Vector3> FilteredObjectVertsList = new List<Vector3>();
            foreach(Vector3 vert in ObjectVertices)
            {
                if(Vector3.Distance(vert, Vector3.zero) < this.MaxRayDistance)
                {
                    FilteredObjectVertsList.Add(vert);
                }
            }
            ObjectVertices = FilteredObjectVertsList.ToArray();
            NumberOfTotalVertices += ObjectVertices.Length;
            vertTransformList.Add(new VertTransform()
            {
                transform = this.LightBlockingObjects[i].transform, vertices = ObjectVertices
            });
        }
        Vector3[] vertArray = vertList.ToArray();
        vertList.Clear();
        VertTransform[] vertTransformArray = vertTransformList.ToArray();
        vertTransformList.Clear();
        this.NumRays = NumberOfTotalVertices * 2;
        // Set up arrays
        AngledVert[] angledverts = new AngledVert[(NumberOfTotalVertices * 2)];
        Vector3[] verts = new Vector3[(NumberOfTotalVertices * 2) + 1];
        Vector2[] uvs = new Vector2[(NumberOfTotalVertices * 2) + 1];
        // Set up first slot of arrays to local position of this raycaster
        verts[0] = this.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);
        uvs[0] = new Vector2(verts[0].x, verts[0].y);
        int vertCount = 0;
        // Do rays for all square ray verts
        for(int i = 0; i < vertArray.Length; i++)
        {
            // Get relative positions including offset
            float YMinus = vertArray[i].y - pos.y - this.RayVertexOffset;
            float YPlus = vertArray[i].y - pos.y + this.RayVertexOffset;
            float XMinus = vertArray[i].x - pos.x - this.RayVertexOffset;
            float XPlus = vertArray[i].x - pos.x + this.RayVertexOffset;
            // Get angles
            float angleMinus = Mathf.Atan2(YMinus, XMinus);
            float anglePlus = Mathf.Atan2(YPlus, XPlus);
            // Cast rays
            RaycastHit2D hitMinus = Physics2D.Raycast(pos, new Vector2(XMinus, YMinus).normalized, this.MaxRayDistance, this.RaycastLayer);
            RaycastHit2D hitPlus = Physics2D.Raycast(pos, new Vector2(XPlus, YPlus).normalized, this.MaxRayDistance, this.RaycastLayer);
            // If rays time out, set them to where they would reach maximum instead of leaving them at zero position
            // Otherwise add an edge penetration distance to ray hit, this lets you see the edge of the wall without seeing through to the other side
            if(hitMinus.distance < 0.01)
            {
                hitMinus.point = new Vector2(pos.x, pos.y) + (new Vector2(XMinus, YMinus).normalized * this.MaxRayDistance);
            }
            else
            {
                hitMinus.point += new Vector2(XMinus, YMinus).normalized * this.EdgePenetration;
            }
            if(hitPlus.distance < 0.01)
            {
                hitPlus.point = new Vector2(pos.x, pos.y) + (new Vector2(XPlus, YPlus).normalized * this.MaxRayDistance);
            }
            else
            {
                hitPlus.point += new Vector2(XMinus, YMinus).normalized * this.EdgePenetration;
            }
            // Draw debug rays on screen
            if(this.DrawDebugRays == true)
            {
                Debug.DrawLine(pos, hitMinus.point, Color.cyan);
                Debug.DrawLine(pos, hitPlus.point, Color.blue);
            }
            // Store first hit data to angledverts array
            angledverts[vertCount * 2].vert = this.transform.worldToLocalMatrix.MultiplyPoint3x4(hitMinus.point);
            angledverts[vertCount * 2].angle = angleMinus;
            angledverts[vertCount * 2].uv = new Vector2(angledverts[vertCount * 2].vert.x, angledverts[vertCount * 2].vert.y);
            // Store second hit data to angledverts array
            angledverts[(vertCount * 2) + 1].vert = this.transform.worldToLocalMatrix.MultiplyPoint3x4(hitPlus.point);
            angledverts[(vertCount * 2) + 1].angle = anglePlus;
            angledverts[(vertCount * 2) + 1].uv = new Vector2(angledverts[(vertCount * 2) + 1].vert.x, angledverts[(vertCount * 2) + 1].vert.y);
            // Increment vertex count
            vertCount++;
        }
        // Do rays for all object verts
        for(int i = 0; i < vertTransformArray.Length; i++)
        {
            // Get vertices for this particular object
            Vector3[] vertices = vertTransformArray[i].vertices;
            // Loop through all vertices in this particular object
            foreach(Vector3 vertexLocalPos in vertices)
            {
                // Get this particular vertex's world position
                Vector2 vertexWorldPos = vertTransformArray[i].transform.localToWorldMatrix.MultiplyPoint3x4(vertexLocalPos);
                // Get relative positions including offset
                float YMinus = vertexWorldPos.y - pos.y - this.RayVertexOffset;
                float YPlus = vertexWorldPos.y - pos.y + this.RayVertexOffset;
                float XMinus = vertexWorldPos.x - pos.x - this.RayVertexOffset;
                float XPlus = vertexWorldPos.x - pos.x + this.RayVertexOffset;
                // Get angles
                float angleMinus = Mathf.Atan2(YMinus, XMinus);
                float anglePlus = Mathf.Atan2(YPlus, XPlus);
                // Cast rays
                RaycastHit2D hitMinus = Physics2D.Raycast(pos, new Vector2(XMinus, YMinus).normalized, this.MaxRayDistance, this.RaycastLayer);
                RaycastHit2D hitPlus = Physics2D.Raycast(pos, new Vector2(XPlus, YPlus).normalized, this.MaxRayDistance, this.RaycastLayer);
                // If rays time out, set them to where they would reach maximum instead of leaving them at zero position
                // Otherwise add an edge penetration distance to ray hit, this lets you see the edge of the wall without seeing through to the other side
                if(hitMinus.distance < 0.01)
                {
                    hitMinus.point = new Vector2(pos.x, pos.y) + (new Vector2(XMinus, YMinus).normalized * this.MaxRayDistance);
                }
                else
                {
                    hitMinus.point += new Vector2(XMinus, YMinus).normalized * this.EdgePenetration;
                }
                if(hitPlus.distance < 0.01)
                {
                    hitPlus.point = new Vector2(pos.x, pos.y) + (new Vector2(XPlus, YPlus).normalized * this.MaxRayDistance);
                }
                else
                {
                    hitPlus.point += new Vector2(XMinus, YMinus).normalized * this.EdgePenetration;
                }
                // Draw debug rays on screen
                if(this.DrawDebugRays == true)
                {
                    Debug.DrawLine(pos, hitMinus.point, Color.red);
                    Debug.DrawLine(pos, hitPlus.point, Color.yellow);
                }
                // Store first hit data to angledverts array
                angledverts[vertCount * 2].vert = this.transform.worldToLocalMatrix.MultiplyPoint3x4(hitMinus.point);
                angledverts[vertCount * 2].angle = angleMinus;
                angledverts[vertCount * 2].uv = new Vector2(angledverts[vertCount * 2].vert.x, angledverts[vertCount * 2].vert.y);
                // Store second hit data to angledverts array
                angledverts[(vertCount * 2) + 1].vert = this.transform.worldToLocalMatrix.MultiplyPoint3x4(hitPlus.point);
                angledverts[(vertCount * 2) + 1].angle = anglePlus;
                angledverts[(vertCount * 2) + 1].uv = new Vector2(angledverts[(vertCount * 2) + 1].vert.x, angledverts[(vertCount * 2) + 1].vert.y);
                // Increment vertex count
                vertCount++;
            }
        }
        // Sort angled vert array
        System.Array.Sort(angledverts, delegate (AngledVert one, AngledVert two)
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
