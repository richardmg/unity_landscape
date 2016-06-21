using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelCubesScript : MonoBehaviour {
	public int atlasIndex = 0;
	public int subImageWidth = 16;
	public int subImageHeight = 8;
	public float voxelDepth = 4;
	public bool addFront = true;
	public bool addBack = true;
	public bool addVolume = true;
	public bool trimVolume = false;
	public float cascade = 0.0f;

	Texture2D texture;
	int startPixelX;
	int startPixelY;

	static int[] indices = new int[8];
	static Vector3 vec = new Vector3();
	static List<Vector3> vertices = new List<Vector3>(); 
	static List<int> normalCodes = new List<int>(); 
	static List<int> tri = new List<int>(); 

	const int kVoxelNotFound = -1;
	const int kBottomLeft = 0;
	const int kTopLeft = 1;
	const int kBottomRight = 2;
	const int kTopRight = 3;
	const int kBackSide = 4;

	void Start () {
		vertices.Clear();
		normalCodes.Clear();
		tri.Clear();

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.sharedMaterial.mainTexture;

		// Caluclate uv coords based on atlasIndex. Note that we don't assign any uv coords to the
		// vertices, since those can be calculated directly (and more precisely) in the shader
		// based on the local position of the vertices themselves. But we piggyback the uv sub image
		// origo onto the normals to give the shader at start offset.
		startPixelX = (atlasIndex * subImageWidth) % texture.width;
		startPixelY = (int)((atlasIndex * subImageWidth) / texture.width) * subImageHeight;
		Vector2 uvSubImageBottomLeft = new Vector2((float)startPixelX / texture.width, ((float)startPixelY / texture.height));

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
		mesh.triangles = tri.ToArray();

		// When using object batching, local vertices and normals will be translated on the CPU before
		// passed down to the GPU. We therefore loose the original values in the shader, which we need.
		// We therefore encode this information covered as vertex color.
		int vertexCount = mesh.vertices.Length;
		Vector2[] uvSubImageBottomLeftArray = new Vector2[vertexCount];
		Color[] unbatchedGeometry = new Color[vertexCount];

		for (int i = 0; i < vertexCount; ++i) {
			Vector3 v = mesh.vertices[i];
			uvSubImageBottomLeftArray[i] = uvSubImageBottomLeft;
			unbatchedGeometry[i] = new Color(v.x, v.y, normalCodes[i], voxelDepth);
		}

		mesh.uv = uvSubImageBottomLeftArray;
		mesh.colors = unbatchedGeometry;

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

	int getVertexIndex(Vector3 v, int normalCode, int index)
	{
		// Check if the vertex can be shared with one already created. Note that this causes the normal to
		// be wrong for the cube on top, but that is corrected in the shader.
		if (index != -1)
			return index;

		vertices.Add(new Vector3(v.x, v.y, v.z));
		normalCodes.Add(normalCode);

		return vertices.Count - 1;
	}


	bool writeIndex(int index, float voxelX, float voxelY, float voxelDepth, int normalCode)
	{
		vec.Set(voxelX, voxelY, voxelDepth);
		int i = vertices.FindIndex(v2 => v2 == vec);
		indices[index] = getVertexIndex(vec, normalCode, i);
		return i != -1;
	}

	void createVoxelLineMesh(float voxelX1, float voxelX2, float voxelY1, float voxelY2)
	{
		float voxelZ1 = 0;
		float voxelZ2 = voxelDepth;
		if (cascade != 0 && (int)voxelY1 % 2 == 0) {
			voxelX1 += cascade;
			voxelX2 -= cascade;
			voxelZ1 += cascade;
			voxelZ2 -= cascade;
		}

		bool reuse0 = writeIndex(0, voxelX1, voxelY1, voxelZ1, kBottomLeft);
		writeIndex(1, voxelX1, voxelY2, voxelZ1, kTopLeft);
		bool reuse2 = writeIndex(2, voxelX2, voxelY1, voxelZ1, kBottomRight);
		writeIndex(3, voxelX2, voxelY2, voxelZ1, kTopRight);
		bool reuse4 = writeIndex(4, voxelX1, voxelY1, voxelZ2, kBottomLeft + kBackSide);
		writeIndex(5, voxelX1, voxelY2, voxelZ2, kTopLeft + kBackSide);
		bool reuse6 = writeIndex(6, voxelX2, voxelY1, voxelZ2, kBottomRight + kBackSide);
		writeIndex(7, voxelX2, voxelY2, voxelZ2, kTopRight + kBackSide);

		if (reuse0 && reuse2 && reuse4 && reuse6) {
			// All nodes as shared. Flip nodes on one side to mark that this is
			// bottom of the cube. For the cubes below, no top face will be visible.
			normalCodes[indices[0]] = kBottomLeft;
			normalCodes[indices[4]] = kBottomLeft + kBackSide;
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
