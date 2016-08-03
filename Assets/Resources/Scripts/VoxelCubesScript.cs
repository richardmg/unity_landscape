using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelCubesScript : MonoBehaviour {
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public float cascade = 0.0f;
	public bool includeVoxelDepthInNormalVolume = false;
	public bool drawXFaces = true;
	public bool drawYFaces = true;
	public bool drawZFaces = true;
	public bool fillHoles = false;
	public bool oneCubePerRow = false; // Fill inner holes, only keep longest line on the outside
	public bool oneCubePerObject = false;
	public bool shareVertices = true;

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
	static List<Vector2> vertexPixelList = new List<Vector2>(); 
	static List<int> normalCodeList = new List<int>(); 
	static List<int> tri = new List<int>(); 

	static Vector3 kVecBottomLeft = new Vector3(-1, -1, -1);
	static Vector3 kVecDeltaNormal = new Vector3(2, 2, 2);

	const int kNotFound = -1;

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

	public void Update()
	{
		transform.Rotate(new Vector3(0, 0.2f, 0));
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
		vertexPixelList.Clear();
		normalCodeList.Clear();
		tri.Clear();

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

		// Traverse each row in the texture
		if (drawXFaces) {
			for (int x = 0; x < subImageWidth; ++x) {
				createFacesForX(x, kLeft);
				createFacesForX(x, kRight);
			}
		}

		if (drawYFaces) {
			for (int y = 0; y < subImageHeight; ++y) {
				createFacesForY(y, kBottom);
				createFacesForY(y, kTop);
			}
		}

		if (drawZFaces)
			createFacesForZ();

		Vector3 volumeSize = new Vector3(effectiveSize.x, effectiveSize.y, voxelDepth);
		Vector3 objectCenter = effectiveSize * 0.5f;

		if (voxelDepth == 0) {
			volumeSize.z = 1;
			objectCenter.z = 0.5f;
		}

		float size = Mathf.Max(volumeSize.x, volumeSize.y);
		if (includeVoxelDepthInNormalVolume) {
			// Gives bad result for small voxel depths
			size = Mathf.Max(size, volumeSize.z);
			volumeSize = new Vector3(size, size, size);
		} else {
			volumeSize = new Vector3(size, size, volumeSize.z);
		}

		// Add a small offset to center, to not fall exactly between two pixels
//		objectCenter -= new Vector3((objectCenter.x % 2 == 0) ? 0.5f : 0, (objectCenter.y % 2 == 0) ? 0.5f : 0, 0);

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
			cubeDesc[i] = new Color(uvAtlasX, uvAtlasY, normalCode + (effectiveSize.x / (2 * subImageWidth)), (int)(voxelDepth * 100) + (effectiveSize.y / (2 * subImageHeight)));

			if (voxelDepth == 0) {
				if (normalCode == kBack || (normalCode >= kBackBottomLeft && normalCode <= kBackTopRight))
					normals[i] = getVolumeNormal(new Vector3(v.x, v.y, 1), objectCenter, volumeSize);
				else
					normals[i] = getVolumeNormal(v, objectCenter, volumeSize);
			} else {
				normals[i] = getVolumeNormal(v, objectCenter, volumeSize);
			}
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

	int getFirstFaceForX(int startX, int startY, int face, bool searchForVisible)
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

	int getFirstFaceForY(int startX, int startY, int face, bool searchForVisible)
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

	void createFacesForX(int x, int face)
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

	void createFacesForY(int y, int face)
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

	bool isFace(int x1, int y, int x2)
	{
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

	void createFacesForZ()
	{
		for (int y = 0; y < subImageHeight; ++y) {
			int x2 = -1;

			// Traverse each column in the texture and look for voxel strips
			while (x2 != subImageWidth) {
				int x1 = getFirstFaceForZ(x2 + 1, y, true);
				if (x1 == kNotFound) {
					x2 =  subImageWidth;
					continue;
				}

				x2 = getFirstFaceForZ(x1 + 1, y, false);
				if (x2 == kNotFound)
					x2 = subImageWidth;

				if (y == 0 || !isFace(x1, y - 1, x2 - 1)) {
					createFrontFace(x1, y, x2 - 1, y);
					createBackFace(x1, y, x2 - 1, y);
				}
			}
		}
	}

	bool normalCodeIsExclusive(int n)
	{
		return n == kLeft || n == kRight || n == kBottom || n == kTop || n == kFront || n == kBack;
	}

	int getVertexIndex(Vector3 v, Vector2 pixel, int normalCode)
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

	int createVertex(float x, float y, float z, Vector2 pixel, int normalCode)
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

		effectiveSize.x = Mathf.Max(effectiveSize.x, x);
		effectiveSize.y = Mathf.Max(effectiveSize.y, y);

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
