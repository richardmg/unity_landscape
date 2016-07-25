using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NormalCode = System.Int32;

public class VoxelQuadScript : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 1;
	public int quadCount = 4;
	public bool includeVoxelDepthInNormalVolume = false;

	// tile means draw texture on all sides of object rather that just in front
	public bool tile = false;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;
	public int readonlyTriangleCount = 0;

	Texture2D texture;
	int startPixelX;
	int startPixelY;

	Vector3 effectiveSize;

	const int subImageWidth = 16;
	const int subImageHeight = 8;

	static List<Vector3> verticeList = new List<Vector3>(); 
	static List<Vector2> uvAtlasCubeRectEncodedList = new List<Vector2>(); 
	static List<int> tri = new List<int>(); 
	static List<NormalCode> normalCodeList = new List<NormalCode>(); 

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

	Vector3 getVolumeNormal(Vector3 vertex, Vector3 objectCenter, Vector3 volumeSize)
	{
		Vector3 v = vertex - objectCenter + (volumeSize * 0.5f);
		Vector3 normalizedVertex = new Vector3(v.x / volumeSize.x, v.y / volumeSize.y, v.z / volumeSize.z);
		Vector3 n = kVecBottomLeft + Vector3.Scale(normalizedVertex, kVecDeltaNormal);
		n /= gameObject.transform.localScale.x;
		return n;
	}

	public void rebuildObject()
	{
		verticeList.Clear();
		uvAtlasCubeRectEncodedList.Clear();
		tri.Clear();
		normalCodeList.Clear();

		effectiveSize = new Vector3(0, 0, voxelDepth);
		Vector3 scale = gameObject.transform.localScale;
		Debug.Assert(scale.x == scale.y && scale.y == scale.z, gameObject.name + " needs a uniform model-View scale to support batching!");

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;

		// Caluclate uv coords based on atlasIndex. Note that we don't assign any uv coords to the
		// verticeList, since those can be calculated directly (and more precisely) in the shader
		// based on the local position of the vertices themselves.
		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;

		if (quadCount > 0)
			createVerticalPyramid(0, kNormalCodeFront);
		if (quadCount > 1)
			createVerticalPyramid(voxelDepth, kNormalCodeBack);
		float pyramidDelta = voxelDepth / (quadCount - 1);
		for (int i = 1; i < quadCount - 1; ++i)
			createVerticalPyramid(pyramidDelta * i, kNormalCodeMiddle);

		Vector3 volumeSize = new Vector3(effectiveSize.x, effectiveSize.y, voxelDepth);
		Vector3 objectCenter = effectiveSize * 0.5f;

		// Shape normal volume from rectangular to square
		float size = Mathf.Max(volumeSize.x, volumeSize.y);
		volumeSize = new Vector3(size, size, volumeSize.z);

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

			int normalCode = normalCodeList[i];
			cubeDesc[i] = new Color(uvAtlasX, uvAtlasY, normalCode + (effectiveSize.x / (2 * subImageWidth)), (effectiveSize.y / (2 * subImageHeight)));
			normals[i] = getVolumeNormal(new Vector3(v.x, v.y, v.z), objectCenter, volumeSize);
		}

		mesh.uv = uvAtlasCubeRectEncodedList.ToArray();
		mesh.colors = cubeDesc;
		mesh.normals = normals;

		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		readonlyVertexCount = mesh.vertices.Length;
		readonlyTriangleCount = tri.Count / 3;
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

	int createVertex(float x, float y, float z, Vector2 uvRect, NormalCode normalCode)
	{
		verticeList.Add(new Vector3(x, y, z));
		uvAtlasCubeRectEncodedList.Add(uvRect);
		normalCodeList.Add(normalCode);

		effectiveSize.x = Mathf.Max(effectiveSize.x, x);
		effectiveSize.y = Mathf.Max(effectiveSize.y, y);

		return verticeList.Count - 1;
	}

	void createVerticalPyramid(float z, NormalCode normalCode)
	{
		int atlasCubeRectX1 = (int)(startPixelX);
		int atlasCubeRectY1 = (int)(startPixelY);
		float atlasCubeRectX2 = (float)(startPixelX + subImageWidth - 0.5) / texture.width; 
		float atlasCubeRectY2 = (float)(startPixelY + subImageHeight - 0.5) / texture.height;
		Vector2 uvAtlasCubeRectEncoded = new Vector2(atlasCubeRectX1 + atlasCubeRectX2, atlasCubeRectY1 + atlasCubeRectY2);

		int index0 = createVertex(0, 0, z, uvAtlasCubeRectEncoded, normalCode);
		int index1 = createVertex(0, subImageHeight, z, uvAtlasCubeRectEncoded, normalCode);
		int index2 = createVertex(subImageWidth, 0, z, uvAtlasCubeRectEncoded, normalCode);
		int index3 = createVertex(subImageWidth, subImageHeight, z, uvAtlasCubeRectEncoded, normalCode);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}
}
