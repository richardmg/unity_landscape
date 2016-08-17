using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NormalCode = System.Int32;

public class VoxelMeshFactory {
	public int atlasIndex = 0;
	public float voxelDepth = 4;
	public bool shareVertices = false;

	public bool xFaces = true;
	public bool yFaces = true;
	public bool zFaces = true;
	public bool useVolume = false;
	public int volumeFaceCountZ = 2;
	public bool simplify = false;

	public Texture2D texture;

	int startPixelX;
	int startPixelY;
	Rect cropRect;

	Mesh mesh = new Mesh();
	List<Vector3> verticeList = new List<Vector3>(2 * 4 * kSubImageWidth * kSubImageHeight); 
	List<Vector2> vertexPixelList = new List<Vector2>(kSubImageWidth * kSubImageHeight); 
	List<int> normalCodeList = new List<int>(2 * 4 * kSubImageWidth * kSubImageHeight); 
	List<int> tri = new List<int>(2 * kSubImageWidth * kSubImageHeight); 

	const int kAtlasWidth = 64;
	const int kAtlasHeight = 64;
	const int kSubImageWidth = 16;
	const int kSubImageHeight = 8;

	const int kMaxVoxelDepth = 100;
	const int kNotFound = -1;

	const NormalCode kFrontLeft = 0;
	const NormalCode kBackLeft = 1;
	const NormalCode kFrontRight = 2;
	const NormalCode kBackRight = 3;
	const NormalCode kFrontBottom = 4;
	const NormalCode kBackBottom = 5;
	const NormalCode kFrontTop = 6;
	const NormalCode kBackTop = 7;
	const NormalCode kFront = 8;
	const NormalCode kBack = 9;
	const NormalCode kNormalCodeMaxValue = kBack;

	// Set to true if shader discard operations should be avoided.
	// Also remember to disable discard in the shader.
	const bool kDisableVolume = false;

	Vector3[] normalForCode = {
		new Vector3(-1, 0, 0),
		new Vector3(-1, 0, 0),
		new Vector3(1, 0, 0),
		new Vector3(1, 0, 0),
		new Vector3(0, -1, 0),
		new Vector3(0, -1, 0),
		new Vector3(0, 1, 0),
		new Vector3(0, 1, 0),
		new Vector3(0, 0, -1),
		new Vector3(0, 0, 1)
	};

	public int readonlyVertexCount = 0;
	public int readonlyTriangleCount = 0;

	public VoxelMeshFactory()
	{
		// TODO: Change out with Color32 matrix, which should be faster access to pixels.
		// And, need to fetch texture from other place than MeshRenderer.
		// TODO: Use fixed sized arrays when creating vertices to avoid uneccessart memory allocations.
		Material m = (Material)Resources.Load("Materials/VoxelObjectExact", typeof(Material));
		texture = (Texture2D)m.mainTexture;
		// Ensure that sizes are synced with shader code
		Debug.Assert(texture.width == kAtlasWidth);
		Debug.Assert(texture.height == kAtlasHeight);
	}

	public void beginMesh()
	{
		mesh = new Mesh();
		verticeList.Clear();
		vertexPixelList.Clear();
		normalCodeList.Clear();
		tri.Clear();
	}

	public void buildMesh()
	{
		startPixelX = (atlasIndex * kSubImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * kSubImageWidth) / texture.width) * kSubImageHeight;
		cropRect = calculatecropRect();

		if (useVolume && !kDisableVolume) {
			if (simplify)
				createMeshVolumeSimplified();
			else
				createMeshVolume();
		} else {
			if (simplify)
				createMeshExactSimplified();
			else
				createMeshExact();
		}
	}

	public void endMesh()
	{
		Color[] cubeDesc = new Color[verticeList.Count];
		Vector3[] normals = new Vector3[verticeList.Count];
		Vector2[] uvAtlas = new Vector2[verticeList.Count];
		Vector2[] uvPixels = new Vector2[verticeList.Count];

		for (int i = 0; i < verticeList.Count; ++i) {
			Vector3 v = verticeList[i];
			normals[i] = normalForCode[normalCodeList[i]];

			// Note that uvPixel specifies which pixel in the atlas the vertex belongs to. And
			// since each pixel have four corners, one pixel can map to four utAtlas coords.
			// We send both as a way to clamp uv inside the subImage in the shader.
			float uvAtlasX = (startPixelX + v.x) / (float)kAtlasWidth;
			float uvAtlasY = (startPixelY + v.y) / (float)kAtlasHeight;
			float uvPixelX = (float)vertexPixelList[i].x / (float)kAtlasWidth;
			float uvPixelY = (float)vertexPixelList[i].y / (float)kAtlasHeight;
			uvAtlas[i] = new Vector2(uvAtlasX, uvAtlasY);
			uvPixels[i] = new Vector2(uvPixelX, uvPixelY);

			// When using object batching, local vertices and normals will be translated on the CPU before
			// passed down to the GPU. We therefore loose the original values in the shader, which we need.
			// We therefore encode this information covered as vertex color.
			// Also, when combinding meshes, vertex data is truncated to be between 0 and 1. So we therefore
			// need to normalize some of the value onto that format.
			float normalizedNormalCode = (float)normalCodeList[i] / (float)kNormalCodeMaxValue;
			float normalizedDepth = voxelDepth / kMaxVoxelDepth;
			cubeDesc[i] = new Color(0, 0, normalizedNormalCode, normalizedDepth);
		}

		mesh.vertices = verticeList.ToArray();
		mesh.triangles = tri.ToArray();
		mesh.uv = uvAtlas;
		mesh.uv2 = uvPixels;
		mesh.colors = cubeDesc;
		mesh.normals = normals;
	}

	public Mesh createMesh() {
		beginMesh();
		buildMesh();
		endMesh();
		return mesh;
	}

	void createMeshExact()
	{
		if (xFaces)
			createXFacesExact();
		if (yFaces)
			createYFacesExact();
		if (zFaces)
			createZFacesExact();
	}

	void createMeshVolume()
	{
		if (xFaces)
			createXFacesVolume();
		if (yFaces)
			createYFacesVolume();
		if (zFaces)
			createZFacesVolume();
	}

	void createMeshExactSimplified()
	{
		if (xFaces)
			createXFacesSimplified();
		if (yFaces)
			createYFacesSimplified();
		if (zFaces)
			createZFacesExact();
	}

	void createMeshVolumeSimplified()
	{
		if (xFaces)
			createXFacesSimplified();
		if (yFaces)
			createYFacesSimplified();
		if (zFaces)
			createZFacesVolume();
	}

	void createXFacesExact()
	{
		for (int x = 0; x < kSubImageWidth; ++x) {
			createVerticalFaces(x, kFrontLeft);
			createVerticalFaces(x, kFrontRight);
		}
	}

	void createYFacesExact()
	{
		for (int y = 0; y < kSubImageHeight; ++y) {
			createHorizontalFaces(y, kFrontBottom);
			createHorizontalFaces(y, kFrontTop);
		}
	}

	void createZFacesExact()
	{
		for (int y1 = 0; y1 < kSubImageHeight; ++y1) {
			int x2 = -1;

			while (x2 != kSubImageWidth) {
				int x1 = getFirstFaceForZ(x2 + 1, y1, true);
				if (x1 == kNotFound) {
					x2 =  kSubImageWidth;
					continue;
				}

				x2 = getFirstFaceForZ(x1 + 1, y1, false);
				if (x2 == kNotFound)
					x2 = kSubImageWidth;

				if (y1 > 0 && isFace(x1, y1 - 1, x2 - 1))
					continue;

				int y2 = y1;
				while (y2 < kSubImageHeight - 1 && isFace(x1, y2 + 1, x2 - 1))
					++y2;
				
				createFrontFace(x1, y1, x2 - 1, y2, 0);

				if (voxelDepth != 0) {
					// Skip back faces for depth == 0, and instead
					// flip normals in the shader for the front surface
					createBackFace(x1, y1, x2 - 1, y2);
				}
			}
		}
	}

	void createXFacesVolume()
	{
		for (int x = 0; x <= kSubImageWidth; ++x) {
			Vector2 singleFaceCount = countSingleFacesForCol(x);
			if (singleFaceCount.x > 0)
				createLeftFace(x, (int)cropRect.y, (int)cropRect.y + (int)cropRect.height - 1);
			if (singleFaceCount.y > 0)
				createRightFace(x - 1, (int)cropRect.y, (int)cropRect.y + (int)cropRect.height - 1);
		}
	}

	void createYFacesVolume()
	{
		for (int y = 0; y <= kSubImageHeight; ++y) {
			Vector2 singleFaceCount = countSingleFacesForRow(y);
			if (singleFaceCount.x > 0)
				createBottomFace((int)cropRect.x, y, (int)cropRect.x + (int)cropRect.width - 1);
			if (singleFaceCount.y > 0)
				createTopFace((int)cropRect.x, y - 1, (int)cropRect.x + (int)cropRect.width - 1);
		}
	}

	void createZFacesVolume()
	{
		float deltaZ = voxelDepth / Mathf.Max(1, volumeFaceCountZ - 1);

		for (int z = 0; z < volumeFaceCountZ - 1; ++z)
			createFrontFace(
				(int)cropRect.x,
				(int)cropRect.y,
				(int)cropRect.x + (int)cropRect.width - 1,
				(int)cropRect.y + (int)cropRect.height - 1,
				z * deltaZ); 

		if (voxelDepth != 0) {
			createBackFace(
				(int)cropRect.x, (int)cropRect.y,
				(int)cropRect.x + (int)cropRect.width - 1,
				(int)cropRect.y + (int)cropRect.height - 1); 
		}
	}

	void createXFacesSimplified()
	{
		int bestColLeft = kNotFound;
		int bestColRight = kNotFound;
		int x2 = (int)cropRect.x + (int)(cropRect.width);

		int bestCount = 0;
		for (int x = (int)cropRect.x; x < x2; ++x) {
			int count = countPixelsForCol(x);
			if (count > bestCount) {
				bestColLeft = x;
				bestCount = count;
			}
		}

		bestCount = 0;
		for (int x = kSubImageWidth - 1; x >= Mathf.Max(0, bestColLeft + 1); --x) {
			int count = countPixelsForCol(x);
			if (count > bestCount) {
				bestColRight = x;
				bestCount = count;
			}
		}

		if (bestColLeft != kNotFound)
			createLeftFace(bestColLeft, (int)cropRect.y, (int)cropRect.y + (int)cropRect.height - 1);
		if (bestColRight != kNotFound)
			createRightFace(bestColRight, (int)cropRect.y, (int)cropRect.y + (int)cropRect.height - 1);
	}

	void createYFacesSimplified()
	{
		int bestRowBottom = kNotFound;
		int bestRowTop = kNotFound;
		int y2 = (int)cropRect.y + (int)(cropRect.height);

		int bestCount = 0;
		for (int y = (int)cropRect.y; y < y2; ++y) {
			int count = countPixelsForRow(y);
			if (count > bestCount) {
				bestRowBottom = y;
				bestCount = count;
			}
		}

		bestCount = 0;
		for (int y = kSubImageHeight - 1; y >= Mathf.Max(0, bestRowBottom + 1); --y) {
			int count = countPixelsForRow(y);
			if (count > bestCount) {
				bestRowTop = y;
				bestCount = count;
			}
		}

		if (bestRowBottom != kNotFound)
			createBottomFace((int)cropRect.x, bestRowBottom, (int)cropRect.x + (int)cropRect.width - 1);
		if (bestRowTop != kNotFound)
			createTopFace((int)cropRect.x, bestRowTop, (int)cropRect.x + (int)cropRect.width - 1);
	}

	bool isFace(int x1, int y, int x2)
	{
		// Returns true if the given coords maps to a separate pixel strip in the atlas
		if (x1 > 0 && texture.GetPixel(startPixelX + x1 - 1, startPixelY + y).a != 0)
			return false;

		if (x2 < kSubImageWidth - 1 && texture.GetPixel(startPixelX + x2 + 1, startPixelY + y).a != 0)
			return false;

		for (int x = x1; x <= x2; ++x) {
			if (texture.GetPixel(startPixelX + x, startPixelY + y).a == 0)
				return false;
		}

		return true;
	}

	Rect calculatecropRect()
	{
		int x1 = 0;
		int y1 = 0;
		int x2 = 0;
		int y2 = 0;

		for (x1 = 0; x1 < kSubImageWidth; ++x1) {
			if (countPixelsForCol(x1) > 0)
				break;
		}

		for (x2 = kSubImageWidth; x2 > x1; --x2) {
			if (countPixelsForCol(x2 - 1) > 0)
				break;
		}

		for (y1 = 0; y1 < kSubImageHeight; ++y1) {
			if (countPixelsForRow(y1) > 0)
				break;
		}

		for (y2 = kSubImageHeight; y2 > y1; --y2) {
			if (countPixelsForRow(y2 - 1) > 0)
				break;
		}

		return new Rect(x1, y1, x2 - x1, y2 -y1);
	}

	int countPixelsForCol(int x)
	{
		int count = 0;
		for (int y = 0; y < kSubImageHeight; ++y) {
			Color c1 = texture.GetPixel(startPixelX + x, startPixelY + y);
			if (c1.a != 0)
				++count;
		}

		return count;
	}

	int countPixelsForRow(int y)
	{
		int count = 0;
		for (int x = 0; x < kSubImageWidth; ++x) {
			Color c1 = texture.GetPixel(startPixelX + x, startPixelY + y);
			if (c1.a != 0)
				++count;
		}

		return count;
	}

	Vector2 countSingleFacesForCol(int x)
	{
		Vector2 faceCount = new Vector2();
		for (int y = 0; y < kSubImageHeight; ++y) {
			Color c1 = (x == kSubImageWidth) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + y);
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
		for (int x = 0; x < kSubImageWidth; ++x) {
			Color c1 = (y == kSubImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + y);
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

	int getFirstFaceForX(int startX, int startY, NormalCode face, bool searchForVisible)
	{
		for (int y = startY; y < kSubImageHeight; ++y) {
			Color c1 = (startX == kSubImageWidth) ? Color.clear : texture.GetPixel(startPixelX + startX, startPixelY + y);
			Color c2 = (startX == 0) ? Color.clear : texture.GetPixel(startPixelX + startX - 1, startPixelY + y);

			if (searchForVisible) {
				if (face == kFrontLeft && c1.a == 1 && c2.a == 0)
					return y;
				if (face == kFrontRight && c1.a == 0 && c2.a == 1)
					return y;
			} else {
				if (face == kFrontLeft && (c1.a == c2.a || c1.a == 0))
					return y;
				if (face == kFrontRight && (c1.a == c2.a || c2.a == 0))
					return y;
			}
		}

		return kNotFound;
	}

	int getFirstFaceForY(int startX, int startY, NormalCode face, bool searchForVisible)
	{
		for (int x = startX; x < kSubImageWidth; ++x) {
			Color c1 = (startY == kSubImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY);
			Color c2 = (startY == 0) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY - 1);

			if (searchForVisible) {
				if (face == kFrontBottom && c1.a == 1 && c2.a == 0)
					return x;
				if (face == kFrontTop && c1.a == 0 && c2.a == 1)
					return x;
			} else {
				if (face == kFrontBottom && (c1.a == c2.a || c1.a == 0))
					return x;
				if (face == kFrontTop && (c1.a == c2.a || c2.a == 0))
					return x;
			}
		}

		return kNotFound;
	}

	int getFirstFaceForZ(int startX, int startY, bool searchForVisible)
	{
		for (int x = startX; x < kSubImageWidth; ++x) {
			Color c = texture.GetPixel(startPixelX + x, startPixelY + startY);
			if (searchForVisible && Mathf.CeilToInt(c.a) == 1)
				return x;
			if (!searchForVisible && Mathf.CeilToInt(c.a) == 0)
				return x;
		}
		return kNotFound;
	}

	void createVerticalFaces(int x, NormalCode face)
	{
		int y2 = -1;
		int faceShift = (face == kFrontLeft) ? 0 : 1;
		while (y2 != kSubImageHeight) {
			int y1 = getFirstFaceForX(x + faceShift, y2 + 1, face, true);
			if (y1 == kNotFound)
				return;

			y2 = getFirstFaceForX(x + faceShift, y1 + 1, face, false);
			if (y2 == kNotFound)
				y2 = kSubImageHeight;

			if (face == kFrontLeft)
				createLeftFace(x, y1, y2 - 1);
			else
				createRightFace(x, y1, y2 - 1);
		}
	}

	void createHorizontalFaces(int y, NormalCode face)
	{
		int x2 = -1;
		int faceShift = (face == kFrontBottom) ? 0 : 1;
		while (x2 != kSubImageWidth) {
			int x1 = getFirstFaceForY(x2 + 1, y + faceShift, face, true);
			if (x1 == kNotFound)
				return;

			x2 = getFirstFaceForY(x1 + 1, y + faceShift, face, false);
			if (x2 == kNotFound)
				x2 = kSubImageWidth;

			if (face == kFrontBottom)
				createBottomFace(x1, y, x2 - 1);
			else
				createTopFace(x1, y, x2 - 1);
		}
	}

	int getVertexIndex(Vector3 v, Vector2 pixel, NormalCode normalCode)
	{
		int i = verticeList.FindIndex(v2 => v2 == v);
		if (i == kNotFound)
			return kNotFound;

		if (vertexPixelList[i] != pixel)
			return kNotFound;

//		if (normalCode != normalCodeList[i])
//			return kNotFound;

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

		int index0 = createVertex(pixelX, pixelY1, 0, pixelBottom, kFrontLeft);
		int index1 = createVertex(pixelX, pixelY2 + 1, 0, pixelTop, kFrontLeft);
		int index4 = createVertex(pixelX, pixelY1, voxelDepth, pixelBottom, kBackLeft);
		int index5 = createVertex(pixelX, pixelY2 + 1, voxelDepth, pixelTop, kBackLeft);

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

		int index2 = createVertex(pixelX + 1, pixelY1, 0, pixelBottom, kFrontRight);
		int index3 = createVertex(pixelX + 1, pixelY2 + 1, 0, pixelTop, kFrontRight);
		int index6 = createVertex(pixelX + 1, pixelY1, voxelDepth, pixelBottom, kBackRight);
		int index7 = createVertex(pixelX + 1, pixelY2 + 1, voxelDepth, pixelTop, kBackRight);

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

		int index0 = createVertex(pixelX1, pixelY, 0, pixelLeft, kFrontBottom);
		int index2 = createVertex(pixelX2 + 1, pixelY, 0, pixelRight, kFrontBottom);
		int index4 = createVertex(pixelX1, pixelY, voxelDepth, pixelLeft, kBackBottom);
		int index6 = createVertex(pixelX2 + 1, pixelY, voxelDepth, pixelRight, kBackBottom);

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

		int index1 = createVertex(pixelX1, pixelY + 1, 0, pixelLeft, kFrontTop);
		int index3 = createVertex(pixelX2 + 1, pixelY + 1, 0, pixelRight, kFrontTop);
		int index5 = createVertex(pixelX1, pixelY + 1, voxelDepth, pixelLeft, kBackTop);
		int index7 = createVertex(pixelX2 + 1, pixelY + 1, voxelDepth, pixelRight, kBackTop);

		tri.Add(index1);
		tri.Add(index5);
		tri.Add(index3);
		tri.Add(index3);
		tri.Add(index5);
		tri.Add(index7);
	}

	void createFrontFace(int pixelX1, int pixelY1, int pixelX2, int pixelY2, float z)
	{
		Vector2 pixelBottomLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY1);
		Vector2 pixelBottomRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY1);
		Vector2 pixelTopLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY2);
		Vector2 pixelTopRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY2);

		int index0 = createVertex(pixelX1, pixelY1, z, pixelBottomLeft, kFront);
		int index1 = createVertex(pixelX1, pixelY2 + 1, z, pixelTopLeft, kFront);
		int index2 = createVertex(pixelX2 + 1, pixelY1, z, pixelBottomRight, kFront);
		int index3 = createVertex(pixelX2 + 1, pixelY2 + 1, z, pixelTopRight, kFront);

		tri.Add(index0);
		tri.Add(index1);
		tri.Add(index2);
		tri.Add(index2);
		tri.Add(index1);
		tri.Add(index3);
	}

	void createBackFace(int pixelX1, int pixelY1, int pixelX2, int pixelY2)
	{
		Vector2 pixelBottomLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY1);
		Vector2 pixelBottomRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY1);
		Vector2 pixelTopLeft = new Vector2(startPixelX + pixelX1, startPixelY + pixelY2);
		Vector2 pixelTopRight = new Vector2(startPixelX + pixelX2, startPixelY + pixelY2);

		int index4 = createVertex(pixelX1, pixelY1, voxelDepth, pixelBottomLeft, kBack);
		int index5 = createVertex(pixelX1, pixelY2 + 1, voxelDepth, pixelTopLeft, kBack);
		int index6 = createVertex(pixelX2 + 1, pixelY1, voxelDepth, pixelBottomRight, kBack);
		int index7 = createVertex(pixelX2 + 1, pixelY2 + 1, voxelDepth, pixelTopRight, kBack);

		tri.Add(index6);
		tri.Add(index7);
		tri.Add(index4);
		tri.Add(index4);
		tri.Add(index7);
		tri.Add(index5);
	}
}
