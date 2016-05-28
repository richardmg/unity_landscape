using UnityEngine;
using System.Collections;

public class VoxelPlaneScript : MonoBehaviour {
	public Texture2D texture;
	public float voxelWidth = 0.1f;
	public float voxelHeight = 0.1f;
	public float voxelDepth = 1;

	private int textureVoxelWidth = 10;
	private int textureVoxelHeight = 10;

	void Start () {
		CombineInstance[] ci = new CombineInstance[textureVoxelHeight];

		for (int y = 0; y < textureVoxelHeight; ++y) {
			int x1 = findFirstTexelAlphaTest(0, y, 1);
			if (x1 == -1)
				continue;
			int x2 = findFirstTexelAlphaTest(x1 + 1, y, 0);
			if (x2 == -1)
				x2 = x1 + 1;

			Mesh mesh = createVoxelLineMesh(x1, x2, y);
			Matrix4x4 transform = new Matrix4x4();
			transform.SetTRS(new Vector3(0, y * voxelHeight, 0), Quaternion.identity, new Vector3(1, 1, 1));
			ci[y].mesh = mesh;
			ci[y].transform = transform;
		}

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ci, true, true);
		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;

		MeshRenderer meshRenderer = (MeshRenderer)gameObject.AddComponent<MeshRenderer>();
		meshRenderer.material = (Material)Resources.Load("Materials/CutoffM");
	}

	int findFirstTexelAlphaTest(int startX, int startY, int alpha)
	{
		float textureStepX = texture.width / textureVoxelWidth;
		float textureStepY = texture.height / textureVoxelHeight;

		for (int x = startX; x < textureVoxelWidth; ++x) {
			// Grab center texel. This will fail for texels that are not solid
			int tx = (int)((x * textureStepX) + (textureStepX / 2));
			int ty = (int)((startY * textureStepY) + (textureStepY / 2));
			Color c = texture.GetPixel(tx, ty);
			// Either the textel is transparent, or it's not
			if (Mathf.CeilToInt(c.a) == alpha)
				return x;
		}
		return -1;
	}

	Mesh createVoxelLineMesh(int voxelX1, int voxelX2, int voxelY)
	{
		float w = (voxelX2 - voxelX1) * voxelWidth;
		float h = voxelHeight;
		float sx = (1.0f / textureVoxelWidth) * voxelX1;
		float ex = (1.0f / textureVoxelWidth) * voxelX2;
		float sy = (1.0f / textureVoxelHeight) * voxelY;
		float ey = (1.0f / textureVoxelHeight) * (voxelY + 1);

		Vector3[] v = new Vector3[8];
		Vector2[] uv = new Vector2[8];
		int[] tri = new int[12];

		// Front vertices
		v[0].x = 0; v[0].y = 0; v[0].z = 0;
		v[1].x = 0; v[1].y = h; v[1].z = 0;
		v[2].x = w; v[2].y = 0; v[2].z = 0;
		v[3].x = w; v[3].y = h; v[3].z = 0;

		// Back vertices
		for (int i = 4; i < 8; ++i) {
			v[i] = v[i - 4];
			v[i].z = voxelDepth;
		}

		// Front texture coords
		uv[0].x = sx; uv[0].y = sy;
		uv[1].x = sx; uv[1].y = ey;
		uv[2].x = ex; uv[2].y = sy;
		uv[3].x = ex; uv[3].y = ey;

		// Back texture coords
		for (int i = 4; i < 8; ++i)
			uv[i] = uv[i - 4];

		// Front triangles
		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 2;
		tri[3] = 2;
		tri[4] = 1;
		tri[5] = 3;

		// Back triangles
		tri[6] = 5;
		tri[7] = 4;
		tri[8] = 7;
		tri[9] = 4;
		tri[10] = 6;
		tri[11] = 7;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;
		mesh.RecalculateNormals();

		return mesh;
	}
}
