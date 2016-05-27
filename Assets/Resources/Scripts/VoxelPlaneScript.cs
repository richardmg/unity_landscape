using UnityEngine;
using System.Collections;

public class VoxelPlaneScript : MonoBehaviour {

	void Start () {
		Mesh mesh0 = createVoxelMesh(10, 1, 1, 1);	
		Mesh mesh1 = createVoxelMesh(1, 1, 1, 1);	

		Matrix4x4 transform0 = new Matrix4x4();
		transform0.SetTRS(new Vector3(0, 1, 0), Quaternion.identity, new Vector3(1, 1, 1));

		Matrix4x4 transform1 = new Matrix4x4();
		transform1.SetTRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

		CombineInstance[] ci = new CombineInstance[2];
		ci[0].mesh = mesh0;
		ci[0].transform = transform0;
		ci[1].mesh = mesh1;
		ci[1].transform = transform1;

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ci, true, true);

		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = (Material)Resources.Load("Materials/CutoffM");
		// CHANGE MATERIAL TO NOT BE TWO-SIDED
	}

	Mesh createVoxelMesh(int voxelCount, float voxelWidth, float voxelHeight, float voxelDepth)
	{
		float w = voxelCount * voxelWidth;
		float h = voxelHeight;

		Vector3[] v = new Vector3[8];
		Vector2[] uv = new Vector2[8];
		int[] tri = new int[12];

		// Front vertices
		v[0].x = 0; v[0].y = 0; v[0].z = 0;
		v[1].x = w; v[1].y = 0; v[1].z = 0;
		v[2].x = 0; v[2].y = h; v[2].z = 0;
		v[3].x = w; v[3].y = h; v[3].z = 0;

		// Back vertices
		for (int i = 4; i < 8; ++i) {
			v[i] = v[i - 4];
			v[i].z = voxelDepth;
		}

		// Front texture coords
		uv[0].x = 0; uv[0].y = 0;
		uv[1].x = 1; uv[1].y = 0;
		uv[2].x = 0; uv[2].y = 1;
		uv[3].x = 1; uv[3].y = 1;

		// Back texture coords
		for (int i = 4; i < 8; ++i)
			uv[i] = uv[i - 4];

		// Front triangles
		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 2;
		tri[3] = 1;
		tri[4] = 3;
		tri[5] = 2;

		// Back triangles
		tri[6] = 5;
		tri[7] = 4;
		tri[8] = 7;
		tri[9] = 4;
		tri[10] = 6;
		tri[11] = 7;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;
		mesh.RecalculateNormals();

		return mesh;
	}
}
