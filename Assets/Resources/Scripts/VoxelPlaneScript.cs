using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelPlaneScript : MonoBehaviour {
	public Texture2D texture;
	public float voxelWidth = 0.1f;
	public float voxelHeight = 0.1f;
	public float voxelDepth = 0.1f;

	private int cols;
	private int rows;

	const int kVoxelNotFound = -1;

	void Start () {
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
				transform.SetTRS(new Vector3(x1 * voxelWidth, y * voxelHeight, 0), Quaternion.identity, new Vector3(1, 1, 1));

				CombineInstance ci = new CombineInstance();
				ci.mesh = mesh;
				ci.transform = transform;
				ciList.Add(ci);
			}
		}

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ciList.ToArray(), true, true);
		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = (Material)Resources.Load("Materials/CutoffM");
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
		float w = (voxelX2 - voxelX1) * voxelWidth;
		float h = voxelHeight;
		float z = voxelDepth;
		float sx = (1.0f / cols) * voxelX1;
		float ex = (1.0f / cols) * voxelX2;
		float sy = (1.0f / rows) * voxelY;
		float ey = (1.0f / rows) * (voxelY + 1);

		Vector3[] v = new Vector3[24];
		Vector2[] uv = new Vector2[24];
		int[] tri = new int[36];

		// Front vertices
		v[0].x = 0; v[0].y = 0; v[0].z = 0;
		v[1].x = 0; v[1].y = h; v[1].z = 0;
		v[2].x = w; v[2].y = 0; v[2].z = 0;
		v[3].x = w; v[3].y = h; v[3].z = 0;

		// Back vertices
		v[4].x = 0; v[4].y = 0; v[4].z = z;
		v[5].x = 0; v[5].y = h; v[5].z = z;
		v[6].x = w; v[6].y = 0; v[6].z = z;
		v[7].x = w; v[7].y = h; v[7].z = z;

		// Top vertices
		v[ 8].x = 0; v[ 8].y = h; v[ 8].z = 0;
		v[ 9].x = 0; v[ 9].y = h; v[ 9].z = z;
		v[10].x = w; v[10].y = h; v[10].z = 0;
		v[11].x = w; v[11].y = h; v[11].z = z;

		// Bottom vertices
		v[12].x = 0; v[12].y = 0; v[12].z = 0;
		v[13].x = 0; v[13].y = 0; v[13].z = z;
		v[14].x = w; v[14].y = 0; v[14].z = 0;
		v[15].x = w; v[15].y = 0; v[15].z = z;

		// Left vertices
		v[16].x = 0; v[16].y = 0; v[16].z = z;
		v[17].x = 0; v[17].y = h; v[17].z = z;
		v[18].x = 0; v[18].y = 0; v[18].z = 0;
		v[19].x = 0; v[19].y = h; v[19].z = 0;

		// Right vertices
		v[20].x = w; v[20].y = 0; v[20].z = z;
		v[21].x = w; v[21].y = h; v[21].z = z;
		v[22].x = w; v[22].y = 0; v[22].z = 0;
		v[23].x = w; v[23].y = h; v[23].z = 0;

		// Front texture coords
		uv[0].x = sx; uv[0].y = sy;
		uv[1].x = sx; uv[1].y = ey;
		uv[2].x = ex; uv[2].y = sy;
		uv[3].x = ex; uv[3].y = ey;

		// Back texture coords
		uv[4].x = sx; uv[4].y = sy;
		uv[5].x = sx; uv[5].y = ey;
		uv[6].x = ex; uv[6].y = sy;
		uv[7].x = ex; uv[7].y = ey;

		// Top texture coords
		uv[ 8].x = sx; uv[ 8].y = ey;
		uv[ 9].x = sx; uv[ 9].y = ey;
		uv[10].x = ex; uv[10].y = ey;
		uv[11].x = ex; uv[11].y = ey;

		// Bottom texture coords
		uv[12].x = sx; uv[12].y = sy;
		uv[13].x = sx; uv[13].y = sy;
		uv[14].x = ex; uv[14].y = sy;
		uv[15].x = ex; uv[15].y = sy;

		// Left texture coords
		uv[16].x = sx; uv[16].y = sy;
		uv[17].x = sx; uv[17].y = ey;
		uv[18].x = sx; uv[18].y = sy;
		uv[19].x = sx; uv[19].y = ey;

		// Right texture coords
		uv[20].x = ex; uv[20].y = sy;
		uv[21].x = ex; uv[21].y = ey;
		uv[22].x = ex; uv[22].y = sy;
		uv[23].x = ex; uv[23].y = ey;

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
		tri[30] = 22;
		tri[31] = 23;
		tri[32] = 21;
		tri[33] = 21;
		tri[34] = 23;
		tri[35] = 20;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;
		mesh.RecalculateNormals();

		return mesh;
	}
}
