using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using FaceDirection = System.Int32;

public class VoxelQuadScript : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 1;

	public bool quadCountX = false;
	public bool quadCountY = false;
	public bool dominatingX = true;
	public bool dominatingY = true;

	public int quadCountZ = 4;
	public float planeOffset = 0;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;
	public int readonlyTriangleCount = 0;

	Texture2D texture;
	Vector3 effectiveSize;

	int startPixelX;
	int startPixelY;
	const int subImageWidth = 16;
	const int subImageHeight = 8;
	Rect effectiveRect;

	static List<Vector3> verticeList = new List<Vector3>(); 
	static List<int> tri = new List<int>(); 
	static List<FaceDirection> faceDirectionList = new List<FaceDirection>(); 
	static List<Vector2> uvList = new List<Vector2>(); 

	static Vector3 kVecBottomLeft = new Vector3(-1, -1, -1);
	static Vector3 kVecDeltaNormal = new Vector3(2, 2, 2);

	const int kNotFound = -1;

	const FaceDirection kFaceUnknown = 0;
	const FaceDirection kFaceLeft = 1;
	const FaceDirection kFaceRight = 2;
	const FaceDirection kFaceBottom = 4;
	const FaceDirection kFaceTop = 8;
	const FaceDirection kFaceFront = 16;
	const FaceDirection kFaceBack = 32;
	const FaceDirection kFaceMiddle = 64;

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
		faceDirectionList.Clear();
		uvList.Clear();

		effectiveSize = new Vector3(0, 0, voxelDepth);
		Vector3 scale = gameObject.transform.localScale;
		Debug.Assert(scale.x == scale.y && scale.y == scale.z, gameObject.name + " needs a uniform model-View scale to support batching!");

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;

		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;
		effectiveRect = calculateEffectiveRect();

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

		if (dominatingX) {
			int bestColLeft = kNotFound;
			int bestColRight = kNotFound;
			int x2 = (int)effectiveRect.x + (int)(effectiveRect.width / 2);
			if (gameObject.name == "VoxelVolume")
				print(effectiveRect.x + ", " + x2);

			int bestCount = 0;
			for (int x = (int)effectiveRect.x; x <= x2; ++x) {
				int count = countPixelsForCol(x);
				if (count > bestCount) {
					bestColLeft = x;
					bestCount = count;
				}
			}

			bestCount = 0;
			for (int x = subImageWidth - 1; x >= x2; --x) {
				int count = countPixelsForCol(x);
				if (count > bestCount) {
					bestColRight = x;
					bestCount = count;
				}
			}

			if (bestColLeft != kNotFound)
				createLeftQuad(bestColLeft);
			if (bestColRight != kNotFound)
				createRightQuad(bestColRight + 1);
		}

		if (dominatingY) {
			int bestRowBottom = kNotFound;
			int bestRowTop = kNotFound;
			int y2 = (int)effectiveRect.y + (int)(effectiveRect.height / 2);

			int bestCount = 0;
			for (int y = (int)effectiveRect.y; y <= y2; ++y) {
				int count = countPixelsForRow(y);
				if (count > bestCount) {
					bestRowBottom = y;
					bestCount = count;
				}
			}

			bestCount = 0;
			for (int y = subImageHeight - 1; y >= y2; --y) {
				int count = countPixelsForRow(y);
				if (count > bestCount) {
					bestRowTop = y;
					bestCount = count;
				}
			}

			if (bestRowBottom != kNotFound)
				createBottomQuad(bestRowBottom);
			if (bestRowTop != kNotFound)
				createTopQuad(bestRowTop + 1);
		}

		float deltaZ = voxelDepth / Mathf.Max(1, quadCountZ - 1);
		for (int z = 0; z < quadCountZ - 1; ++z)
			createFrontQuad((z + planeOffset) * deltaZ);
		createBackQuad((quadCountZ - 1 - planeOffset) * deltaZ);

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

			float uvZ = v.z / (voxelDepth * 2);
			cubeDesc[i] = new Color(uvAtlasX, uvAtlasY, faceDirectionList[i] + uvZ, voxelDepth);
			normals[i] = getVolumeNormal(v);
		}

		mesh.uv = uvList.ToArray();
		mesh.colors = cubeDesc;
		mesh.normals = normals;

		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		readonlyVertexCount = mesh.vertices.Length;
		readonlyTriangleCount = tri.Count / 3;
	}

	Rect calculateEffectiveRect()
	{
		int x1 = 0;
		int y1 = 0;
		int x2 = 0;
		int y2 = 0;

		for (x1 = 0; x1 < subImageWidth; ++x1) {
			if (countPixelsForCol(x1) > 0)
				break;
		}

		for (x2 = subImageWidth; x2 > x1; --x2) {
			if (countPixelsForCol(x2 - 1) > 0)
				break;
		}

		for (y1 = 0; y1 < subImageHeight; ++y1) {
			if (countPixelsForRow(y1) > 0)
				break;
		}

		for (y2 = subImageHeight; y2 > y1; --y2) {
			if (countPixelsForRow(y2 - 1) > 0)
				break;
		}

		return new Rect(x1, y1, x2 - x1, y2 -y1);
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

	int countPixelsForCol(int x)
	{
		int count = 0;
		for (int y = 0; y < subImageHeight; ++y) {
			Color c1 = texture.GetPixel(startPixelX + x, startPixelY + y);
			if (c1.a != 0)
				++count;
		}

		return count;
	}

	int countPixelsForRow(int y)
	{
		int count = 0;
		for (int x = 0; x < subImageWidth; ++x) {
			Color c1 = texture.GetPixel(startPixelX + x, startPixelY + y);
			if (c1.a != 0)
				++count;
		}

		return count;
	}

	int createVertex(float x, float y, float z, FaceDirection faceDirection, float uvShiftX = 0, float uvShiftY = 0, Vector2? uv = null)
	{
		if (uv == null) {
			float uvAtlasX = (startPixelX + x + uvShiftX) / texture.width;
			float uvAtlasY = (startPixelY + y + uvShiftY) / texture.height;
			uv = new Vector2(uvAtlasX, uvAtlasY);
		}

		verticeList.Add(new Vector3(x, y, z));
		uvList.Add((Vector2)uv);
		faceDirectionList.Add(faceDirection);
		effectiveSize.x = Mathf.Max(effectiveSize.x, x);
		effectiveSize.y = Mathf.Max(effectiveSize.y, y);

		return verticeList.Count - 1;
	}

	void createLeftQuad(float x)
	{
		float y1 = effectiveRect.y;
		float y2 = effectiveRect.y + effectiveRect.height;

		int index0 = createVertex(x, y1, voxelDepth, kFaceLeft, 0.5f);
		int index1 = createVertex(x, y2, voxelDepth, kFaceLeft, 0.5f);
		int index2 = createVertex(x, y1, 0, kFaceLeft, 0.5f);
		int index3 = createVertex(x, y2, 0, kFaceLeft, 0.5f);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createRightQuad(float x)
	{
		float y1 = effectiveRect.y;
		float y2 = effectiveRect.y + effectiveRect.height;

		int index0 = createVertex(x, y1, 0, kFaceRight, -0.5f);
		int index1 = createVertex(x, y2, 0, kFaceRight, -0.5f);
		int index2 = createVertex(x, y1, voxelDepth, kFaceRight, -0.5f);
		int index3 = createVertex(x, y2, voxelDepth, kFaceRight, -0.5f);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createBottomQuad(float y)
	{
		float x1 = effectiveRect.x;
		float x2 = effectiveRect.x + effectiveRect.width;

		int index0 = createVertex(x1, y, voxelDepth, kFaceBottom, 0, 0.5f);
		int index1 = createVertex(x1, y, 0, kFaceBottom, 0, 0.5f);
		int index2 = createVertex(x2, y, voxelDepth, kFaceBottom, 0, 0.5f);
		int index3 = createVertex(x2, y, 0, kFaceBottom, 0, 0.5f);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createTopQuad(float y)
	{
		float x1 = effectiveRect.x;
		float x2 = effectiveRect.x + effectiveRect.width;

		int index0 = createVertex(x1, y, 0, kFaceTop, 0, -0.5f);
		int index1 = createVertex(x1, y, voxelDepth, kFaceTop, 0, -0.5f);
		int index2 = createVertex(x2, y, 0, kFaceTop, 0, -0.5f);
		int index3 = createVertex(x2, y, voxelDepth, kFaceTop, 0, -0.5f);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createFrontQuad(float z, FaceDirection faceDirection = kFaceFront)
	{
		float x1 = effectiveRect.x;
		float y1 = effectiveRect.y;
		float x2 = effectiveRect.x + effectiveRect.width;
		float y2 = effectiveRect.y + effectiveRect.height;

		int index0 = createVertex(x1, y1, z, faceDirection);
		int index1 = createVertex(x1, y2, z, faceDirection);
		int index2 = createVertex(x2, y1, z, faceDirection);
		int index3 = createVertex(x2, y2, z, faceDirection);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createBackQuad(float z)
	{
		float x1 = effectiveRect.x;
		float y1 = effectiveRect.y;
		float x2 = effectiveRect.x + effectiveRect.width;
		float y2 = effectiveRect.y + effectiveRect.height;

		int index0 = createVertex(x2, y1, z, kFaceBack);
		int index1 = createVertex(x2, y2, z, kFaceBack);
		int index2 = createVertex(x1, y1, z, kFaceBack);
		int index3 = createVertex(x1, y2, z, kFaceBack);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

}
