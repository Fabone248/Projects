using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DivideMeshScript : MonoBehaviour {

    public Material shrapnel;

    Vector3[] mainVertices;
   // List<shape> shapesList = new List<shape>();

	// Use this for initialization
	void Start () {
        mainVertices = GetComponent<MeshFilter>().mesh.vertices;

        for (int i = 0; i < mainVertices.Length; ++i)
        {
            //Debug.Log(mainVertices[i]);
        }

        Vector3 size = new Vector3(
            GetComponent<MeshFilter>().mesh.bounds.extents.x * transform.lossyScale.x,
            GetComponent<MeshFilter>().mesh.bounds.extents.y * transform.lossyScale.y,
            GetComponent<MeshFilter>().mesh.bounds.extents.z * transform.lossyScale.z
            );
        Vector3 minBound = new Vector3(
            transform.position.x - size.x,
            transform.position.y - size.y,
            transform.position.z - size.z);

        Vector3 maxBound = new Vector3(
            transform.position.x + size.x,
            transform.position.y + size.y,
            transform.position.z + size.z);

        List<Vector3> bounds = new List<Vector3>();

        Vector3 point0 = new Vector3(minBound.x, maxBound.y, minBound.z);
        Vector3 point1 = new Vector3(Mathf.Lerp(minBound.x, maxBound.x, Random.Range(0f, 1f)), maxBound.y, minBound.z);
        Vector3 point2 = new Vector3(point1.x, Mathf.Lerp(minBound.y, maxBound.y, Random.Range(0f, 1f)), minBound.z);
        Vector3 point3 = new Vector3(point0.x, point2.y, minBound.z);
        
        Vector3 point4 = new Vector3(point0.x, point0.y, Mathf.Lerp(minBound.z, maxBound.z, Random.Range(0f, 1f)));
        Vector3 point5 = new Vector3(point1.x, point1.y, point4.z);
        Vector3 point6 = new Vector3(point2.x, point2.y, point4.z);
        Vector3 point7 = new Vector3(point3.x, point3.y, point4.z);
        
        int[] triangles = new int[36];
        Vector2[] uvs = new Vector2[24];

        // Front
        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;
        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        uvs[0] = new Vector2(0,1);
        uvs[1] = new Vector2(1,1);
        uvs[2] = new Vector2(1,0);
        uvs[3] = new Vector2(0,0);

        uvs[4] = new Vector2(0, 1);
        uvs[5] = new Vector2(1, 1);
        uvs[6] = new Vector2(1, 0);
        uvs[7] = new Vector2(0, 0);
        
        // Top
        triangles[6] = 8;
        triangles[7] = 4;
        triangles[8] = 5;
        triangles[9] = 8;
        triangles[10] = 5;
        triangles[11] = 9;

        //uvs[4] = new Vector2(point4.x, point4.z);
        //uvs[5] = new Vector2(point5.x, point5.z);
        //uvs[6] = new Vector2(point1.x, point1.z);
        //uvs[7] = new Vector2(point0.x, point0.z);

        // Right
        triangles[12] = 17;
        triangles[13] = 13;
        triangles[14] = 6;
        triangles[15] = 17;
        triangles[16] = 6;
        triangles[17] = 10;

        //uvs[8] = new Vector2(point1.x, point1.z);
        //uvs[9] = new Vector2(point5.x, point5.z);
        //uvs[10] = new Vector2(point6.x, point6.z);
        //uvs[11] = new Vector2(point2.x, point2.z);

        // Bottom
        triangles[18] = 18;
        triangles[19] = 14;
        triangles[20] = 7;
        triangles[21] = 18;
        triangles[22] = 7;
        triangles[23] = 11;

        //uvs[12] = new Vector2(point3.x, point3.z);
        //uvs[13] = new Vector2(point2.x, point2.z);
        //uvs[14] = new Vector2(point6.x, point6.z);
        //uvs[15] = new Vector2(point7.x, point7.z);

        // Left
        triangles[24] = 19;
        triangles[25] = 12;
        triangles[26] = 16;
        triangles[27] = 19;
        triangles[28] = 23;
        triangles[29] = 12;

        //uvs[16] = new Vector2(point4.x, point4.z);
        //uvs[17] = new Vector2(point0.x, point0.z);
        //uvs[18] = new Vector2(point3.x, point3.z);
        //uvs[19] = new Vector2(point7.x, point7.z);

        // Rear
        triangles[30] = 20;
        triangles[31] = 23;
        triangles[32] = 22;
        triangles[33] = 20;
        triangles[34] = 22;
        triangles[35] = 21;

        //uvs[0] = new Vector2(point5.x, point5.z);
        //uvs[1] = new Vector2(point4.x, point4.z);
        //uvs[2] = new Vector2(point7.x, point7.z);
        //uvs[3] = new Vector2(point6.x, point6.z);

        for (int i = 0; i < 3; ++i)
        {
            bounds.Add(point0);
            bounds.Add(point1);
            bounds.Add(point2);
            bounds.Add(point3);
            bounds.Add(point4);
            bounds.Add(point5);
            bounds.Add(point6);
            bounds.Add(point7);
        }

        GameObject cube = new GameObject();
        cube.transform.position = getCenterOfGravity(bounds);

        for (int i = 0; i < bounds.Count; ++i)
        {
            bounds[i] = new Vector3(bounds[i].x - cube.transform.position.x, bounds[i].y - cube.transform.position.y, bounds[i].z - cube.transform.position.z);
        }

        //Destroy(cube.GetComponent<BoxCollider>());

        cube.AddComponent<MeshRenderer>();
        cube.AddComponent<MeshFilter>();
        cube.AddComponent<MeshCollider>();
        
        cube.GetComponent<Renderer>().material = shrapnel;

        
        Mesh colliderMesh = new Mesh();
        colliderMesh.vertices = bounds.ToArray();
        colliderMesh.triangles = triangles;
        colliderMesh.uv = uvs;
        colliderMesh.RecalculateNormals();

        cube.GetComponent<MeshCollider>().sharedMesh = colliderMesh;
	    cube.GetComponent<MeshFilter>().mesh = colliderMesh;

        GameObject rotator = new GameObject();
        rotator.transform.position = transform.position;
        cube.transform.parent = rotator.transform;
        rotator.transform.rotation = transform.rotation;
        cube.transform.parent = null;
        Destroy(rotator);
        //Debug.Log(cube.GetComponent<MeshFilter>().mesh.bounds.center);
        
    }

    Vector3 getCenterOfGravity(List<Vector3> bounds)
    {

        Vector3 centerOfGravity = new Vector3(0f, 0f, 0f);
        Vector3 maxBound = bounds[0];
        Vector3 minBound = bounds[0];

        for (int i = 1; i < bounds.Count; ++i)
        {
            maxBound = Vector3.Max(bounds[i], maxBound);
            minBound = Vector3.Min(bounds[i], minBound);
        }

        centerOfGravity.x = ((maxBound.x - minBound.x) / 2) + minBound.x;
        centerOfGravity.y = ((maxBound.y - minBound.y) / 2) + minBound.y;
        centerOfGravity.z = ((maxBound.z - minBound.z) / 2) + minBound.z;

        return centerOfGravity;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void CreateNewShape(List<Vector3> bounds)
    {
        int[] indexes = new int[bounds.Count*3];

        for (int i = 0; i < indexes.Length; ++i)
        { 
            //indexes[i] = 
        }
    }

    /*
    class shape
    {
        Vector3[] vertices;
        int[] indexes;

        public shape(Vector3[] _vertices, int[] _indexes)
        {
            vertices = _vertices;
            indexes = _indexes;
        }
    }
    */
}
