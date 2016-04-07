using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileLayer
{
	public abstract void moveTile(Vector2 tileMatrixCoord, Vector2 tileGridCoord, Vector3 tileWorldPos);
}

public class TileEngine {

	int m_matrixColumnCount;
	int m_matrixColumnCountHalf;

	int m_tileWidth;
	float m_tileWidthHalf;

	int m_playerTileCoordX;
	int m_playerTileCoordZ;

	int m_matrixTopIndex;
	int m_matrixRightIndex;

	List<TileLayer> m_tileLayerList;

	public TileEngine(int columnCount, int tileWorldWidth)
	{
		m_tileLayerList = new List<TileLayer>();
		m_matrixColumnCount = columnCount;
		m_matrixColumnCountHalf = m_matrixColumnCount / 2;
		m_tileWidth = tileWorldWidth;
		m_tileWidthHalf = m_tileWidth / 2;
		m_matrixTopIndex = m_matrixColumnCount - 1;
		m_matrixRightIndex = m_matrixColumnCount - 1;
	}

	public void addTileLayer(TileLayer tileLayer)
	{
		m_tileLayerList.Add(tileLayer);
	}

	public int columnCount()
	{
		return m_matrixColumnCount;
	}

	public int tileWidth()
	{
		return m_tileWidth;
	}

	public Vector3 gridCoordToWorldPos(Vector2 gridCoord)
	{
		// Return the 'bottom left' corner of the tile 
		float x = (gridCoord.x * m_tileWidth) + m_tileWidthHalf;
		float z = (gridCoord.y * m_tileWidth) + m_tileWidthHalf;
		return new Vector3(x, 0, z);
	}

	public void startx(Vector3 playerPos)
	{
		m_playerTileCoordX = Mathf.FloorToInt((playerPos.x + m_tileWidthHalf) / m_tileWidth);
		m_playerTileCoordZ = Mathf.FloorToInt((playerPos.z + m_tileWidthHalf) / m_tileWidth);

		for (int z = 0; z < m_matrixColumnCount; ++z) {
			for (int x = 0; x < m_matrixColumnCount; ++x) {
				Vector2 tileMatrixCoord = new Vector2(x, z);
				Vector2 tileGridCoord = new Vector2(x - m_matrixColumnCountHalf, z - m_matrixColumnCountHalf);
				Vector3 worldPos = gridCoordToWorldPos(tileGridCoord);
				foreach (TileLayer tileLayer in m_tileLayerList)
					tileLayer.moveTile(tileMatrixCoord, tileGridCoord, worldPos);
			}
		}
	}

	public void update(Vector3 playerPos)
	{
		int prevTileCoordX = m_playerTileCoordX;
		m_playerTileCoordX = Mathf.FloorToInt((playerPos.x + m_tileWidthHalf) / m_tileWidth);
		if (m_playerTileCoordX != prevTileCoordX)
			updateTiles(m_playerTileCoordX, prevTileCoordX, ref m_matrixRightIndex, false);

		int prevTileCoordZ = m_playerTileCoordZ;
		m_playerTileCoordZ = Mathf.FloorToInt((playerPos.z + m_tileWidthHalf) / m_tileWidth);
		if (prevTileCoordZ != m_playerTileCoordZ)
			updateTiles(m_playerTileCoordZ, prevTileCoordZ, ref m_matrixTopIndex, true);
	}

	private void updateTiles(int currentTileCoord, int prevTileCoord, ref int matrixFrontIndex, bool updateZAxis)
	{
		int tilesCrossed = currentTileCoord - prevTileCoord;
		int moveDirection = tilesCrossed > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossed), m_matrixColumnCount);

		// Update matrix pointer, which is passed as ref
		matrixFrontIndex = (m_matrixColumnCount + matrixFrontIndex + (tilesCrossed % m_matrixColumnCount)) % m_matrixColumnCount;

		for (int i = 0; i < nuberOfRowsToUpdate; ++i) {
			// Get the matrix row that contains tiles that are out of sight, and move it in front of the player
			int matrixRowOrColToReuse = (m_matrixColumnCount + matrixFrontIndex + (i * -moveDirection)) % m_matrixColumnCount;
			if (moveDirection < 0) {
				// When moving "backwards", reuse the new bottom index instead
				matrixRowOrColToReuse = (matrixRowOrColToReuse + 1) % m_matrixColumnCount;
			}

			int tileCoordXorZ = moveDirection > 0 ?
				currentTileCoord + m_matrixColumnCountHalf - i - 1 :
				currentTileCoord - m_matrixColumnCountHalf + i;

			if (updateZAxis) {
				for (int j = 0; j < m_matrixColumnCount; ++j) {
					int matrixCol = (m_matrixColumnCount + m_matrixRightIndex - j) % m_matrixColumnCount;
					Vector2 tileMatrixCoord = new Vector2(matrixCol, matrixRowOrColToReuse);
					Vector2 tileGridCoord = new Vector2(m_playerTileCoordX + m_matrixColumnCountHalf - j - 1, tileCoordXorZ);
					Vector3 worldPos = gridCoordToWorldPos(tileGridCoord);
					foreach (TileLayer tileLayer in m_tileLayerList)
						tileLayer.moveTile(tileMatrixCoord, tileGridCoord, worldPos);
				}
			} else {
				for (int j = 0; j < m_matrixColumnCount; ++j) {
					int matrixRow = (m_matrixColumnCount + m_matrixTopIndex - j) % m_matrixColumnCount;
					Vector2 tileMatrixCoord = new Vector2(matrixRowOrColToReuse, matrixRow);
					Vector2 tileGridCoord = new Vector2(tileCoordXorZ, m_playerTileCoordZ + m_matrixColumnCountHalf - j - 1);
					Vector3 worldPos = gridCoordToWorldPos(tileGridCoord);
					foreach (TileLayer tileLayer in m_tileLayerList)
						tileLayer.moveTile(tileMatrixCoord, tileGridCoord, worldPos);
				}
			}
		}
	}

}
