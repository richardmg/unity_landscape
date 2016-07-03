﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelCubesScript : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public float uniScale = 1;
	public float cascade = 0.0f;

	Texture2D texture;
	int startPixelX;
	int startPixelY;

	static int subImageWidth = 16;
	static int subImageHeight = 8;

	static int[] indices = new int[8];
	static Vector3 vec = new Vector3();
	static List<Vector3> verticeList = new List<Vector3>(); 
	static List<Vector2> uvAtlasCubeRectEncodedList = new List<Vector2>(); 
	static List<int> normalCodeList = new List<int>(); 
	static List<int> tri = new List<int>(); 

	static float n = 1.0f;
	static Vector3[] normalForCode = {
		new Vector3(-1, n, -1).normalized,
		new Vector3(-1, n, -1).normalized,
		new Vector3(1, n, -1).normalized,
		new Vector3(1, n, -1).normalized,
		new Vector3(-1, n, 1).normalized,
		new Vector3(-1, n, 1).normalized,
		new Vector3(1, n, 1).normalized,
		new Vector3(1, n, 1).normalized
	};

	const int kVoxelNotFound = -1;
	const int kBottomLeft = 0;
	const int kTopLeft = 1;
	const int kBottomRight = 2;
	const int kTopRight = 3;
	const int kBackSide = 4;

	void Start () {
		verticeList.Clear();
		uvAtlasCubeRectEncodedList.Clear();
		normalCodeList.Clear();
		tri.Clear();

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;

		// Caluclate uv coords based on atlasIndex. Note that we don't assign any uv coords to the
		// verticeList, since those can be calculated directly (and more precisely) in the shader
		// based on the local position of the vertices themselves.
		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;

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

				createVoxelLineMesh(x1, x2, y, y + 1);
			}
		}

		Mesh mesh = new Mesh();
		mesh.vertices = verticeList.ToArray();
		mesh.triangles = tri.ToArray();

		// When using object batching, local vertices and normals will be translated on the CPU before
		// passed down to the GPU. We therefore loose the original values in the shader, which we need.
		// We therefore encode this information covered as vertex color.
		int vertexCount = mesh.vertices.Length;
		Color[] cubeDesc = new Color[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];

		for (int i = 0; i < vertexCount; ++i) {
			Vector3 v = mesh.vertices[i];
			float uvAtlasX = (startPixelX + v.x) / texture.width;
			float uvAtlasY = (startPixelY + v.y) / texture.height;
			cubeDesc[i] = new Color(uvAtlasX, uvAtlasY, normalCodeList[i], voxelDepth);
			normals[i] = normalForCode[normalCodeList[i]];
		}

		mesh.uv = uvAtlasCubeRectEncodedList.ToArray();
		mesh.colors = cubeDesc;
		mesh.normals = normals;

		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		print("VoxelCubes: vertex count for " + gameObject.name + ": " + mesh.vertices.Length);
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

	int getVertexIndex(Vector3 v, Vector2 uvRect, int normalCode, int index)
	{
		// Check if the vertex can be shared with one already created. Note that this causes the normal to
		// be wrong for the cube on top, but that is corrected in the shader.
		if (index != -1)
			return index;

		verticeList.Add(new Vector3(v.x * uniScale, v.y * uniScale, v.z * uniScale));
		normalCodeList.Add(normalCode);
		uvAtlasCubeRectEncodedList.Add(uvRect);

		return verticeList.Count - 1;
	}

	bool writeIndex(int index, float x, float y, float z, Vector2 uvRect, int normalCode)
	{
		vec.Set(x, y, z);
		int i = -1;//verticeList.FindIndex(v2 => v2 == vec);
		indices[index] = getVertexIndex(vec, uvRect, normalCode, i);
		return i != -1;
	}

	void createVoxelLineMesh(float voxelX1, float voxelX2, float voxelY1, float voxelY2)
	{
		float voxelZ1 = 0;
		float voxelZ2 = voxelDepth;
		if (cascade != 0 && (int)voxelY1 % 2 == 0) {
			voxelX1 += cascade;
			voxelX2 -= cascade;
			voxelZ1 += cascade;
			voxelZ2 -= cascade;
		}

		int atlasCubeRectX1 = (int)(startPixelX + voxelX1);
		int atlasCubeRectY1 = (int)(startPixelY + voxelY1);
		float atlasCubeRectX2 = (float)(startPixelX + voxelX2 - 0.5) / texture.width; 
		float atlasCubeRectY2 = (float)(startPixelY + voxelY2 - 0.5) / texture.height;
		Vector2 uvAtlasCubeRectEncoded = new Vector2(atlasCubeRectX1 + atlasCubeRectX2, atlasCubeRectY1 + atlasCubeRectY2);

		bool reuse0 = writeIndex(0, voxelX1, voxelY1, voxelZ1, uvAtlasCubeRectEncoded, kBottomLeft);
		writeIndex(1, voxelX1, voxelY2, voxelZ1, uvAtlasCubeRectEncoded, kTopLeft);
		bool reuse2 = writeIndex(2, voxelX2, voxelY1, voxelZ1, uvAtlasCubeRectEncoded, kBottomRight);
		writeIndex(3, voxelX2, voxelY2, voxelZ1, uvAtlasCubeRectEncoded, kTopRight);
		bool reuse4 = writeIndex(4, voxelX1, voxelY1, voxelZ2, uvAtlasCubeRectEncoded, kBottomLeft + kBackSide);
		writeIndex(5, voxelX1, voxelY2, voxelZ2, uvAtlasCubeRectEncoded, kTopLeft + kBackSide);
		bool reuse6 = writeIndex(6, voxelX2, voxelY1, voxelZ2, uvAtlasCubeRectEncoded, kBottomRight + kBackSide);
		writeIndex(7, voxelX2, voxelY2, voxelZ2, uvAtlasCubeRectEncoded, kTopRight + kBackSide);

		if (reuse0 && reuse2 && reuse4 && reuse6) {
			// All nodes as shared. Flip nodes on one side to mark that this is
			// bottom of the cube. For the cubes below, no top face will be visible.
			normalCodeList[indices[0]] = kBottomLeft;
			normalCodeList[indices[4]] = kBottomLeft + kBackSide;
		}

		// Front triangles
		tri.Add(indices[0]);
		tri.Add(indices[1]);
		tri.Add(indices[2]);
		tri.Add(indices[2]);
		tri.Add(indices[1]);
		tri.Add(indices[3]);

		// Back triangles
		tri.Add(indices[6]);
		tri.Add(indices[7]);
		tri.Add(indices[4]);
		tri.Add(indices[4]);
		tri.Add(indices[7]);
		tri.Add(indices[5]);

		// Top triangles
		tri.Add(indices[1]);
		tri.Add(indices[5]);
		tri.Add(indices[3]);
		tri.Add(indices[3]);
		tri.Add(indices[5]);
		tri.Add(indices[7]);

		// Bottom triangles
		tri.Add(indices[4]);
		tri.Add(indices[0]);
		tri.Add(indices[6]);
		tri.Add(indices[6]);
		tri.Add(indices[0]);
		tri.Add(indices[2]);

		// Left triangles
		tri.Add(indices[4]);
		tri.Add(indices[5]);
		tri.Add(indices[0]);
		tri.Add(indices[0]);
		tri.Add(indices[5]);
		tri.Add(indices[1]);

		// Right triangles
		tri.Add(indices[2]);
		tri.Add(indices[3]);
		tri.Add(indices[6]);
		tri.Add(indices[6]);
		tri.Add(indices[3]);
		tri.Add(indices[7]);
	}
}
