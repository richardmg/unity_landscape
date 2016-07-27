﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NormalCode = System.Int32;

public class VoxelQuadScript : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 1;

	public bool quadCountX = false;
	public bool quadCountY = false;
	public int quadCountZ = 4;
	public float planeOffset = 0.01f;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;
	public int readonlyTriangleCount = 0;

	Texture2D texture;
	Vector3 effectiveSize;
	Vector2 uvAtlasSubImageRectEncoded;

	int startPixelX;
	int startPixelY;
	const int subImageWidth = 16;
	const int subImageHeight = 8;

	static List<Vector3> verticeList = new List<Vector3>(); 
	static List<int> tri = new List<int>(); 

	static Vector3 kVecBottomLeft = new Vector3(-1, -1, -1);
	static Vector3 kVecDeltaNormal = new Vector3(2, 2, 2);

	const int kVoxelNotFound = -1;

	const NormalCode kNormalCodeFront = 0;
	const NormalCode kNormalCodeMiddle = 1;
	const NormalCode kNormalCodeBack = 2;

	void Start ()
	{
		rebuildObject();
	}

	void OnValidate()
	{
		rebuildObject();
	}

	Vector3 getVolumeNormal(Vector3 vertex)
	{
		// Shape normal volume from rectangular to square
		Vector3 volumeSize = new Vector3(effectiveSize.x, effectiveSize.y, voxelDepth);
		Vector3 objectCenter = effectiveSize * 0.5f;
		float size = Mathf.Max(volumeSize.x, volumeSize.y);
		volumeSize = new Vector3(size, size, volumeSize.z);

		Vector3 v = vertex - objectCenter + (volumeSize * 0.5f);
		Vector3 normalizedVertex = new Vector3(v.x / volumeSize.x, v.y / volumeSize.y, v.z / volumeSize.z);
		Vector3 n = kVecBottomLeft + Vector3.Scale(normalizedVertex, kVecDeltaNormal);
		n /= gameObject.transform.localScale.x;

		return n;
	}

	public void rebuildObject()
	{
		verticeList.Clear();
		tri.Clear();

		effectiveSize = new Vector3(0, 0, voxelDepth);
		Vector3 scale = gameObject.transform.localScale;
		Debug.Assert(scale.x == scale.y && scale.y == scale.z, gameObject.name + " needs a uniform model-View scale to support batching!");

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;

		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;
		float atlasSubImageRectX2 = (float)(startPixelX + subImageWidth - 0.5) / texture.width; 
		float atlasSubImageRectY2 = (float)(startPixelY + subImageHeight - 0.5) / texture.height;
		uvAtlasSubImageRectEncoded = new Vector2(startPixelX + atlasSubImageRectX2, startPixelY + atlasSubImageRectY2);

		if (quadCountX) {
			float deltaX = subImageWidth / Mathf.Max(1, subImageWidth - 1);
			for (int x = 0; x <= subImageWidth; ++x) {
				Vector2 singleFaceCount = countSingleFacesForCol(x);
				if (singleFaceCount.x > 0)
					createLeftQuad(x * deltaX);
				if (singleFaceCount.y > 0)
					createRightQuad(x * deltaX);
			}
		}

		if (quadCountY) {
			float deltaY = subImageHeight / Mathf.Max(1, subImageHeight - 1);
			for (int y = 0; y <= subImageHeight; ++y) {
				Vector2 singleFaceCount = countSingleFacesForRow(y);
				if (singleFaceCount.x > 0)
					createBottomQuad(y * deltaY);
				if (singleFaceCount.y > 0)
					createTopQuad(y * deltaY);
			}
		}

		float deltaZ = voxelDepth / Mathf.Max(1, quadCountZ - 1);
		for (int z = 0; z < quadCountZ - 1; ++z)
			createFrontQuad(z * deltaZ);
		createBackQuad((quadCountZ - 1) * deltaZ);

		Mesh mesh = new Mesh();
		mesh.vertices = verticeList.ToArray();
		mesh.triangles = tri.ToArray();

		// When using object batching, local vertices and normals will be translated on the CPU before
		// passed down to the GPU. We therefore loose the original values in the shader, which we need.
		// We therefore encode this information covered as vertex color.
		// Note: Several places I pass down two different pieces of information using a single float
		// where the integer part represents the first piece, and the fraction the second.
		int vertexCount = mesh.vertices.Length;
		Color[] cubeDesc = new Color[vertexCount];
		Vector3[] normals = new Vector3[vertexCount];

		for (int i = 0; i < vertexCount; ++i) {
			Vector3 v = mesh.vertices[i];
			float uvAtlasX = (startPixelX + v.x) / texture.width;
			float uvAtlasY = (startPixelY + v.y) / texture.height;

			int normalCode = v.z == 0 ? kNormalCodeFront : v.z == voxelDepth ? kNormalCodeBack : kNormalCodeMiddle;
			cubeDesc[i] = new Color(uvAtlasX, uvAtlasY, normalCode + (effectiveSize.x / (2 * subImageWidth)), (effectiveSize.y / (2 * subImageHeight)));
			normals[i] = getVolumeNormal(new Vector3(v.x, v.y, v.z));
		}

		Vector2[] uvAtlasSubImageRectArray = new Vector2[verticeList.Count];
		for (int i = 0; i < uvAtlasSubImageRectArray.Length; ++i)
			uvAtlasSubImageRectArray[i] = uvAtlasSubImageRectEncoded;
		
		mesh.uv = uvAtlasSubImageRectArray;
		mesh.colors = cubeDesc;
		mesh.normals = normals;

		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		readonlyVertexCount = mesh.vertices.Length;
		readonlyTriangleCount = tri.Count / 3;
	}

	Vector2 countSingleFacesForCol(int x)
	{
		Vector2 faceCount = new Vector2();
		for (int y = 0; y < subImageHeight; ++y) {
			Color c1 = (x == subImageWidth) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + y);
			Color c2 = (x == 0) ? Color.clear : texture.GetPixel(startPixelX + x - 1, startPixelY + y);
			if (c1.a == c2.a)
				continue;

			if (c1.a != 0)
				faceCount.x = faceCount.x + 1;
			else
				faceCount.y = faceCount.y + 1;
		}

		return faceCount;
	}

	Vector2 countSingleFacesForRow(int y)
	{
		Vector2 faceCount = new Vector2();
		for (int x = 0; x < subImageWidth; ++x) {
			Color c1 = (y == subImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + y);
			Color c2 = (y == 0) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + y - 1);
			if (c1.a == c2.a)
				continue;

			if (c1.a != 0)
				faceCount.x = faceCount.x + 1;
			else
				faceCount.y = faceCount.y + 1;
		}

		return faceCount;
	}

	int findFirstVoxelAlphaTest(int startX, int startY, int alpha)
	{
		for (int x = startX; x < subImageWidth; ++x) {
			Color c = texture.GetPixel(x, startY);
			if (Mathf.CeilToInt(c.a) == alpha)
				return x;
		}
		return kVoxelNotFound;
	}

	int createVertex(float x, float y, float z)
	{
		verticeList.Add(new Vector3(x, y, z));
		effectiveSize.x = Mathf.Max(effectiveSize.x, x);
		effectiveSize.y = Mathf.Max(effectiveSize.y, y);
		return verticeList.Count - 1;
	}

	void createLeftQuad(float x)
	{
		int index0 = createVertex(x + planeOffset, 0, voxelDepth);
		int index1 = createVertex(x + planeOffset, subImageHeight, voxelDepth);
		int index2 = createVertex(x + planeOffset, 0, 0);
		int index3 = createVertex(x + planeOffset, subImageHeight, 0);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createRightQuad(float x)
	{
		int index0 = createVertex(x - planeOffset, 0, 0);
		int index1 = createVertex(x - planeOffset, subImageHeight, 0);
		int index2 = createVertex(x - planeOffset, 0, voxelDepth);
		int index3 = createVertex(x - planeOffset, subImageHeight, voxelDepth);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createBottomQuad(float y)
	{
		int index0 = createVertex(0, y + planeOffset, voxelDepth);
		int index1 = createVertex(0, y + planeOffset, 0);
		int index2 = createVertex(subImageWidth, y + planeOffset, voxelDepth);
		int index3 = createVertex(subImageWidth, y + planeOffset, 0);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createTopQuad(float y)
	{
		int index0 = createVertex(0, y - planeOffset, 0);
		int index1 = createVertex(0, y - planeOffset, voxelDepth);
		int index2 = createVertex(subImageWidth, y - planeOffset, 0);
		int index3 = createVertex(subImageWidth, y - planeOffset, voxelDepth);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createFrontQuad(float z)
	{
		int index0 = createVertex(0, 0, z);
		int index1 = createVertex(0, subImageHeight, z);
		int index2 = createVertex(subImageWidth, 0, z);
		int index3 = createVertex(subImageWidth, subImageHeight, z);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createBackQuad(float z)
	{
		int index0 = createVertex(subImageWidth, 0, z);
		int index1 = createVertex(subImageWidth, subImageHeight, z);
		int index2 = createVertex(0, 0, z);
		int index3 = createVertex(0, subImageHeight, z);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}
}
