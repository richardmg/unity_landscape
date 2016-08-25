using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerTerrain : MonoBehaviour, ITileTerrainLayer
{
	[Range (8, 512)]
	public int groundResolution = 33;
	[Range (0, 200)]
	public int pixelError = 50;
	public Texture2D terrainTexture;

	GameObject[,] m_tileMatrix;
	TerrainData m_terrainData;
	public float[,] m_heightArray;

	public void initTileLayer(TileEngine engine)
	{
		int tileCount = engine.tileCount;
		m_tileMatrix = new GameObject[tileCount, tileCount];

		LandscapeDescription desc = new LandscapeDescription();
		desc.size = engine.tileSize;
		desc.pixelError = pixelError;
		desc.resolution = groundResolution;
		desc.texture = terrainTexture;

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				m_tileMatrix[x, z] = LandscapeTools.createTerrainGameObject(desc);
				m_tileMatrix[x, z].transform.SetParent(transform, false);
			}
		}

		TerrainData data = m_tileMatrix[0, 0].GetComponent<Terrain>().terrainData;
		m_heightArray = new float[data.heightmapResolution, data.heightmapResolution];
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject tileObject = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			Terrain terrain = tileObject.GetComponent<Terrain>();
			TerrainData tdata = terrain.terrainData;

			tileObject.transform.localPosition = desc.worldPos;

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

	public void removeAllTiles()
	{
		for (int i = 0; i < transform.childCount; ++i) {
			GameObject go = transform.GetChild(i).gameObject;
			UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(go); };
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
