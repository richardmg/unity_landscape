using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileMoveDescription
{
	public Vector2 tileMatrixCoord;
	public Vector2 tileGridCoord;
	public Vector3 tileWorldPos;
}

public interface ITileLayer
{
	void moveTiles(TileMoveDescription[] desc, bool alongZ);
}

public interface ITile
{
	void initTile(GameObject gameObject, bool firstTile);
	void moveTile(TileMoveDescription desc);
}

public class TileEngine {

	int m_matrixRowCount;
	int m_matrixRowCountHalf;

	int m_tileWorldWidth;
	Vector3 m_changeTileOffset;
	Vector2 m_centerTileCoord;
	TileMoveDescription[] m_tileMoveDesc;

	int m_matrixTopIndex;
	int m_matrixRightIndex;

	List<ITileLayer> m_tileLayerList;

	public TileEngine(int rowCount, int tileWorldWidth)
	{
		m_tileLayerList = new List<ITileLayer>();
		m_matrixRowCount = rowCount;
		m_matrixRowCountHalf = m_matrixRowCount / 2;
		m_tileWorldWidth = tileWorldWidth;
		m_changeTileOffset = new Vector3(m_tileWorldWidth / 2, 0, m_tileWorldWidth / 2);
		m_matrixTopIndex = m_matrixRowCount - 1;
		m_matrixRightIndex = m_matrixRowCount - 1;
		m_tileMoveDesc = new TileMoveDescription[m_matrixRowCount];
		for (int i = 0; i < m_matrixRowCount; ++i)
			m_tileMoveDesc[i] = new TileMoveDescription();

		Debug.AssertFormat(m_matrixRowCount >= 2, "TileEngine: column count must be greater than or equal to 2");
		Debug.AssertFormat(m_tileWorldWidth > 0, "TileEngine: tile width must be greater than 0");
	}

	public void addTileLayer(ITileLayer tileLayer)
	{
		m_tileLayerList.Add(tileLayer);
	}

	public Vector3 gridCoordToWorldPos(Vector2 gridCoord)
	{
		float x = gridCoord.x * m_tileWorldWidth;
		float z = gridCoord.y * m_tileWorldWidth;
		return new Vector3(x, 0, z);
	}

	public Vector2 worldPosToGridCoord(Vector3 worldPos)
	{
		int x = Mathf.FloorToInt(worldPos.x / m_tileWorldWidth);
		int z = Mathf.FloorToInt(worldPos.z / m_tileWorldWidth);
		return new Vector2(x, z);
	}

	public void start(Vector3 playerPos)
	{
		m_centerTileCoord = worldPosToGridCoord(playerPos + m_changeTileOffset);

		for (int z = 0; z < m_matrixRowCount; ++z) {
			for (int x = 0; x < m_matrixRowCount; ++x) {
				m_tileMoveDesc[x].tileMatrixCoord = new Vector2(x, z);
				m_tileMoveDesc[x].tileGridCoord = new Vector2(
					x + (int)m_centerTileCoord.x - m_matrixRowCountHalf,
					z + (int)m_centerTileCoord.y - m_matrixRowCountHalf);
				m_tileMoveDesc[x].tileWorldPos = gridCoordToWorldPos(m_tileMoveDesc[x].tileGridCoord);

				foreach (ITileLayer tileLayer in m_tileLayerList)
					tileLayer.moveTiles(m_tileMoveDesc, false);
			}
		}
	}

	public void update(Vector3 playerPos)
	{
		Vector2 prevCenterTileCoord = m_centerTileCoord;
		m_centerTileCoord = worldPosToGridCoord(playerPos + m_changeTileOffset);

		if (m_centerTileCoord.x != prevCenterTileCoord.x)
			updateTiles((int)m_centerTileCoord.x, (int)prevCenterTileCoord.x, ref m_matrixRightIndex, false);

		if (m_centerTileCoord.y != prevCenterTileCoord.y)
			updateTiles((int)m_centerTileCoord.y, (int)prevCenterTileCoord.y, ref m_matrixTopIndex, true);
	}

	private void updateTiles(int currentTileCoord, int prevTileCoord, ref int matrixFrontIndex, bool updateZAxis)
	{
		int tilesCrossed = currentTileCoord - prevTileCoord;
		int moveDirection = tilesCrossed > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossed), m_matrixRowCount);

		// Update matrix pointer, which is passed as ref
		matrixFrontIndex = (m_matrixRowCount + matrixFrontIndex + (tilesCrossed % m_matrixRowCount)) % m_matrixRowCount;

		for (int i = 0; i < nuberOfRowsToUpdate; ++i) {
			// Get the matrix row that contains tiles that are out of sight, and move it in front of the player
			int matrixRowOrColToReuse = (m_matrixRowCount + matrixFrontIndex + (i * -moveDirection)) % m_matrixRowCount;
			if (moveDirection < 0) {
				// When moving "backwards", reuse the new bottom index instead
				matrixRowOrColToReuse = (matrixRowOrColToReuse + 1) % m_matrixRowCount;
			}

			int tileCoordXorZ = moveDirection > 0 ?
				currentTileCoord + m_matrixRowCountHalf - i - 1 :
				currentTileCoord - m_matrixRowCountHalf + i;

			if (updateZAxis) {
				for (int j = 0; j < m_matrixRowCount; ++j) {
					int matrixCol = (m_matrixRowCount + m_matrixRightIndex - j) % m_matrixRowCount;
					m_tileMoveDesc[j].tileMatrixCoord = new Vector2(matrixCol, matrixRowOrColToReuse);
					m_tileMoveDesc[j].tileGridCoord = new Vector2((int)m_centerTileCoord.x + m_matrixRowCountHalf - j - 1, tileCoordXorZ);
					m_tileMoveDesc[j].tileWorldPos = gridCoordToWorldPos(m_tileMoveDesc[j].tileGridCoord);
				}
			} else {
				for (int j = 0; j < m_matrixRowCount; ++j) {
					int matrixRow = (m_matrixRowCount + m_matrixTopIndex - j) % m_matrixRowCount;
					m_tileMoveDesc[j].tileMatrixCoord = new Vector2(matrixRowOrColToReuse, matrixRow);
					m_tileMoveDesc[j].tileGridCoord = new Vector2(tileCoordXorZ, (int)m_centerTileCoord.y + m_matrixRowCountHalf - j - 1);
					m_tileMoveDesc[j].tileWorldPos = gridCoordToWorldPos(m_tileMoveDesc[j].tileGridCoord);
				}
			}

			foreach (ITileLayer tileLayer in m_tileLayerList)
				tileLayer.moveTiles(m_tileMoveDesc, updateZAxis);
		}
	}
}

public class TileTerrainLayer : ITileLayer 
{
	GameObject[,] m_tileMatrix;

	public TileTerrainLayer(GameObject tilePrefab)
	{
		int count = LandscapeConstructor.instance.rows;
		m_tileMatrix = new GameObject[count, count];
		for (int z = 0; z < count; ++z) {
			for (int x = 0; x < count; ++x) {
				GameObject gameObject = (GameObject)GameObject.Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
				m_tileMatrix[x, z] = gameObject;
				ITile tile = gameObject.GetComponent<ITile>();
				Debug.AssertFormat(tile != null, "TileGroundLayer: tilePrefab needs to have a script attached that implements ITile");
				bool firstTile = (x == 0 && z == 0);
				tile.initTile(gameObject, firstTile);
			}
		}
	}

	public void moveTiles(TileMoveDescription[] desc, bool alongZ)
	{
		for (int i = 0; i < desc.Length; ++i) {
			TileMoveDescription tmd = desc[i];
			GameObject tile = m_tileMatrix[(int)tmd.tileMatrixCoord.x, (int)tmd.tileMatrixCoord.y];
			tile.GetComponent<ITile>().moveTile(tmd);
		}

//		 First go, interalte over all tiles and bind them

		// todo: shift matrix. Kanskje jeg kan hente ut dette fra tileEngine, slik at jeg setter
		// opp neighbour tileMatrixCoors allerede der?

//		int count = LandscapeConstructor.instance.rows;
//		for (int z = 0; z < count; ++z) {
//			for (int x = 0; x < count; ++x) {
//				Terrain tile  = m_tileMatrix[x, z].GetComponent<Terrain>();
//				Terrain left  = x > 0 ? m_tileMatrix[x - 1, z].GetComponent<Terrain>() : null;
//				Terrain right = x < count - 1 ? m_tileMatrix[x + 1, z].GetComponent<Terrain>() : null;
//				Terrain top  = z < count - 1 ? m_tileMatrix[x, z + 1].GetComponent<Terrain>() : null;
//				Terrain bottom  = z > 0 ? m_tileMatrix[x, z - 1].GetComponent<Terrain>() : null;
//				tile.SetNeighbors(left, top, right, bottom);
//			}
//		}

	}
}
