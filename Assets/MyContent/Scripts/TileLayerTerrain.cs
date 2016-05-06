using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerTerrain : ITileTerrainLayer
{
	GameObject[,] m_tileMatrix;
	public float[,] m_heightArray;

	public TileLayerTerrain(string name, GameObject prefab, Transform parentTransform)
	{
		Terrain terrain = prefab.GetComponent<Terrain>();
		Debug.AssertFormat(terrain != null, this.GetType().Name + ": prefab needs to have a Terrain component");
		int res = terrain.terrainData.heightmapResolution;
		m_heightArray = new float[res, res];

		int count = LandscapeConstructor.m_instance.rows;
		m_tileMatrix = new GameObject[count, count];
		Transform childRoot = parentTransform.Find(name);

		// If a child with name already exists, adopt its
		// children instead of creating new ones.
		if (childRoot == null)
			constructNewGameobjects(name, prefab, parentTransform);
		else
			adoptChildren(childRoot);

	}

	public void constructNewGameobjects(string name, GameObject prefab, Transform parentTransform)
	{
		Transform tilesParent = new GameObject(name).transform;
		tilesParent.SetParent(parentTransform);
		int count = m_tileMatrix.GetLength(0);

		for (int z = 0; z < count; ++z) {
			for (int x = 0; x < count; ++x) {
				m_tileMatrix[x, z] = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
				m_tileMatrix[x, z].transform.SetParent(tilesParent);
			}
		}
	}

	public void adoptChildren(Transform childrenRoot)
	{
		int i = 0;
		int count = m_tileMatrix.GetLength(0);

		foreach (Transform child in childrenRoot) {
			m_tileMatrix[i % count, (int)(i / count)] = child.gameObject;
			i++;
		}
	}

	public void initTiles(TileDescription[] tilesToInit)
	{
		for (int i = 0; i < tilesToInit.Length; ++i) {
			TileDescription desc = tilesToInit[i];
			GameObject tileObject = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			Terrain terrain = tileObject.GetComponent<Terrain>();
			TerrainData tdata = terrain.terrainData;

			if (desc.gridCoord.x == 0 && desc.gridCoord.y == 0) {
				Vector3 scale = tdata.heightmapScale;
				float w = tdata.size.x;
				float l = tdata.size.z;
				Debug.AssertFormat(scale.y == LandscapeConstructor.m_instance.landscapeHeightLargeScale, "LandscapeTile: Landscape height needs to match global height function");
				Debug.AssertFormat(w == l && w == (float)LandscapeConstructor.m_instance.tileWidth, "LandscapeTile: landscape size needs to be the same as in tileWidth");
			}

			terrain.terrainData = LandscapeTools.Clone(tdata);
			tileObject.GetComponent<TerrainCollider>().terrainData = terrain.terrainData;
		}

		moveTiles(tilesToInit);
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject tileObject = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			Terrain terrain = tileObject.GetComponent<Terrain>();
			TerrainData tdata = terrain.terrainData;

			tileObject.transform.position = desc.worldPos;

			int res = tdata.heightmapResolution;
			Vector3 scale = tdata.heightmapScale;

			for (int x = 0; x < res; ++x) {
				for (int z = 0; z < res; ++z) {
					float height = LandscapeConstructor.getGroundHeight(desc.worldPos.x + (x * scale.x), desc.worldPos.z + (z * scale.z));
					m_heightArray[z, x] = height / scale.y;
				}
			}

			tdata.SetHeights(0, 0, m_heightArray);
		}
	}

	public void updateTileNeighbours(TileDescription[] tilesWithNewNeighbours)
	{
		for (int i = 0; i < tilesWithNewNeighbours.Length; ++i) {
			TileDescription desc = tilesWithNewNeighbours[i];
			GameObject tileObject = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			Terrain terrain = tileObject.GetComponent<Terrain>();
			TileNeighbours tn = tilesWithNewNeighbours[i].neighbours;

			Terrain top = getTerrainSafe(tn.top);
			Terrain bottom = getTerrainSafe(tn.bottom);
			Terrain left = getTerrainSafe(tn.left);
			Terrain right = getTerrainSafe(tn.right);

			terrain.SetNeighbors(left, top, right, bottom);
		}
	}

	Terrain getTerrainSafe(Vector2 matrixPos)
	{
		return (int)matrixPos.x == -1 ? null : m_tileMatrix[(int)matrixPos.x, (int)matrixPos.y].GetComponent<Terrain>();
	}
}
