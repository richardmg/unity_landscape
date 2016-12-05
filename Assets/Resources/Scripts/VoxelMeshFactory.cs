using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NormalCode = System.Int32;
using UvCode = System.Int32;

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

	public float centerX = Root.kSubImageWidth / 2;
	public float centerY = 0;
	public float centerZ = 0;

	public Texture2D texture;

	int startPixelX;
	int startPixelY;
	Rect cropRect;

	Mesh mesh = new Mesh();
	List<Vector3> vertexList = new List<Vector3>(2 * 4 * Root.kSubImageWidth * Root.kSubImageHeight); 
	List<Vector2> vertexPixelList = new List<Vector2>(Root.kSubImageWidth * Root.kSubImageHeight); 
	List<Vector2> normalMapList = new List<Vector2>(2 * 4 * Root.kSubImageWidth * Root.kSubImageHeight); 
	List<NormalCode> normalCodeList = new List<NormalCode>(2 * 4 * Root.kSubImageWidth * Root.kSubImageHeight); 
	List<int> tri = new List<int>(2 * Root.kSubImageWidth * Root.kSubImageHeight); 

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
	// Note that current factory implementation will anyway create volume faces for
	// inner faces (it creates one big face across the whole subimage). So this must
	// also be fixed if this route is taken.
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

	public void beginMesh()
	{
		if (Root.instance.atlasManager != null)
			texture = Root.instance.atlasManager.textureAtlas;
		else
			texture = Root.instance.textureAtlas;

		mesh = new Mesh();
		vertexList.Clear();
		vertexPixelList.Clear();
		normalMapList.Clear();
		normalCodeList.Clear();
		tri.Clear();

		// todo: should centerZ be
		centerZ = voxelDepth / 2;
	}

	public void buildMesh()
	{
		AtlasManager.getAtlasPixelForIndex(atlasIndex, out startPixelX, out startPixelY);
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
		Color[] cubeDesc = new Color[vertexList.Count];
		Vector3[] normals = new Vector3[vertexList.Count];
		Vector2[] uvAtlas = new Vector2[vertexList.Count];
		Vector2[] uvPixels = new Vector2[vertexList.Count];
		float cull = (voxelDepth == 0 || simplify) ? 0 : 1;

		for (int i = 0; i < vertexList.Count; ++i) {
			Vector3 v = vertexList[i];
			normals[i] = normalForCode[normalCodeList[i]];

			// Note that uvPixel specifies which pixel in the atlas the vertex belongs to. And
			// since each pixel have four corners, one pixel can map to four uvAtlas coords.
			// We send both as a way to clamp uv inside the subImage in the shader.
			float uvAtlasX = (startPixelX + v.x) / (float)Root.kAtlasWidth;
			float uvAtlasY = (startPixelY + v.y) / (float)Root.kAtlasHeight;
			float uvPixelX = (float)vertexPixelList[i].x / (float)Root.kAtlasWidth;
			float uvPixelY = (float)vertexPixelList[i].y / (float)Root.kAtlasHeight;
			uvAtlas[i] = new Vector2(uvAtlasX, uvAtlasY);
			uvPixels[i] = new Vector2(uvPixelX, uvPixelY);

			// When using object batching, local vertices and normals will be translated on the CPU before
			// passed down to the GPU. We therefore lose the original values in the shader, which we need.
			// We therefore encode this information covered as vertex color.
			// Also, when combinding meshes, vertex data is truncated to be between 0 and 1. So we therefore
			// need to normalize some of the value onto that format.
			float normalizedNormalCode = (float)normalCodeList[i] / (float)kNormalCodeMaxValue;
			float normalizedDepth = voxelDepth;// / kMaxVoxelDepth;

			int materialId = 0; // or pass metallic and shininess directly.
			cubeDesc[i] = new Color(cull, materialId, normalizedNormalCode, normalizedDepth);

			// Adjust all vertices according to what is center
			v.x -= centerX;
			v.y -= centerY;
			v.z -= centerZ;
			vertexList[i] = v;
		}

		mesh.vertices = vertexList.ToArray();
		mesh.triangles = tri.ToArray();
		mesh.uv = uvAtlas;
		mesh.uv2 = uvPixels;
		mesh.uv3 = normalMapList.ToArray();
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
		for (int x = 0; x < Root.kSubImageWidth; ++x) {
			createVerticalFaces(x, kFrontLeft);
			createVerticalFaces(x, kFrontRight);
		}
	}

	void createYFacesExact()
	{
		for (int y = 0; y < Root.kSubImageHeight; ++y) {
			createHorizontalFaces(y, kFrontBottom);
			createHorizontalFaces(y, kFrontTop);
		}
	}

	void createZFacesExact()
	{
		for (int y1 = 0; y1 < Root.kSubImageHeight; ++y1) {
			int x2 = -1;

			while (x2 != Root.kSubImageWidth) {
				int x1 = getFirstFaceForZ(x2 + 1, y1, true);
				if (x1 == kNotFound) {
					x2 =  Root.kSubImageWidth;
					continue;
				}

				x2 = getFirstFaceForZ(x1 + 1, y1, false);
				if (x2 == kNotFound)
					x2 = Root.kSubImageWidth;

				if (y1 > 0 && isFace(x1, y1 - 1, x2 - 1))
					continue;

				int y2 = y1;
				while (y2 < Root.kSubImageHeight - 1 && isFace(x1, y2 + 1, x2 - 1))
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
		for (int x = 0; x <= Root.kSubImageWidth; ++x) {
			Vector2 singleFaceCount = countSingleFacesForCol(x);
			if (singleFaceCount.x > 0)
				createLeftFace(x, (int)cropRect.y, (int)cropRect.y + (int)cropRect.height - 1);
			if (singleFaceCount.y > 0)
				createRightFace(x - 1, (int)cropRect.y, (int)cropRect.y + (int)cropRect.height - 1);
		}
	}

	void createYFacesVolume()
	{
		for (int y = 0; y <= Root.kSubImageHeight; ++y) {
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
		for (int x = Root.kSubImageWidth - 1; x >= Mathf.Max(0, bestColLeft + 1); --x) {
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
		for (int y = Root.kSubImageHeight - 1; y >= Mathf.Max(0, bestRowBottom + 1); --y) {
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

		if (x2 < Root.kSubImageWidth - 1 && texture.GetPixel(startPixelX + x2 + 1, startPixelY + y).a != 0)
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

		for (x1 = 0; x1 < Root.kSubImageWidth; ++x1) {
			if (countPixelsForCol(x1) > 0)
				break;
		}

		for (x2 = Root.kSubImageWidth; x2 > x1; --x2) {
			if (countPixelsForCol(x2 - 1) > 0)
				break;
		}

		for (y1 = 0; y1 < Root.kSubImageHeight; ++y1) {
			if (countPixelsForRow(y1) > 0)
				break;
		}

		for (y2 = Root.kSubImageHeight; y2 > y1; --y2) {
			if (countPixelsForRow(y2 - 1) > 0)
				break;
		}

		return new Rect(x1, y1, x2 - x1, y2 -y1);
	}

	int countPixelsForCol(int x)
	{
		int count = 0;
		for (int y = 0; y < Root.kSubImageHeight; ++y) {
			Color c1 = texture.GetPixel(startPixelX + x, startPixelY + y);
			if (c1.a != 0)
				++count;
		}

		return count;
	}

	int countPixelsForRow(int y)
	{
		int count = 0;
		for (int x = 0; x < Root.kSubImageWidth; ++x) {
			Color c1 = texture.GetPixel(startPixelX + x, startPixelY + y);
			if (c1.a != 0)
				++count;
		}

		return count;
	}

	Vector2 countSingleFacesForCol(int x)
	{
		Vector2 faceCount = new Vector2();
		for (int y = 0; y < Root.kSubImageHeight; ++y) {
			Color c1 = (x == Root.kSubImageWidth) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + y);
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
		for (int x = 0; x < Root.kSubImageWidth; ++x) {
			Color c1 = (y == Root.kSubImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + y);
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
		for (int y = startY; y < Root.kSubImageHeight; ++y) {
			Color c1 = (startX == Root.kSubImageWidth) ? Color.clear : texture.GetPixel(startPixelX + startX, startPixelY + y);
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
		for (int x = startX; x < Root.kSubImageWidth; ++x) {
			Color c1 = (startY == Root.kSubImageHeight) ? Color.clear : texture.GetPixel(startPixelX + x, startPixelY + startY);
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
		for (int x = startX; x < Root.kSubImageWidth; ++x) {
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
		while (y2 != Root.kSubImageHeight) {
			int y1 = getFirstFaceForX(x + faceShift, y2 + 1, face, true);
			if (y1 == kNotFound)
				return;

			y2 = getFirstFaceForX(x + faceShift, y1 + 1, face, false);
			if (y2 == kNotFound)
				y2 = Root.kSubImageHeight;

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
		while (x2 != Root.kSubImageWidth) {
			int x1 = getFirstFaceForY(x2 + 1, y + faceShift, face, true);
			if (x1 == kNotFound)
				return;

			x2 = getFirstFaceForY(x1 + 1, y + faceShift, face, false);
			if (x2 == kNotFound)
				x2 = Root.kSubImageWidth;

			if (face == kFrontBottom)
				createBottomFace(x1, y, x2 - 1);
			else
				createTopFace(x1, y, x2 - 1);
		}
	}

	int getVertexIndex(Vector3 v, Vector2 pixel, NormalCode normalCode)
	{
		int i = vertexList.FindIndex(v2 => v2 == v);
		if (i == kNotFound)
			return kNotFound;

		if (vertexPixelList[i] != pixel)
			return kNotFound;

//		if (normalCode != normalCodeList[i])
//			return kNotFound;

		return i;
	}

	int createVertex(float x, float y, float z, Vector2 pixel, NormalCode normalCode, Vector2 normalMap)
	{
		Vector3 v = new Vector3(x, y, z);

		if (shareVertices) {
			int index = getVertexIndex(v, pixel, normalCode);
			if (index != kNotFound)
				return index;
		}

		vertexList.Add(v);
		normalCodeList.Add(normalCode);
		vertexPixelList.Add(pixel);
		normalMapList.Add(normalMap);

		return vertexList.Count - 1;
	}

	Vector2 createUvNormal(float pixelX1, float pixelY1)
	{
		// When moving to normal map atlas, this must be rewritten so
		// that we don't scale bottomLeft/bottomRight.

		// Use wood material:
		float scaleX = 2.0f;
		float scaleY = 0.8f;

		return new Vector2(pixelX1 * scaleX / Root.kSubImageWidth, pixelY1 * scaleY / Root.kSubImageHeight);
	}

	void createLeftFace(int pixelX, int pixelY1, int pixelY2)
	{
		Vector2 pixelBottom = new Vector2(startPixelX + pixelX, startPixelY + pixelY1);
		Vector2 pixelTop = new Vector2(startPixelX + pixelX, startPixelY + pixelY2);

		Vector2 uvNMBottomLeft = createUvNormal(0, pixelY1);
		Vector2 uvNMTopLeft = createUvNormal(0, pixelY2 + 1);
		Vector2 uvNMBottomRight = createUvNormal(voxelDepth + 1, pixelY1);
		Vector2 uvNMTopRight = createUvNormal(voxelDepth + 1, pixelY2 + 1);

		int index0 = createVertex(pixelX, pixelY1, 0, pixelBottom, kFrontLeft, uvNMBottomRight);
		int index1 = createVertex(pixelX, pixelY2 + 1, 0, pixelTop, kFrontLeft, uvNMTopRight);
		int index4 = createVertex(pixelX, pixelY1, voxelDepth, pixelBottom, kBackLeft, uvNMBottomLeft);
		int index5 = createVertex(pixelX, pixelY2 + 1, voxelDepth, pixelTop, kBackLeft, uvNMTopLeft);

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

		Vector2 uvNMBottomLeft = createUvNormal(0, pixelY1);
		Vector2 uvNMTopLeft = createUvNormal(0, pixelY2 + 1);
		Vector2 uvNMBottomRight = createUvNormal(voxelDepth + 1, pixelY1);
		Vector2 uvNMTopRight = createUvNormal(voxelDepth + 1, pixelY2 + 1);

		int index2 = createVertex(pixelX + 1, pixelY1, 0, pixelBottom, kFrontRight, uvNMBottomLeft);
		int index3 = createVertex(pixelX + 1, pixelY2 + 1, 0, pixelTop, kFrontRight, uvNMTopLeft);
		int index6 = createVertex(pixelX + 1, pixelY1, voxelDepth, pixelBottom, kBackRight, uvNMBottomRight);
		int index7 = createVertex(pixelX + 1, pixelY2 + 1, voxelDepth, pixelTop, kBackRight, uvNMTopRight);

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

		Vector2 uvNMBottomLeft = createUvNormal(pixelX1, 0);
		Vector2 uvNMTopLeft = createUvNormal(pixelX1, voxelDepth + 1);
		Vector2 uvNMBottomRight = createUvNormal(pixelX2 + 1, 0);
		Vector2 uvNMTopRight = createUvNormal(pixelX2 + 1, voxelDepth + 1);

		int index0 = createVertex(pixelX1, pixelY, 0, pixelLeft, kFrontBottom, uvNMTopLeft);
		int index2 = createVertex(pixelX2 + 1, pixelY, 0, pixelRight, kFrontBottom, uvNMTopRight);
		int index4 = createVertex(pixelX1, pixelY, voxelDepth, pixelLeft, kBackBottom, uvNMBottomLeft);
		int index6 = createVertex(pixelX2 + 1, pixelY, voxelDepth, pixelRight, kBackBottom, uvNMBottomRight);

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

		Vector2 uvNMBottomLeft = createUvNormal(pixelX1, 0);
		Vector2 uvNMTopLeft = createUvNormal(pixelX1, voxelDepth + 1);
		Vector2 uvNMBottomRight = createUvNormal(pixelX2 + 1, 0);
		Vector2 uvNMTopRight = createUvNormal(pixelX2 + 1, voxelDepth + 1);

		int index1 = createVertex(pixelX1, pixelY + 1, 0, pixelLeft, kFrontTop, uvNMBottomLeft);
		int index3 = createVertex(pixelX2 + 1, pixelY + 1, 0, pixelRight, kFrontTop, uvNMBottomRight);
		int index5 = createVertex(pixelX1, pixelY + 1, voxelDepth, pixelLeft, kBackTop, uvNMTopLeft);
		int index7 = createVertex(pixelX2 + 1, pixelY + 1, voxelDepth, pixelRight, kBackTop, uvNMTopRight);

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

		Vector2 uvNMBottomLeft = createUvNormal(pixelX1, pixelY1);
		Vector2 uvNMTopLeft = createUvNormal(pixelX1, pixelY2 + 1);
		Vector2 uvNMBottomRight = createUvNormal(pixelX2 + 1, pixelY1);
		Vector2 uvNMTopRight = createUvNormal(pixelX2 + 1, pixelY2 + 1);

		int index0 = createVertex(pixelX1, pixelY1, z, pixelBottomLeft, kFront, uvNMBottomLeft);
		int index1 = createVertex(pixelX1, pixelY2 + 1, z, pixelTopLeft, kFront, uvNMTopLeft);
		int index2 = createVertex(pixelX2 + 1, pixelY1, z, pixelBottomRight, kFront, uvNMBottomRight);
		int index3 = createVertex(pixelX2 + 1, pixelY2 + 1, z, pixelTopRight, kFront, uvNMTopRight);

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

		Vector2 uvNMBottomLeft = createUvNormal(pixelX1, pixelY1);
		Vector2 uvNMTopLeft = createUvNormal(pixelX1, pixelY2 + 1);
		Vector2 uvNMBottomRight = createUvNormal(pixelX2 + 1, pixelY1);
		Vector2 uvNMTopRight = createUvNormal(pixelX2 + 1, pixelY2 + 1);

		int index4 = createVertex(pixelX1, pixelY1, voxelDepth, pixelBottomLeft, kBack, uvNMBottomRight);
		int index5 = createVertex(pixelX1, pixelY2 + 1, voxelDepth, pixelTopLeft, kBack, uvNMTopRight);
		int index6 = createVertex(pixelX2 + 1, pixelY1, voxelDepth, pixelBottomRight, kBack, uvNMBottomLeft);
		int index7 = createVertex(pixelX2 + 1, pixelY2 + 1, voxelDepth, pixelTopRight, kBack, uvNMTopLeft);

		tri.Add(index6);
		tri.Add(index7);
		tri.Add(index4);
		tri.Add(index4);
		tri.Add(index7);
		tri.Add(index5);
	}
}
