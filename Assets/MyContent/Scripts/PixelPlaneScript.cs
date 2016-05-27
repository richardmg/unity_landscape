using UnityEngine;
using System.Collections;

public class PixelPlaneScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Mesh mesh = createMeshCube();	
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
//		meshRenderer.material.shader = Shader.Find("Custom/CutoffM");
		meshRenderer.material = (Material)Resources.Load("Custom/CutoffM");
	}

	Mesh createMeshCube()
	{
		Vector3[] v = new Vector3[4];
		v[0].x = -10; v[0].y = 0;  v[0].z = 0;
		v[1].x = 10;  v[1].y = 0;  v[1].z = 0;
		v[2].x = -10; v[2].y = 50; v[2].z = 0;
		v[3].x = 10;  v[3].y = 50; v[3].z = 0;

		Vector2[] uv = new Vector2[4];
		uv[0].x = 0; uv[0].y = 0;
		uv[1].x = 1; uv[1].y = 0;
		uv[2].x = 0; uv[2].y = 1;
		uv[3].x = 1; uv[3].y = 1;

		int[] tri = new int[6];
		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 2;
		tri[3] = 1;
		tri[4] = 3;
		tri[5] = 2;

		Mesh mesh = new Mesh();
//		mesh.Clear();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;
		mesh.RecalculateNormals();

		return mesh;
	}
}
