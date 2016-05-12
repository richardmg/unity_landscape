using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerTerrain : ITileTerrainLayer
{
	GameObject m_layerRoot;
	GameObject[,] m_tileMatrix;
	Texture2D m_terrainTexture;
	public float[,] m_heightArray;

	public TileLayerTerrain(string name, Texture2D terrainTexture, Transform parentTransform)
	{
		m_terrainTexture = terrainTexture;
		m_layerRoot = new GameObject(name);
		m_layerRoot.transform.SetParent(parentTransform);
		m_heightArray = new float[33, 33];
	}

	public void initTileResources(int tileCount, float tileWorldSize)
	{
		m_tileMatrix = new GameObject[tileCount, tileCount];
		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				m_tileMatrix[x, z] = Terrain.CreateTerrainGameObject(createTerrainData());
				m_tileMatrix[x, z].transform.SetParent(m_layerRoot.transform);
			}
		}
	}

	public void initTiles(TileDescription[] tilesToInit)
	{
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

	TerrainData createTerrainData()
	{
		TerrainData data = new TerrainData();
		data.alphamapResolution = 512;
		data.baseMapResolution = 1024;
		data.SetDetailResolution(384, 16);
		data.heightmapResolution = 33;
		data.size = new Vector3(1000, 200, 1000);

		SplatPrototype[] splatArray = new SplatPrototype[1]; 
		splatArray[0] = new SplatPrototype(); 
		splatArray[0].texture = m_terrainTexture;
		data.splatPrototypes = splatArray;  

		return data;
	}
}
