using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelFrontScript : MonoBehaviour {
	public int atlasIndex = 0;
	public float border = 0;

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
	static List<int> normalCodeList = new List<int>(); 
	static List<int> tri = new List<int>(); 

	static Vector3 kVecBottomLeft = new Vector3(-1, -1, -1);
	static Vector3 kVecDeltaNormal = new Vector3(2, 2, 2);

	const int kLeft = 0;
	const int kRight = 1;
	const int kBottom = 2;
	const int kTop = 3;
	const int kFront = 4;
	const int kBack = 5;
	const int kFrontBottomLeft = 6;
	const int kFrontTopLeft = 7;
	const int kFrontBottomRight = 8;
	const int kFrontTopRight = 9;
	const int kBackBottomLeft = 10;
	const int kBackTopLeft = 11;
	const int kBackBottomRight = 12;
	const int kBackTopRight = 13;

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
		normalCodeList.Clear();
		tri.Clear();

		effectiveSize = new Vector3(0, 0, 1);
		Vector3 scale = gameObject.transform.localScale;

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;

		// Caluclate uv coords based on atlasIndex. Note that we don't assign any uv coords to the
		// verticeList, since those can be calculated directly (and more precisely) in the shader
		// based on the local position of the vertices themselves.
		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;

		if (border == 0)
			createQuadMesh(0, 0, subImageWidth - 1, subImageHeight - 1, 0, 0);
		else
			createQuadMesh(0, 0, subImageWidth - 1, subImageHeight - 1, -border, 0);

		Vector3 objectCenter = effectiveSize * 0.5f;
		float size = Mathf.Max(effectiveSize.x, effectiveSize.y);
		effectiveSize = new Vector3(size, size, effectiveSize.z);

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

//			Trenger å gjøre om på dette
			float uvAtlasX = (startPixelX + v.x) / texture.width;
			float uvAtlasY = (startPixelY + v.y) / texture.height;

			// Ensure uvSubImageEffectiveWidth ends up as a fraction, so make the range go from 0 - 0.5
			int normalCode = normalCodeList[i];
			cubeDesc[i] = new Color(uvAtlasX, uvAtlasY, normalCode + (effectiveSize.x / (2 * subImageWidth)), (effectiveSize.y / (2 * subImageHeight)));

			if (normalCode == kBack || (normalCode >= kBackBottomLeft && normalCode <= kBackTopRight))
				normals[i] = getVolumeNormal(new Vector3(v.x, v.y, 1), objectCenter, effectiveSize);
			else
				normals[i] = getVolumeNormal(v, objectCenter, effectiveSize);
		}

		mesh.uv = uvAtlasCubeRectEncodedList.ToArray();
		mesh.colors = cubeDesc;
		mesh.normals = normals;

		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		readonlyVertexCount = mesh.vertices.Length;
		readonlyTriangleCount = mesh.triangles.Length;
	}

	int createVertex(float x, float y, float z, Vector2 uvRect, int normalCode)
	{
		verticeList.Add(new Vector3(x, y, z));
		normalCodeList.Add(normalCode);
		uvAtlasCubeRectEncodedList.Add(uvRect);

		effectiveSize.x = Mathf.Max(effectiveSize.x, x);
		effectiveSize.y = Mathf.Max(effectiveSize.y, y);

		return verticeList.Count - 1;
	}

	void createQuadMesh(float voxelX1, float voxelY1, float voxelX2, float voxelY2, float offsetX, float offsetY)
	{
		float voxelZ1 = 0;

		int atlasCubeRectX1 = (int)(startPixelX + voxelX1);
		int atlasCubeRectY1 = (int)(startPixelY + voxelY1);
		float atlasCubeRectX2 = (float)(startPixelX + voxelX2 - 0.5) / texture.width; 
		float atlasCubeRectY2 = (float)(startPixelY + voxelY2 - 0.5) / texture.height;
		Vector2 uvAtlasCubeRectEncoded = new Vector2(atlasCubeRectX1 + atlasCubeRectX2, atlasCubeRectY1 + atlasCubeRectY2);

		int index0 = createVertex(voxelX1, voxelY1, voxelZ1, uvAtlasCubeRectEncoded, kFrontBottomLeft);
		int index1 = createVertex(voxelX1, voxelY2, voxelZ1, uvAtlasCubeRectEncoded, kFrontTopLeft);
		int index2 = createVertex(voxelX2, voxelY1, voxelZ1, uvAtlasCubeRectEncoded, kFrontBottomRight);
		int index3 = createVertex(voxelX2, voxelY2, voxelZ1, uvAtlasCubeRectEncoded, kFrontTopRight);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}
}
