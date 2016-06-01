using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TextureArrayVolumeScript : MonoBehaviour {
	public int count = 50;

	private Texture2D texture;
	private int cols;
	private int rows;

	void Start () {
		MeshRenderer meshRenderer = (MeshRenderer)gameObject.GetComponent<MeshRenderer>();
		texture = (Texture2D)meshRenderer.material.mainTexture;

		cols = texture.width;
		rows = texture.height;

		List<CombineInstance> ciList = new List<CombineInstance>();

		for (int i = 0; i < count; ++i)
			ciList.Add(createCombineInstance(createXYQuad((1.0f / count) * i), new Vector3(0, 0, 0)));

		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(ciList.ToArray(), true, true);
		finalMesh.RecalculateNormals();
		finalMesh.Optimize();
		MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = finalMesh;
	}

	CombineInstance createCombineInstance(Mesh mesh, Vector3 pos)
	{
		CombineInstance ci = new CombineInstance();
		ci.mesh = mesh;
		Matrix4x4 transform = new Matrix4x4();
		transform.SetTRS(pos, Quaternion.identity, new Vector3(1, 1, 1));
		ci.transform = transform;
		return ci;
	}

	Mesh createXYQuad(float z)
	{
		Vector3[] v = new Vector3[4];
		Vector2[] uv = new Vector2[4];
		int[] tri = new int[6];

		v[0].x = 0;    v[0].y = 0;    v[0].z = z;
		v[1].x = 0;    v[1].y = rows; v[1].z = z;
		v[2].x = cols; v[2].y = rows; v[2].z = z;
		v[3].x = cols; v[3].y = 0;    v[3].z = z;

		uv[0].x = 0; uv[0].y = 0;
		uv[1].x = 0; uv[1].y = 1;
		uv[2].x = 1; uv[2].y = 1;
		uv[3].x = 1; uv[3].y = 0;

		tri[0] = 0;
		tri[1] = 1;
		tri[2] = 3;
		tri[3] = 3;
		tri[4] = 1;
		tri[5] = 2;

		Mesh mesh = new Mesh();
		mesh.vertices = v;
		mesh.uv = uv;
		mesh.triangles = tri;

		return mesh;
	}
}
