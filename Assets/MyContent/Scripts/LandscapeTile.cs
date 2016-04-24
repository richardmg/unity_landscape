using UnityEngine;
using System.Collections;

public class LandscapeTile : MonoBehaviour, ITile {

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
	}

	public void moveTile(TileMoveDescription desc)
	{
		transform.position = desc.tileWorldPos;

		Terrain terrain = GetComponent<Terrain>();
		TerrainData tdata = terrain.terrainData;
		int res = tdata.heightmapResolution;
		Vector3 scale = tdata.heightmapScale;
		float[,] heights = new float[res, res];

		for (int x = 0; x < res; ++x) {
			for (int z = 0; z < res; ++z) {
				float height = LandscapeConstructor.getGroundHeight(desc.tileWorldPos.x + (x * scale.x), desc.tileWorldPos.z + (z * scale.z));
				heights[z, x] = height / scale.y;
			}
		}

		// todo: check if GetHeights make a copy of the array, or if a copy on write happens. If so, we should
		// avoid creating arrays all the time....
		tdata.SetHeights(0, 0, heights);
	}
}
