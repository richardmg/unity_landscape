using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelPlaneScript : MonoBehaviour {
	private Texture2D texture;
	private int cols;
	private int rows;

	const int kVoxelNotFound = -1;

	void Start () {
		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.material.mainTexture;

		cols = texture.width;
		rows = texture.height;

		List<CombineInstance> ciList = new List<CombineInstance>();

		// Traverse each row in the texture
		for (int y = 0; y < rows; ++y) {
			int x2 = -1;

			// Traverse each column in the texture and look for voxel strips
			while (x2 != cols) {
				int x1 = findFirstVoxelAlphaTest(x2 + 1, y, 1);
				if (x1 == kVoxelNotFound) {
					x2 = cols;
					continue;
				}

				x2 = findFirstVoxelAlphaTest(x1 + 1, y, 0);
				if (x2 == kVoxelNotFound)
					x2 = cols;

				Mesh mesh = createVoxelLineMesh(x1, x2, y);
				Matrix4x4 transform = new Matrix4x4();
				transform.SetTRS(new Vector3(x1, y, 0), Quaternion.identity, new Vector3(1, 1, 1));

				CombineInstance ci = new CombineInstance();
				ci.mesh = mesh;
				ci.transform = transform;
				ciList.Add(ci);
			}
		}

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ciList.ToArray(), true, true);
		finalMesh.RecalculateNormals();
		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;
	}

	int findFirstVoxelAlphaTest(int startX, int startY, int alpha)
	{
		for (int x = startX; x < cols; ++x) {
			Color c = texture.GetPixel(x, startY);
			if (Mathf.CeilToInt(c.a) == alpha)
				return x;
		}
		return kVoxelNotFound;
	}

//	int findFirstVoxelCenterSampleAlphaTest(int startX, int startY, int alpha)
//	{
//		float textureStepX = texture.width / cols;
//		float textureStepY = texture.height / rows;
//
//		for (int x = startX; x < cols; ++x) {
//			// Grab center pixel in texel. This will fail for texels that are not solid
//			int tx = (int)((x * textureStepX) + (textureStepX / 2));
//			int ty = (int)((startY * textureStepY) + (textureStepY / 2));
//			Color c = texture.GetPixel(tx, ty);
//			// Either the textel is transparent, or it's not
//			if (Mathf.CeilToInt(c.a) == alpha)
//				return x;
//		}
//		return kVoxelNotFound;
//	}

	Mesh createVoxelLineMesh(int voxelX1, int voxelX2, int voxelY)
	{
		float w = voxelX2 - voxelX1;
		float uvx1 = (1.0f / cols) * voxelX1;
		float uvx2 = (1.0f / cols) * voxelX2;
		float uvy1 = (1.0f / rows) * voxelY;
		float uvy2 = (1.0f / rows) * voxelY;

		Vector3[] v = new Vector3[24];
		Vector2[] uv = new Vector2[24];
		int[] tri = new int[36];

		// Front vertices
		v[0].x = 0; v[0].y = 0; v[0].z = 0;
		v[1].x = 0; v[1].y = 1; v[1].z = 0;
		v[2].x = w; v[2].y = 0; v[2].z = 0;
		v[3].x = w; v[3].y = 1; v[3].z = 0;

		// Back vertices
		v[4].x = 0; v[4].y = 0; v[4].z = 1;
		v[5].x = 0; v[5].y = 1; v[5].z = 1;
		v[6].x = w; v[6].y = 0; v[6].z = 1;
		v[7].x = w; v[7].y = 1; v[7].z = 1;

		// Top vertices
		v[ 8].x = 0; v[ 8].y = 1; v[ 8].z = 0;
		v[ 9].x = 0; v[ 9].y = 1; v[ 9].z = 1;
		v[10].x = w; v[10].y = 1; v[10].z = 0;
		v[11].x = w; v[11].y = 1; v[11].z = 1;

		// Bottom vertices
		v[12].x = 0; v[12].y = 0; v[12].z = 0;
		v[13].x = 0; v[13].y = 0; v[13].z = 1;
		v[14].x = w; v[14].y = 0; v[14].z = 0;
		v[15].x = w; v[15].y = 0; v[15].z = 1;

		// Left vertices
		v[16].x = 0; v[16].y = 0; v[16].z = 1;
		v[17].x = 0; v[17].y = 1; v[17].z = 1;
		v[18].x = 0; v[18].y = 0; v[18].z = 0;
		v[19].x = 0; v[19].y = 1; v[19].z = 0;

		// Right vertices
		v[20].x = w; v[20].y = 0; v[20].z = 0;
		v[21].x = w; v[21].y = 1; v[21].z = 0;
		v[22].x = w; v[22].y = 0; v[22].z = 1;
		v[23].x = w; v[23].y = 1; v[23].z = 1;

		// Front texture coords
		uv[0].x = uvx1; uv[0].y = uvy1;
		uv[1].x = uvx1; uv[1].y = uvy2;
		uv[2].x = uvx2; uv[2].y = uvy1;
		uv[3].x = uvx2; uv[3].y = uvy2;

		// Back texture coords
		uv[4].x = uvx1; uv[4].y = uvy1;
		uv[5].x = uvx1; uv[5].y = uvy2;
		uv[6].x = uvx2; uv[6].y = uvy1;
		uv[7].x = uvx2; uv[7].y = uvy2;

		// Top texture coords
		uv[ 8].x = uvx1; uv[ 8].y = uvy2;
		uv[ 9].x = uvx1; uv[ 9].y = uvy2;
		uv[10].x = uvx2; uv[10].y = uvy2;
		uv[11].x = uvx2; uv[11].y = uvy2;

		// Bottom texture coords
		uv[12].x = uvx1; uv[12].y = uvy1;
		uv[13].x = uvx1; uv[13].y = uvy1;
		uv[14].x = uvx2; uv[14].y = uvy1;
		uv[15].x = uvx2; uv[15].y = uvy1;

		// Left texture coords
		float ex2 = (1.0f / cols) * (voxelX1 + 1);
		uv[16].x = uvx1;  uv[16].y = uvy1;
		uv[17].x = uvx1;  uv[17].y = uvy2;
		uv[18].x = ex2; uv[18].y = uvy1;
		uv[19].x = ex2; uv[19].y = uvy2;

		// Right texture coords
		ex2 = (1.0f / cols) * (voxelX2 - 1);
		uv[20].x = ex2; uv[20].y = uvy1;
		uv[21].x = ex2; uv[21].y = uvy2;
		uv[22].x = uvx2;  uv[22].y = uvy1;
		uv[23].x = uvx2;  uv[23].y = uvy2;

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

		// Top triangles
		tri[12] = 8;
		tri[13] = 9;
		tri[14] = 10;
		tri[15] = 10;
		tri[16] = 9;
		tri[17] = 11;

		// Bottom triangles
		tri[18] = 13;
		tri[19] = 12;
		tri[20] = 15;
		tri[21] = 15;
		tri[22] = 12;
		tri[23] = 14;

		// Left triangles
		tri[24] = 16;
		tri[25] = 17;
		tri[26] = 18;
		tri[27] = 18;
		tri[28] = 17;
		tri[29] = 19;

		// Right triangles
		tri[30] = 20;
		tri[31] = 21;
		tri[32] = 22;
		tri[33] = 22;
		tri[34] = 21;
		tri[35] = 23;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;

		return mesh;
	}
}
