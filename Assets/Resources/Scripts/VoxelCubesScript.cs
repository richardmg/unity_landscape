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

	// tile means draw texture on all sides of object rather that just in front
	public bool tile = false;

	// Read-only, for editor inspection
	public int readonlyVertexCount = 0;
	public int readonlyTriangleCount = 0;
	public int readonlyCubeCount = 0;

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
		uvAtlasCubeRectEncodedList.Clear();
		normalCodeList.Clear();
		tri.Clear();
		readonlyCubeCount = 0;

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
		return kNotFound;
	}

	int findLastVoxel(int startY)
	{
		for (int x = subImageWidth - 1; x >= 0; --x) {
			Color c = texture.GetPixel(startPixelX + x, startPixelY + startY);
			if (Mathf.CeilToInt(c.a) == 1.0)
				return x;
		}
		return kNotFound;
	}

	int getFirstVisibleFaceForX(int startX, int startY, int face)
	{
		for (int y = startY; y < subImageHeight; ++y) {
			Color c1 = (startX == subImageWidth) ? Color.clear : texture.GetPixel(startPixelX + startX, startPixelY + y);
			Color c2 = (startX == 0) ? Color.clear : texture.GetPixel(startPixelX + startX - 1, startPixelY + y);

			if (face == kLeft && c1.a == 1 && c2.a == 0)
				return y;
			if (face == kRight && c1.a == 0 && c2.a == 1)
				return y;
		}

		return kNotFound;
	}

	int getFirstHiddenFaceForX(int startX, int startY, int face)
	{
		for (int y = startY; y < subImageHeight; ++y) {
			Color c1 = (startX == subImageWidth) ? Color.clear : texture.GetPixel(startPixelX + startX, startPixelY + y);
			Color c2 = (startX == 0) ? Color.clear : texture.GetPixel(startPixelX + startX - 1, startPixelY + y);

			if (face == kLeft && (c1.a == c2.a || c1.a == 0))
				return y;
			if (face == kRight && (c1.a == c2.a || c2.a == 0))
				return y;
		}

		return kNotFound;
	}

	int getFirstVisibleFaceForY(int startX, int startY, int face)
	{
		for (int x = startX; x < subImageWidth; ++x) {
			Color c1 = (startY == subImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY);
			Color c2 = (startY == 0) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY - 1);

			if (face == kBottom && c1.a == 1 && c2.a == 0)
				return x;
			if (face == kTop && c1.a == 0 && c2.a == 1)
				return x;
		}

		return kNotFound;
	}

	int getFirstHiddenFaceForY(int startX, int startY, int face)
	{
		for (int x = startX; x < subImageWidth; ++x) {
			Color c1 = (startY == subImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY);
			Color c2 = (startY == 0) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY - 1);

			if (face == kBottom && (c1.a == c2.a || c1.a == 0))
				return x;
			if (face == kTop && (c1.a == c2.a || c2.a == 0))
				return x;
		}

		return kNotFound;
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

	Vector2 createCubeRect(float voxelX1, float voxelY1, float voxelX2, float voxelY2)
	{
		int atlasCubeRectX1 = (int)(startPixelX + voxelX1);
		int atlasCubeRectY1 = (int)(startPixelY + voxelY1);
		float atlasCubeRectX2 = (float)(startPixelX + voxelX2 - 0.5) / texture.width; 
		float atlasCubeRectY2 = (float)(startPixelY + voxelY2 - 0.5) / texture.height;
		return new Vector2(atlasCubeRectX1 + atlasCubeRectX2, atlasCubeRectY1 + atlasCubeRectY2);
	}

	void createFacesForX(int x, int face)
	{
		int y2 = -1;
		int faceShift = (face == kLeft) ? 0 : 1;
		while (y2 != subImageHeight) {
			int y1 = getFirstVisibleFaceForX(x + faceShift, y2 + 1, face);
			if (y1 == kNotFound)
				return;

			y2 = getFirstHiddenFaceForX(x + faceShift, y1 + 1, face);
			if (y2 == kNotFound)
				y2 = subImageHeight;

			if (face == kLeft)
				createLeftFace(x, y1, x + 1, y2);
			else
				createRightFace(x, y1, x + 1, y2);
		}
	}

	void createFacesForY(int y, int face)
	{
		int x2 = -1;
		int faceShift = (face == kBottom) ? 0 : 1;
		while (x2 != subImageWidth) {
			int x1 = getFirstVisibleFaceForY(x2 + 1, y + faceShift, face);
			if (x1 == kNotFound)
				return;

			x2 = getFirstHiddenFaceForY(x1 + 1, y + faceShift, face);
			if (x2 == kNotFound)
				x2 = subImageWidth;

			if (face == kBottom)
				createBottomFace(x1, y, x2, y + 1);
			else
				createTopFace(x1, y, x2, y + 1);
		}
	}

	void createFacesForZ()
	{
		for (int y = 0; y < subImageHeight; ++y) {
			int x2 = -1;

			// Traverse each column in the texture and look for voxel strips
			while (x2 != subImageWidth) {
				int x1 = findFirstVoxelAlphaTest(x2 + 1, y, 1);
				if (x1 == kNotFound) {
					x2 =  subImageWidth;
					continue;
				}

				x2 = findFirstVoxelAlphaTest(x1 + 1, y, 0);
				if (x2 == kNotFound)
					x2 = subImageWidth;

				createFrontFace(x1, y, x2, y + 1);
				createBackFace(x1, y, x2, y + 1);
			}
		}
	}

	void createLeftFace(int voxelX1, int voxelY1, int voxelX2, int voxelY2)
	{
		Vector2 cubeRect = createCubeRect(voxelX1, voxelY1, voxelX2, voxelY2);

		int index0 = createVertex(voxelX1, voxelY1, 0, cubeRect, kFrontBottomLeft);
		int index1 = createVertex(voxelX1, voxelY2, 0, cubeRect, kFrontTopLeft);
		int index4 = createVertex(voxelX1, voxelY1, voxelDepth, cubeRect, kBackBottomLeft);
		int index5 = createVertex(voxelX1, voxelY2, voxelDepth, cubeRect, kBackTopLeft);

		tri.Add(index4);
		tri.Add(index5);
		tri.Add(index0);
		tri.Add(index0);
		tri.Add(index5);
		tri.Add(index1);
	}

	void createRightFace(int voxelX1, int voxelY1, int voxelX2, int voxelY2)
	{
		Vector2 cubeRect = createCubeRect(voxelX1, voxelY1, voxelX2, voxelY2);

		int index2 = createVertex(voxelX2, voxelY1, 0, cubeRect, kFrontBottomRight);
		int index3 = createVertex(voxelX2, voxelY2, 0, cubeRect, kFrontTopRight);
		int index6 = createVertex(voxelX2, voxelY1, voxelDepth, cubeRect, kBackBottomRight);
		int index7 = createVertex(voxelX2, voxelY2, voxelDepth, cubeRect, kBackTopRight);

		tri.Add(index2);
		tri.Add(index3);
		tri.Add(index6);
		tri.Add(index6);
		tri.Add(index3);
		tri.Add(index7);
	}

	void createBottomFace(int voxelX1, int voxelY1, int voxelX2, int voxelY2)
	{
		Vector2 cubeRect = createCubeRect(voxelX1, voxelY1, voxelX2, voxelY2);

		// TODO: reuse vertices

		int index0 = createVertex(voxelX1, voxelY1, 0, cubeRect, kFrontBottomLeft);
		int index2 = createVertex(voxelX2, voxelY1, 0, cubeRect, kFrontBottomRight);
		int index4 = createVertex(voxelX1, voxelY1, voxelDepth, cubeRect, kBackBottomLeft);
		int index6 = createVertex(voxelX2, voxelY1, voxelDepth, cubeRect, kBackBottomRight);

		tri.Add(index4);
		tri.Add(index0);
		tri.Add(index6);
		tri.Add(index6);
		tri.Add(index0);
		tri.Add(index2);
	}

	void createTopFace(int voxelX1, int voxelY1, int voxelX2, int voxelY2)
	{
		Vector2 cubeRect = createCubeRect(voxelX1, voxelY1, voxelX2, voxelY2);

		int index1 = createVertex(voxelX1, voxelY2, 0, cubeRect, kFrontTopLeft);
		int index3 = createVertex(voxelX2, voxelY2, 0, cubeRect, kFrontTopRight);
		int index5 = createVertex(voxelX1, voxelY2, voxelDepth, cubeRect, kBackTopLeft);
		int index7 = createVertex(voxelX2, voxelY2, voxelDepth, cubeRect, kBackTopRight);

		tri.Add(index1);
		tri.Add(index5);
		tri.Add(index3);
		tri.Add(index3);
		tri.Add(index5);
		tri.Add(index7);
	}

	void createFrontFace(float voxelX1, float voxelY1, float voxelX2, float voxelY2)
	{
		Vector2 cubeRect = createCubeRect(voxelX1, voxelY1, voxelX2, voxelY2);

		int index0 = createVertex(voxelX1, voxelY1, 0, cubeRect, kFrontBottomLeft);
//		int index1 = createVertex(voxelX1, voxelY2, 0, cubeRect, kFrontTopLeft);
		int index2 = createVertex(voxelX2, voxelY1, 0, cubeRect, kFrontBottomRight);
		int index3 = createVertex(voxelX2, voxelY2, 0, cubeRect, kFrontTopRight);
		int index1FrontExlusive = createVertex(voxelX1, voxelY2, 0, cubeRect, kFront);

		tri.Add(index0);
		tri.Add(index1FrontExlusive);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1FrontExlusive);
		tri.Add(index3);
	}

	void createBackFace(float voxelX1, float voxelY1, float voxelX2, float voxelY2)
	{
		Vector2 cubeRect = createCubeRect(voxelX1, voxelY1, voxelX2, voxelY2);

		int index4 = createVertex(voxelX1, voxelY1, voxelDepth, cubeRect, kBackBottomLeft);
		int index5 = createVertex(voxelX1, voxelY2, voxelDepth, cubeRect, kBackTopLeft);
		int index6 = createVertex(voxelX2, voxelY1, voxelDepth, cubeRect, kBackBottomRight);
//		int index7 = createVertex(voxelX2, voxelY2, voxelDepth, cubeRect, kBackTopRight);
		int index7BackExclusive = createVertex(voxelX2, voxelY2, voxelDepth, cubeRect, kBack);

		tri.Add(index6);
		tri.Add(index7BackExclusive);
		tri.Add(index4);
		tri.Add(index4);
		tri.Add(index7BackExclusive);
		tri.Add(index5);
	}
}
