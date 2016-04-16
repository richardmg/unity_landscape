using UnityEngine;
using System.Collections;

public class TileGround : MonoBehaviour {

	public void moveTile(Vector2 tileGridCoord, Vector3 tileWorldPos)
	{
		transform.position = tileWorldPos;

		float scale = 0.15f;
		Mesh mesh = GetComponent<MeshFilter>().mesh;

		// todo: will this create a copy of the array? If so, can it be avoided?
		Vector3[] vertices = mesh.vertices;

		for (int i = 0; i < vertices.Length; ++i)
			vertices[i].y = Mathf.PerlinNoise((vertices[i].x + tileWorldPos.x) * scale, (vertices[i].z + tileWorldPos.z) * scale);
		mesh.vertices = vertices;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

//		Terrain terrain = GetComponent<Terrain>();
//		TerrainData tdata = terrain.terrainData;
//		int w = (int)tdata.size.x;
//		int h = (int)tdata.size.z;
////		float[,] heights = tdata.GetHeights(0, 0, w, h);
//		float[,] heights = new float[w, h];
//		for (int x = 0; x < w; ++x)
//			for (int z = 0; z < h; ++z) 
//				heights[x, z] = Mathf.PerlinNoise(x * scale, z * scale) * 5;
//
//		// todo: check if GetHeights make a copy of the array, or if a copy on write happens. If so, we should
//		// avoid creating arrays all the time....
//		tdata.SetHeights(0, 0, heights);
	}
}
