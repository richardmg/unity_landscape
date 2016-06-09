using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelVolumeScript : MonoBehaviour {
	public int atlasIndex = 0;
	public int subImageWidth = 16;
	public int subImageHeight = 8;
	public bool addFront = true;
	public bool addBack = true;
	public bool addVolume = true;
	public bool trimVolume = false;
	public float textureBleedCorrection = 0.16f;

	Texture2D texture;

	float uvOnePixelX;
	float uvOnePixelY;

	float uvx1;
	float uvx2;
	float uvy1;
	float uvy2;

	const int kTopSide = 1;
	const int kBottomSide = 0;
	const int kLeftSide = 0;
	const int kRightSide = 1;
	const int kFrontSide = 0;
	const int kBackSide = 1;

	void Start () {
		List<CombineInstance> ciList = new List<CombineInstance>();
		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.material.mainTexture;

		// Caluclate uv coords based on atlasIndex
		int startPixelX = (atlasIndex * subImageWidth) % texture.width;
		int startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;
		uvOnePixelX = 1.0f / texture.width;
		uvOnePixelY = 1.0f / texture.height;

		uvx1 = (float)startPixelX / texture.width;
		uvx2 = uvx1 + (subImageWidth * uvOnePixelX) - (uvOnePixelX * textureBleedCorrection);
		uvy1 = 1 - ((float)(startPixelY + subImageHeight) / texture.height);
		uvy2 = uvy1 + (subImageHeight * uvOnePixelY) - (uvOnePixelY * textureBleedCorrection);

		// Create mesh parts
		if (addFront)
			ciList.Add(createCombineInstance(createXYQuad(0, kFrontSide), new Vector3(0, 0, 0)));
		if (addBack)
			ciList.Add(createCombineInstance(createXYQuad(1, kBackSide), new Vector3(0, 0, 0)));

		float xOffset = 1 - (float)(subImageWidth  - textureBleedCorrection) / (float)(subImageWidth);
		float yOffset = 1 - (float)(subImageHeight - textureBleedCorrection) / (float)(subImageHeight);

		int x, y;
		if (addVolume) {
			for (x = 0; x < subImageWidth; ++x) {
				if (trimVolume && !hasVerticalEdgesInCol(x))
					continue;
				ciList.Add(createCombineInstance(createZYQuad(x, kLeftSide), new Vector3(x + (x * xOffset), 0, 0)));
			}

			for (y = 0; y < subImageHeight; ++y) {
				if (trimVolume && !hasHorisontalEdgesInRow(y))
					continue;
				ciList.Add(createCombineInstance(createXZQuad(y, kBottomSide), new Vector3(0, y + (y * yOffset), 0)));
			}

			x = (subImageWidth - 1);
			y = (subImageHeight - 1);
			ciList.Add(createCombineInstance(createZYQuad(subImageWidth - 1, kRightSide), new Vector3(x + (x * xOffset) - textureBleedCorrection, 0, 0)));
			ciList.Add(createCombineInstance(createXZQuad(subImageHeight - 1, kTopSide), new Vector3(0, y + (y * yOffset) - textureBleedCorrection, 0)));
		}

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ciList.ToArray(), true, true);
		finalMesh.RecalculateNormals();
		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;
	}

	bool hasHorisontalEdgesInRow(int row)
	{
//		for (int x = 0; x < cols; ++x) {
//			Color c = texture.GetPixel(x, row);
//			if (c.a != 0)
//				return true;
//		}
		return true;
	}

	bool hasVerticalEdgesInCol(int col)
	{
		return true;
	}

	CombineInstance createCombineInstance(Mesh mesh, Vector3 pos)
	{
		CombineInstance ci = new CombineInstance();
		ci.mesh = mesh;
		Matrix4x4 transform = new Matrix4x4();
		transform.SetTRS(pos, Quaternion.identity, new Vector3(1, 1, 1));
		ci.transform = transform;
		return ci;
	}

	Mesh createXYQuad(int z, int side)
	{
		Vector3[] v = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		Vector2[] uv2 = new Vector2[4];
		int[] tri = new int[6];

		v[0].x = 0; 			v[0].y = 0;				 v[0].z = side;
		v[1].x = 0; 			v[1].y = subImageHeight; v[1].z = side;
		v[2].x = subImageWidth; v[2].y = subImageHeight; v[2].z = side;
		v[3].x = subImageWidth; v[3].y = 0;				 v[3].z = side;

		uv[0].x = uvx1; uv[0].y = uvy1;
		uv[1].x = uvx1; uv[1].y = uvy2;
		uv[2].x = uvx2; uv[2].y = uvy2;
		uv[3].x = uvx2; uv[3].y = uvy1;

		uv2[0].x = 0; uv2[0].y = 0;
		uv2[1].x = 0; uv2[1].y = 1;
		uv2[2].x = 1; uv2[2].y = 1;
		uv2[3].x = 1; uv2[3].y = 0;

		if (side == kFrontSide) {
			tri[0] = 0;
			tri[1] = 1;
			tri[2] = 3;
			tri[3] = 3;
			tri[4] = 1;
			tri[5] = 2;
		} else {
			tri[0] = 3;
			tri[1] = 2;
			tri[2] = 0;
			tri[3] = 0;
			tri[4] = 2;
			tri[5] = 1;
		}

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.uv2 = uv2;
		mesh.triangles = tri;

		return mesh;
	}

	Mesh createXZQuad(int y, int side)
	{
		Vector3[] v = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		Vector2[] uv2 = new Vector2[4];
		int[] tri = new int[6];
		float uvy = uvy1 + (y * uvOnePixelY);
		float x = subImageWidth;

		v[0].x = 0; v[0].y = side; v[0].z = 1;
		v[1].x = 0; v[1].y = side; v[1].z = 0;
		v[2].x = x; v[2].y = side; v[2].z = 0;
		v[3].x = x; v[3].y = side; v[3].z = 1;

		uv[0].x = uvx1; uv[0].y = uvy;
		uv[1].x = uvx1; uv[1].y = uvy;
		uv[2].x = uvx2; uv[2].y = uvy;
		uv[3].x = uvx2; uv[3].y = uvy;

		uv2[0].x = 0; uv2[0].y = 0;
		uv2[1].x = 0; uv2[1].y = 1;
		uv2[2].x = 1; uv2[2].y = 1;
		uv2[3].x = 1; uv2[3].y = 0;

		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 3;
		tri[3] = 3;
		tri[4] = 1;
		tri[5] = 2;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.uv2 = uv2;
		mesh.triangles = tri;

		return mesh;
	}

	Mesh createZYQuad(int x, int side)
	{
		Vector3[] v = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		Vector2[] uv2 = new Vector2[4];
		int[] tri = new int[6];
		float uvx = uvx1 + (x * uvOnePixelX);
		float y = subImageHeight;

		v[0].x = side; v[0].y = 0; v[0].z = 1;
		v[1].x = side; v[1].y = y; v[1].z = 1;
		v[2].x = side; v[2].y = y; v[2].z = 0;
		v[3].x = side; v[3].y = 0; v[3].z = 0;

		uv[0].x = uvx; uv[0].y = uvy1;
		uv[1].x = uvx; uv[1].y = uvy2;
		uv[2].x = uvx; uv[2].y = uvy2;
		uv[3].x = uvx; uv[3].y = uvy1;

		uv2[0].x = 0; uv2[0].y = 0;
		uv2[1].x = 0; uv2[1].y = 1;
		uv2[2].x = 1; uv2[2].y = 1;
		uv2[3].x = 1; uv2[3].y = 0;

		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 3;
		tri[3] = 3;
		tri[4] = 1;
		tri[5] = 2;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.uv2 = uv2;
		mesh.triangles = tri;

		return mesh;
	}
}
