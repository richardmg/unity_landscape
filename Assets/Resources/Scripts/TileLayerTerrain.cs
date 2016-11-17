using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerTerrain : MonoBehaviour
{
	[Range (33, 512)]
	public int heightmapResolution = 33;
	[Range (0, 200)]
	public int pixelError = 50;
	[Range (0.1f, 5)]
	public float textureScale = 1;
	public Texture2D terrainTexture;

	GameObject[,] m_tileMatrix;
	Terrain[,] m_terrainMatrix;
	TerrainData m_terrainData;
	public float[,] m_heightArray;
	public TileEngine tileEngine;

	public static TileLayerTerrain worldTerrain;

	[HideInInspector]
	public float noiseScaleOct0 = 0.003f;
	[HideInInspector]
	public float noiseScaleOct1 = 0.02f;
	[HideInInspector]
	public float noiseScaleOct2 = 0.1f;

	public void Awake()
	{
		TileLayerTerrain.worldTerrain = this;
		tileEngine = new TileEngine(4, 1000);
		initTiles();
		tileEngine.updateAllTiles(updateTiles);
	}

	public void initTiles()
	{
		int tileCount = tileEngine.tileCount;

		m_tileMatrix = new GameObject[tileCount, tileCount];
		m_terrainMatrix = new Terrain[tileCount, tileCount];
		m_heightArray = new float[heightmapResolution, heightmapResolution];

		LandscapeDescription desc = new LandscapeDescription();
		desc.size = tileEngine.tileWorldSize;
		desc.pixelError = pixelError;
		desc.resolution = heightmapResolution;
		desc.texture = terrainTexture;
		desc.textureScale = textureScale;

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject go = LandscapeTools.createTerrainGameObject(desc);
				go.transform.SetParent(transform, false);
				go.name = "MatrixTile " + x + ", " + z;
				m_tileMatrix[x, z] = go;
				m_terrainMatrix[x, z] = go.GetComponent<Terrain>();
			}
		}
	}

	public void Update()
	{
		tileEngine.updateTiles(Root.instance.player.transform.position, updateTiles);
	}

	public void removeAllTiles()
	{
		for (int i = 0; i < transform.childCount; ++i) {
			GameObject go = transform.GetChild(i).gameObject;
			UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(go); };
		}
	}

	void updateTiles(TileDescription[] tilesToUpdate)
	{
		updateTileGeometry(tilesToUpdate);
		updateTileNeighbours(tilesToUpdate);
	}

	public void updateAllTiles()
	{
		tileEngine.updateAllTiles(updateTiles);
	}

	void updateTileGeometry(TileDescription[] tilesToUpdate)
	{
		for (int i = 0; i < tilesToUpdate.Length; ++i) {
			TileDescription desc = tilesToUpdate[i];
			GameObject go = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			go.transform.localPosition = desc.worldPos;

			TerrainData tdata = m_terrainMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y].terrainData;
			Vector3 scale = tdata.heightmapScale;

			for (int x = 0; x < heightmapResolution; ++x) {
				for (int z = 0; z < heightmapResolution; ++z) {
					float height = Root.instance.landscapeManager.calculateHeight(desc.worldPos.x + (x * scale.x), desc.worldPos.z + (z * scale.z));
					m_heightArray[z, x] = height / scale.y;
				}
			}

			tdata.SetHeights(0, 0, m_heightArray);
		}
	}

	void updateTileNeighbours(TileDescription[] tilesWithNewNeighbours)
	{
		for (int i = 0; i < tilesWithNewNeighbours.Length; ++i) {
			TileDescription desc = tilesWithNewNeighbours[i];

			TileNeighbours tn = desc.neighbours;
			Terrain top = getTerrainSafe(tn.top);
			Terrain bottom = getTerrainSafe(tn.bottom);
			Terrain left = getTerrainSafe(tn.left);
			Terrain right = getTerrainSafe(tn.right);

			Terrain terrain = m_terrainMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			terrain.SetNeighbors(left, top, right, bottom);
		}
	}

	Terrain getTerrainSafe(Vector2 matrixPos)
	{
		return (int)matrixPos.x == -1 ? null : m_terrainMatrix[(int)matrixPos.x, (int)matrixPos.y];
	}

	public float sampleHeight(Vector3 worldPos)
	{
		int tileX, tileZ;
		int matrixX, matrixY;
		tileEngine.tileCoordAtWorldPos(worldPos, out tileX, out tileZ);
		tileEngine.matrixCoordForTileCoord(tileX, tileZ, out matrixX, out matrixY);
		return m_terrainMatrix[matrixX, matrixY].SampleHeight(worldPos);
	}

}
