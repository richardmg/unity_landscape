﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelVolumeScript : MonoBehaviour {
	private Texture2D texture;
	private int cols;
	private int rows;

	const int kTopSide = 1;
	const int kBottomSide = 0;
	const int kLeftSide = 0;
	const int kRightSide = 1;
	const int kFrontSide = 0;
	const int kBackSide = 1;

	void Start () {
		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.material.mainTexture;

		cols = texture.width;
		rows = texture.height;

		List<CombineInstance> ciList = new List<CombineInstance>();

		// Create front and back quad
//		ciList.Add(createCombineInstance(createXYQuad(0, kFrontSide), new Vector3(0, 0, 0)));
		ciList.Add(createCombineInstance(createXYQuad(1, kBackSide), new Vector3(0, 0, 0)));


		// Traverse each row in the texture
		for (int y = 0; y < rows; ++y) {
//			if (!hasOpaquePixelsInRow(y))
//				continue;
			ciList.Add(createCombineInstance(createXZQuad(y, kBottomSide), new Vector3(0, y, 0)));
		}

		for (int x = 0; x < cols; ++x) {
//			if (!hasOpaquePixelsInCol(x))
//				continue;
//			ciList.Add(createCombineInstance(createZYQuad(x), new Vector3(0, 0, 0)));
		}

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ciList.ToArray(), true, true);
		finalMesh.RecalculateNormals();
		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;
	}

	bool hasOpaquePixelsInRow(int row)
	{
		for (int x = 0; x < cols; ++x) {
			Color c = texture.GetPixel(x, row);
			if (c.a != 0)
				return true;
		}
		return false;
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
		int[] tri = new int[6];

		v[0].x = 0;    v[0].y = 0;    v[0].z = side;
		v[1].x = 0;    v[1].y = rows; v[1].z = side;
		v[2].x = cols; v[2].y = rows; v[2].z = side;
		v[3].x = cols; v[3].y = 0;    v[3].z = side;

		uv[0].x = 0; uv[0].y = 0;
		uv[1].x = 0; uv[1].y = 1;
		uv[2].x = 1; uv[2].y = 1;
		uv[3].x = 1; uv[3].y = 0;

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
		mesh.triangles = tri;

		return mesh;
	}

	Mesh createXZQuad(int y, int side)
	{
		Vector3[] v = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		int[] tri = new int[6];
		float uvy0 = (1.0f / rows) * y;
		float uvy1 = (1.0f / rows) * y;

		v[0].x = 0;    v[0].y = side; v[0].z = 0;
		v[1].x = 0;    v[1].y = side; v[1].z = 1;
		v[2].x = cols; v[2].y = side; v[2].z = 1;
		v[3].x = cols; v[3].y = side; v[3].z = 0;

		uv[0].x = 0; uv[0].y = uvy0;
		uv[1].x = 0; uv[1].y = uvy1;
		uv[2].x = 1; uv[2].y = uvy1;
		uv[3].x = 1; uv[3].y = uvy0;

		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 3;
		tri[3] = 3;
		tri[4] = 1;
		tri[5] = 2;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;

		return mesh;
	}

	Mesh createZYQuad(int x)
	{
		Vector3[] v = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		int[] tri = new int[6];
		float uvx0 = (1.0f / cols) * x;

		v[0].x = x; v[0].y = 0;    v[0].z = 1;
		v[1].x = x; v[1].y = rows; v[1].z = 1;
		v[2].x = x; v[2].y = rows; v[2].z = 0;
		v[3].x = x; v[3].y = 0;    v[3].z = 0;

		uv[0].x = uvx0; uv[0].y = 0;
		uv[1].x = uvx0; uv[1].y = 1;
		uv[2].x = uvx0; uv[2].y = 1;
		uv[3].x = uvx0; uv[3].y = 0;

		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 3;
		tri[3] = 3;
		tri[4] = 1;
		tri[5] = 2;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;

		return mesh;
	}
}
