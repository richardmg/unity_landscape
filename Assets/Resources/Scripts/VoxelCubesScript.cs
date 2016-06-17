using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelCubesScript : MonoBehaviour {
	public int atlasIndex = 0;
	public int subImageWidth = 16;
	public int subImageHeight = 8;
	public bool addFront = true;
	public bool addBack = true;
	public bool addVolume = true;
	public bool trimVolume = false;

	Texture2D texture;
	Vector2 uvSubImageBottomLeft;
	Vector2 uvOnePixel;
	int startPixelX;
	int startPixelY;

	static int[] indices = new int[8];
	static Vector3 vec = new Vector3();
	static Vector3 nor = new Vector3();

	static List<Vector3> vertices = new List<Vector3>(); 
	static List<Vector3> normals = new List<Vector3>(); 
	static List<Vector2> uvs = new List<Vector2>(); 
	static List<int> tri = new List<int>(); 

	const int kVoxelNotFound = -1;

	void Start () {
		vertices.Clear();
		normals.Clear();
		uvs.Clear();
		tri.Clear();

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.material.mainTexture;

		// Caluclate uv coords based on atlasIndex
		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;

		uvSubImageBottomLeft = new Vector2((float)startPixelX / texture.width, ((float)startPixelY / texture.height));
		uvOnePixel = new Vector2(1.0f / texture.width, 1.0f / texture.height);

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
		mesh.vertices = vertices.ToArray();
		mesh.normals = normals.ToArray();
		mesh.uv = uvs.ToArray();
		mesh.triangles = tri.ToArray();

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

	int getVertexIndex(Vector3 v, Vector3 n)
	{
		vertices.Add(new Vector3(v.x, v.y, v.z));
		normals.Add(new Vector3(n.x, n.y, n.z));
		uvs.Add(new Vector2(uvSubImageBottomLeft.x + (v.x * uvOnePixel.x), uvSubImageBottomLeft.y + (v.y * uvOnePixel.y)));

		return vertices.Count - 1;
	}

	void createVoxelLineMesh(int voxelX1, int voxelX2, int voxelY1, int voxelY2)
	{
		for (int side = 0; side <= 4; side += 4) {
			float offset = side / 4;
			float normalZ = -1 + (side / 2);

			vec.Set(voxelX1, voxelY1, offset);
			nor.Set(-1 - uvSubImageBottomLeft.x, -1 - uvSubImageBottomLeft.y, normalZ);
			indices[0 + side] = getVertexIndex(vec, nor);

			vec.Set(voxelX1, voxelY2, offset);
			nor.Set(-1 - uvSubImageBottomLeft.x, 1 + uvSubImageBottomLeft.y, normalZ);
			indices[1 + side] = getVertexIndex(vec, nor);

			vec.Set(voxelX2, voxelY1, offset);
			nor.Set(1 + uvSubImageBottomLeft.x, -1 - uvSubImageBottomLeft.y, normalZ);
			indices[2 + side] = getVertexIndex(vec, nor);

			vec.Set(voxelX2, voxelY2, offset);
			nor.Set(1 + uvSubImageBottomLeft.x, 1 + uvSubImageBottomLeft.y, normalZ);
			indices[3 + side] = getVertexIndex(vec, nor);
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
