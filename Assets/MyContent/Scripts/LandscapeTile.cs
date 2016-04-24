using UnityEngine;
using System.Collections;

public class LandscapeTile : MonoBehaviour, ITile {

	public float[,] m_heightArray;

	public void initTile(GameObject gameObject, bool firstTile)
	{
		Terrain terrain = GetComponent<Terrain>();
		TerrainData tdata = terrain.terrainData;

		if (firstTile) {
			Vector3 scale = tdata.heightmapScale;
			float w = tdata.size.x;
			float l = tdata.size.z;
			Debug.AssertFormat(scale.y == LandscapeConstructor.instance.landscapeHeightLargeScale, "LandscapeTile: Landscape height needs to match global height function");
			Debug.AssertFormat(w == l && w == (float)LandscapeConstructor.instance.tileWidth, "LandscapeTile: landscape size needs to be the same as in tileWidth");
		}

		terrain.terrainData = LandscapeTools.Clone(tdata);
		gameObject.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;

		int res = tdata.heightmapResolution;
		m_heightArray = new float[res, res];
	}

	public void moveTile(TileMoveDescription desc)
	{
		transform.position = desc.tileWorldPos;

		Terrain terrain = GetComponent<Terrain>();
		TerrainData tdata = terrain.terrainData;
		int res = tdata.heightmapResolution;
		Vector3 scale = tdata.heightmapScale;

		for (int x = 0; x < res; ++x) {
			for (int z = 0; z < res; ++z) {
				float height = LandscapeConstructor.getGroundHeight(desc.tileWorldPos.x + (x * scale.x), desc.tileWorldPos.z + (z * scale.z));
				m_heightArray[z, x] = height / scale.y;
			}
		}

		tdata.SetHeights(0, 0, m_heightArray);
	}
}
