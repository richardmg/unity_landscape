using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelPlaneScript : MonoBehaviour {
	public int atlasIndex = 0;
	public int subImageWidth = 16;
	public int subImageHeight = 8;
	public bool addFront = true;
	public bool addBack = true;
	public bool addVolume = true;
	public bool trimVolume = false;

	Texture2D texture;
	Vector2 uvSubImageBottomLeft;
	Vector2 uvOnePixel;
	int startPixelX;
	int startPixelY;

	const int kVoxelNotFound = -1;

	void Start () {
		List<CombineInstance> ciList = new List<CombineInstance>();
		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.material.mainTexture;

		// Caluclate uv coords based on atlasIndex
		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;

		uvSubImageBottomLeft = new Vector2((float)startPixelX / texture.width, ((float)startPixelY / texture.height));
		uvOnePixel = new Vector2(1.0f / texture.width, 1.0f / texture.height);

		// Traverse each row in the texture
		for (int y = 0; y < subImageHeight; ++y) {
			int x2 = -1;

			// Traverse each column in the texture and look for voxel strips
			while (x2 != subImageWidth) {
				int x1 = findFirstVoxelAlphaTest(x2 + 1, y, 1);
				if (x1 == kVoxelNotFound) {
					x2 =  subImageWidth;
					continue;
				}

				x2 = findFirstVoxelAlphaTest(x1 + 1, y, 0);
				if (x2 == kVoxelNotFound)
					x2 = subImageWidth;

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
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;

		print("Vertex count for mesh at index " + atlasIndex + " (" + gameObject.name + "): " + finalMesh.vertices.Length);
	}

	int findFirstVoxelAlphaTest(int startX, int startY, int alpha)
	{
		for (int x = startX; x < subImageWidth; ++x) {
			Color c = texture.GetPixel(startPixelX + x, startPixelY + startY);
			if (Mathf.CeilToInt(c.a) == alpha)
				return x;
		}
		return kVoxelNotFound;
	}

	Mesh createVoxelLineMesh(int voxelX1, int voxelX2, int voxelY)
	{
		float w = voxelX2 - voxelX1;
		float uvx1 = uvSubImageBottomLeft.x + (voxelX1 * uvOnePixel.x);
		float uvx2 = uvSubImageBottomLeft.x + (voxelX2 * uvOnePixel.x);
		float uvy = uvSubImageBottomLeft.y + (voxelY * uvOnePixel.y);

		Vector3[] v = new Vector3[8];
		Vector3[] n = new Vector3[8];
		Vector2[] uv = new Vector2[8];
		int[] tri = new int[36];
		float half = 0.5f;

		// Front vertices
		v[0].x = -half; v[0].y = -half; v[0].z = -half;
		v[1].x = -half; v[1].y = half; v[1].z = -half;
		v[2].x = w - half; v[2].y = -half; v[2].z = -half;
		v[3].x = w - half; v[3].y = half; v[3].z = -half;

		// Back vertices
		v[4].x = -half; v[4].y = -half; v[4].z = half;
		v[5].x = -half; v[5].y = half; v[5].z = half;
		v[6].x = w - half; v[6].y = -half; v[6].z = half;
		v[7].x = w - half; v[7].y = half; v[7].z = half;

		// Front normals
		n[0].x = -1; n[0].y = -1; n[0].z = -1;
		n[1].x = -1; n[1].y = 1; n[1].z = -1;
		n[2].x = 1; n[2].y = -1; n[2].z = -1;
		n[3].x = 1; n[3].y = 1; n[3].z = -1;

		// Back normals
		n[4].x = -1; n[4].y = -1; n[4].z = 1;
		n[5].x = -1; n[5].y = 1; n[5].z = 1;
		n[6].x = 1; n[6].y = -1; n[6].z = 1;
		n[7].x = 1; n[7].y = 1; n[7].z = 1;

		// Front texture coords
		uv[0].x = uvx1; uv[0].y = uvy;
		uv[1].x = uvx1; uv[1].y = uvy;
		uv[2].x = uvx2; uv[2].y = uvy;
		uv[3].x = uvx2; uv[3].y = uvy;

		// Back texture coords
		uv[4].x = uvx1; uv[4].y = uvy;
		uv[5].x = uvx1; uv[5].y = uvy;
		uv[6].x = uvx2; uv[6].y = uvy;
		uv[7].x = uvx2; uv[7].y = uvy;

		// Front triangles
		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 2;
		tri[3] = 2;
		tri[4] = 1;
		tri[5] = 3;

		// Back triangles
		tri[6] = 6;
		tri[7] = 7;
		tri[8] = 4;
		tri[9] = 4;
		tri[10] = 7;
		tri[11] = 5;

		// Top triangles
		tri[12] = 1;
		tri[13] = 5;
		tri[14] = 3;
		tri[15] = 3;
		tri[16] = 5;
		tri[17] = 7;

		// Bottom triangles
		tri[18] = 4;
		tri[19] = 0;
		tri[20] = 6;
		tri[21] = 6;
		tri[22] = 0;
		tri[23] = 2;

		// Left triangles
		tri[24] = 4;
		tri[25] = 5;
		tri[26] = 0;
		tri[27] = 0;
		tri[28] = 5;
		tri[29] = 1;

		// Right triangles
		tri[30] = 2;
		tri[31] = 3;
		tri[32] = 6;
		tri[33] = 6;
		tri[34] = 3;
		tri[35] = 7;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.normals = n;
		mesh.uv = uv;
		mesh.triangles = tri;

		return mesh;
	}
}
