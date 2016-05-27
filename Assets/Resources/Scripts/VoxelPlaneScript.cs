using UnityEngine;
using System.Collections;

public class VoxelPlaneScript : MonoBehaviour {
	private float voxelWidth = 0.1f;
	private float voxelHeight = 0.1f;
	private float voxelDepth = 1;
	private int textureVoxelWidth = 10;
	private int textureVoxelHeight = 10;

	void Start () {
		CombineInstance[] ci = new CombineInstance[textureVoxelHeight];

		for (int i = 0; i < textureVoxelHeight; ++i) {
			Mesh mesh = createVoxelMesh(0, i, textureVoxelWidth);
			Matrix4x4 transform = new Matrix4x4();
			transform.SetTRS(new Vector3(0, i * voxelHeight, 0), Quaternion.identity, new Vector3(1, 1, 1));
			ci[i].mesh = mesh;
			ci[i].transform = transform;
		}

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ci, true, true);
		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = (Material)Resources.Load("Materials/CutoffM");
		// CHANGE MATERIAL TO NOT BE TWO-SIDED
	}

	Mesh createVoxelMesh(int voxelX, int voxelY, int voxelCountX)
	{
		float w = (voxelCountX - voxelX) * voxelWidth;
		float h = voxelHeight;
		float sx = (1.0f / textureVoxelWidth) * voxelX;
		float ex = (1.0f / textureVoxelWidth) * voxelCountX;
		float sy = (1.0f / textureVoxelHeight) * voxelY;
		float ey = (1.0f / textureVoxelHeight) * (voxelY + 1);

		Vector3[] v = new Vector3[8];
		Vector2[] uv = new Vector2[8];
		int[] tri = new int[12];

		// Front vertices
		v[0].x = 0; v[0].y = 0; v[0].z = 0;
		v[1].x = 0; v[1].y = h; v[1].z = 0;
		v[2].x = w; v[2].y = 0; v[2].z = 0;
		v[3].x = w; v[3].y = h; v[3].z = 0;

		// Back vertices
		for (int i = 4; i < 8; ++i) {
			v[i] = v[i - 4];
			v[i].z = voxelDepth;
		}

		// Front texture coords
		uv[0].x = sx; uv[0].y = sy;
		uv[1].x = sx; uv[1].y = ey;
		uv[2].x = ex; uv[2].y = sy;
		uv[3].x = ex; uv[3].y = ey;

		// Back texture coords
		for (int i = 4; i < 8; ++i)
			uv[i] = uv[i - 4];

		// Front triangles
		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 2;
		tri[3] = 2;
		tri[4] = 1;
		tri[5] = 3;

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
