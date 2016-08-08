using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NormalCode = System.Int32;

public class VoxelCubesScript : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 4;

	public bool volume = false;
	public bool xFaces = true;
	public bool yFaces = true;
	public bool zFaces = true;
	public bool dominatingXFace = true;
	public bool dominatingYFace = true;
	public bool cubify = false;

	public bool shareVertices = true;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;
	public int readonlyTriangleCount = 0;

	Texture2D texture;
	int startPixelX;
	int startPixelY;
	Rect effectiveRect;

	const int subImageWidth = 16;
	const int subImageHeight = 8;

	static List<Vector3> verticeList = new List<Vector3>(); 
	static List<Vector2> vertexPixelList = new List<Vector2>(); 
	static List<int> normalCodeList = new List<int>(); 
	static List<int> tri = new List<int>(); 

	static Vector3 kVecBottomLeft = new Vector3(-1, -1, -1);
	static Vector3 kVecDeltaNormal = new Vector3(2, 2, 2);

	const int kNotFound = -1;

	const NormalCode kLeft = 0;
	const NormalCode kRight = 1;
	const NormalCode kBottom = 2;
	const NormalCode kTop = 3;
	const NormalCode kFront = 4;
	const NormalCode kBack = 5;
	const NormalCode kFrontBottomLeft = 6;
	const NormalCode kFrontTopLeft = 7;
	const NormalCode kFrontBottomRight = 8;
	const NormalCode kFrontTopRight = 9;
	const NormalCode kBackBottomLeft = 10;
	const NormalCode kBackTopLeft = 11;
	const NormalCode kBackBottomRight = 12;
	const NormalCode kBackTopRight = 13;

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
		Vector3 volumeSize = new Vector3(effectiveRect.width, effectiveRect.height, voxelDepth);
		Vector3 objectCenter = new Vector3(effectiveRect.width * 0.5f, effectiveRect.height * 0.5f, voxelDepth / 2);
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
		vertexPixelList.Clear();
		normalCodeList.Clear();
		tri.Clear();

		effectiveRect = new Rect(0, 0, 0, 0);
		Vector3 scale = gameObject.transform.localScale;
		Debug.Assert(scale.x == scale.y && scale.y == scale.z, gameObject.name + " needs a uniform model-View scale to support batching!");

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;

		// Caluclate uv coords based on atlasIndex. Note that we don't assign any uv coords to the
		// verticeList, since those can be calculated directly (and more precisely) in the shader
		// based on the local position of the vertices themselves.
		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;
		effectiveRect = calculateEffectiveRect();

		// Traverse each row in the texture
		if (xFaces) {
			for (int x = 0; x < subImageWidth; ++x) {
				createFacesForX(x, kLeft);
				createFacesForX(x, kRight);
			}
		}

		if (yFaces) {
			for (int y = 0; y < subImageHeight; ++y) {
				createFacesForY(y, kBottom);
				createFacesForY(y, kTop);
			}
		}

		if (zFaces)
			createFacesForZ();

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

			// Ensure uvSubImageEffectiveWidth ends up as a fraction, so make the range go from 0 - 0.5
			int normalCode = normalCodeList[i];
			cubeDesc[i] = new Color(uvAtlasX, uvAtlasY, normalCode, voxelDepth);
			normals[i] = getVolumeNormal(v);
		}

		mesh.uv = vertexPixelList.ToArray();
		mesh.colors = cubeDesc;
		mesh.normals = normals;

		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = mesh;

		readonlyVertexCount = mesh.vertices.Length;
		readonlyTriangleCount = tri.Count / 3;
	}

	bool normalCodeIsExclusive(NormalCode n)
	{
		return n == kLeft || n == kRight || n == kBottom || n == kTop || n == kFront || n == kBack;
	}

	bool isFace(int x1, int y, int x2)
	{
		// Returns true if the given coords maps to a separate pixel strip in the atlas
		if (x1 > 0 && texture.GetPixel(startPixelX + x1 - 1, startPixelY + y).a != 0)
			return false;

		if (x2 < subImageWidth - 1 && texture.GetPixel(startPixelX + x2 + 1, startPixelY + y).a != 0)
			return false;

		for (int x = x1; x <= x2; ++x) {
			if (texture.GetPixel(startPixelX + x, startPixelY + y).a == 0)
				return false;
		}

		return true;
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

	int getFirstFaceForX(int startX, int startY, NormalCode face, bool searchForVisible)
	{
		for (int y = startY; y < subImageHeight; ++y) {
			Color c1 = (startX == subImageWidth) ? Color.clear : texture.GetPixel(startPixelX + startX, startPixelY + y);
			Color c2 = (startX == 0) ? Color.clear : texture.GetPixel(startPixelX + startX - 1, startPixelY + y);

			if (searchForVisible) {
				if (face == kLeft && c1.a == 1 && c2.a == 0)
					return y;
				if (face == kRight && c1.a == 0 && c2.a == 1)
					return y;
			} else {
				if (face == kLeft && (c1.a == c2.a || c1.a == 0))
					return y;
				if (face == kRight && (c1.a == c2.a || c2.a == 0))
					return y;
			}
		}

		return kNotFound;
	}

	int getFirstFaceForY(int startX, int startY, NormalCode face, bool searchForVisible)
	{
		for (int x = startX; x < subImageWidth; ++x) {
			Color c1 = (startY == subImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY);
			Color c2 = (startY == 0) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY - 1);

			if (searchForVisible) {
				if (face == kBottom && c1.a == 1 && c2.a == 0)
					return x;
				if (face == kTop && c1.a == 0 && c2.a == 1)
					return x;
			} else {
				if (face == kBottom && (c1.a == c2.a || c1.a == 0))
					return x;
				if (face == kTop && (c1.a == c2.a || c2.a == 0))
					return x;
			}
		}

		return kNotFound;
	}

	int getFirstFaceForZ(int startX, int startY, bool searchForVisible)
	{
		for (int x = startX; x < subImageWidth; ++x) {
			Color c = texture.GetPixel(startPixelX + x, startPixelY + startY);
			if (searchForVisible && Mathf.CeilToInt(c.a) == 1)
				return x;
			if (!searchForVisible && Mathf.CeilToInt(c.a) == 0)
				return x;
		}
		return kNotFound;
	}

	void createFacesForX(int x, NormalCode face)
	{
		int y2 = -1;
		int faceShift = (face == kLeft) ? 0 : 1;
		while (y2 != subImageHeight) {
			int y1 = getFirstFaceForX(x + faceShift, y2 + 1, face, true);
			if (y1 == kNotFound)
				return;

			y2 = getFirstFaceForX(x + faceShift, y1 + 1, face, false);
			if (y2 == kNotFound)
				y2 = subImageHeight;

			if (face == kLeft)
				createLeftFace(x, y1, y2 - 1);
			else
				createRightFace(x, y1, y2 - 1);
		}
	}

	void createFacesForY(int y, NormalCode face)
	{
		int x2 = -1;
		int faceShift = (face == kBottom) ? 0 : 1;
		while (x2 != subImageWidth) {
			int x1 = getFirstFaceForY(x2 + 1, y + faceShift, face, true);
			if (x1 == kNotFound)
				return;

			x2 = getFirstFaceForY(x1 + 1, y + faceShift, face, false);
			if (x2 == kNotFound)
				x2 = subImageWidth;

			if (face == kBottom)
				createBottomFace(x1, y, x2 - 1);
			else
				createTopFace(x1, y, x2 - 1);
		}
	}

	void createFacesForZ()
	{
		for (int y1 = 0; y1 < subImageHeight; ++y1) {
			int x2 = -1;

			while (x2 != subImageWidth) {
				int x1 = getFirstFaceForZ(x2 + 1, y1, true);
				if (x1 == kNotFound) {
					x2 =  subImageWidth;
					continue;
				}

				x2 = getFirstFaceForZ(x1 + 1, y1, false);
				if (x2 == kNotFound)
					x2 = subImageWidth;

				if (y1 > 0 && isFace(x1, y1 - 1, x2 - 1))
					continue;

				int y2 = y1;
				while (y2 < subImageHeight - 1 && isFace(x1, y2 + 1, x2 - 1))
					++y2;
				
				createFrontFace(x1, y1, x2 - 1, y2);
				createBackFace(x1, y1, x2 - 1, y2);
			}
		}
	}

	int getVertexIndex(Vector3 v, Vector2 pixel, NormalCode normalCode)
	{
		if (normalCodeIsExclusive(normalCode)) {
			// Cannot share vertices that are meant to be exclusive
			return kNotFound;
		}

		int i = verticeList.FindIndex(v2 => v2 == v);
		if (i == kNotFound)
			return kNotFound;

		if (vertexPixelList[i] != pixel)
			return kNotFound;

		if (normalCodeIsExclusive(normalCodeList[i]))
			return kNotFound;

		return i;
	}

	int createVertex(float x, float y, float z, Vector2 pixel, NormalCode normalCode)
	{
		Vector3 v = new Vector3(x, y, z);

		if (shareVertices) {
			int index = getVertexIndex(v, pixel, normalCode);
			if (index != kNotFound)
				return index;
		}

		verticeList.Add(v);
		normalCodeList.Add(normalCode);
		vertexPixelList.Add(pixel);

		return verticeList.Count - 1;
	}

	void createLeftFace(int pixelX, int pixelY1, int pixelY2)
	{
		Vector2 pixelBottom = new Vector2(startPixelX + pixelX, startPixelY + pixelY1);
		Vector2 pixelTop = new Vector2(startPixelX + pixelX, startPixelY + pixelY2);

		int index0 = createVertex(pixelX, pixelY1, 0, pixelBottom, kFrontBottomLeft);
		int index1 = createVertex(pixelX, pixelY2 + 1, 0, pixelTop, kFrontTopLeft);
		int index4 = createVertex(pixelX, pixelY1, voxelDepth, pixelBottom, kBackBottomLeft);
		int index5 = createVertex(pixelX, pixelY2 + 1, voxelDepth, pixelTop, kBackTopLeft);

		tri.Add(index4);
		tri.Add(index5);
		tri.Add(index0);
		tri.Add(index0);
		tri.Add(index5);
		tri.Add(index1);
	}

	void createRightFace(int pixelX, int pixelY1, int pixelY2)
	{
		Vector2 pixelBottom = new Vector2(startPixelX + pixelX, startPixelY + pixelY1);
		Vector2 pixelTop = new Vector2(startPixelX + pixelX, startPixelY + pixelY2);

		int index2 = createVertex(pixelX + 1, pixelY1, 0, pixelBottom, kFrontBottomRight);
		int index3 = createVertex(pixelX + 1, pixelY2 + 1, 0, pixelTop, kFrontTopRight);
		int index6 = createVertex(pixelX + 1, pixelY1, voxelDepth, pixelBottom, kBackBottomRight);
		int index7 = createVertex(pixelX + 1, pixelY2 + 1, voxelDepth, pixelTop, kBackTopRight);

		tri.Add(index2);
		tri.Add(index3);
		tri.Add(index6);
		tri.Add(index6);
		tri.Add(index3);
		tri.Add(index7);
	}

	void createBottomFace(int pixelX1, int pixelY, int pixelX2)
	{
		Vector2 pixelLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY);
		Vector2 pixelRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY);

		int index0 = createVertex(pixelX1, pixelY, 0, pixelLeft, kFrontBottomLeft);
		int index2 = createVertex(pixelX2 + 1, pixelY, 0, pixelRight, kFrontBottomRight);
		int index4 = createVertex(pixelX1, pixelY, voxelDepth, pixelLeft, kBackBottomLeft);
		int index6 = createVertex(pixelX2 + 1, pixelY, voxelDepth, pixelRight, kBackBottomRight);

		tri.Add(index4);
		tri.Add(index0);
		tri.Add(index6);
		tri.Add(index6);
		tri.Add(index0);
		tri.Add(index2);
	}

	void createTopFace(int pixelX1, int pixelY, int pixelX2)
	{
		Vector2 pixelLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY);
		Vector2 pixelRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY);

		int index1 = createVertex(pixelX1, pixelY + 1, 0, pixelLeft, kFrontTopLeft);
		int index3 = createVertex(pixelX2 + 1, pixelY + 1, 0, pixelRight, kFrontTopRight);
		int index5 = createVertex(pixelX1, pixelY + 1, voxelDepth, pixelLeft, kBackTopLeft);
		int index7 = createVertex(pixelX2 + 1, pixelY + 1, voxelDepth, pixelRight, kBackTopRight);

		tri.Add(index1);
		tri.Add(index5);
		tri.Add(index3);
		tri.Add(index3);
		tri.Add(index5);
		tri.Add(index7);
	}

	void createFrontFace(float pixelX1, float pixelY1, float pixelX2, float pixelY2)
	{
		Vector2 pixelBottomLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY1);
		Vector2 pixelBottomRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY1);
		Vector2 pixelTopLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY2);
		Vector2 pixelTopRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY2);

		int index0 = createVertex(pixelX1, pixelY1, 0, pixelBottomLeft, kFrontBottomLeft);
		int index1 = createVertex(pixelX1, pixelY2 + 1, 0, pixelTopLeft, kFront);
		int index2 = createVertex(pixelX2 + 1, pixelY1, 0, pixelBottomRight, kFrontBottomRight);
		int index3 = createVertex(pixelX2 + 1, pixelY2 + 1, 0, pixelTopRight, kFrontTopRight);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createBackFace(float pixelX1, float pixelY1, float pixelX2, float pixelY2)
	{
		Vector2 pixelBottomLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY1);
		Vector2 pixelBottomRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY1);
		Vector2 pixelTopLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY2);
		Vector2 pixelTopRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY2);

		int index4 = createVertex(pixelX1, pixelY1, voxelDepth, pixelBottomLeft, kBackBottomLeft);
		int index5 = createVertex(pixelX1, pixelY2 + 1, voxelDepth, pixelTopLeft, kBackTopLeft);
		int index6 = createVertex(pixelX2 + 1, pixelY1, voxelDepth, pixelBottomRight, kBackBottomRight);
		int index7 = createVertex(pixelX2 + 1, pixelY2 + 1, voxelDepth, pixelTopRight, kBack);

		tri.Add(index6);
		tri.Add(index7);
		tri.Add(index4);
		tri.Add(index4);
		tri.Add(index7);
		tri.Add(index5);
	}
}
