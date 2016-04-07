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
		m_matrixTopIndex = m_matrixColumnCount - 1;
		m_matrixRightIndex = m_matrixColumnCount - 1;
		m_playerTileCoordX = int.MaxValue;
		m_playerTileCoordZ = int.MaxValue;
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

	private float tileCoordToWorldPos(int pos)
	{
		// Return the 'bottom left' corner of the tile 
		return (pos * m_tileWidth) + (m_tileWidth / 2);
	}

	public void update(Vector3 playerPos) {
		int newTileCoordX = Mathf.FloorToInt((playerPos.x + (m_tileWidth / 2)) / m_tileWidth);
		int newTileCoordZ = Mathf.FloorToInt((playerPos.z + (m_tileWidth / 2)) / m_tileWidth);
		if (m_playerTileCoordZ != newTileCoordZ)
			updateTiles(ref m_playerTileCoordZ, newTileCoordZ, ref m_matrixTopIndex, true);
		if (m_playerTileCoordX != newTileCoordX)
			updateTiles(ref m_playerTileCoordX, newTileCoordX, ref m_matrixRightIndex, false);
	}

	private void updateTiles(ref int oldTileCoord, int newTileCoord, ref int matrixFrontIndex, bool updateZAxis)
	{
		int tilesCrossed = newTileCoord - oldTileCoord;
		int moveDirection = tilesCrossed > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossed), m_matrixColumnCount);
		int newMatrixFrontIndex = (m_matrixColumnCount + matrixFrontIndex + (tilesCrossed % m_matrixColumnCount)) % m_matrixColumnCount;

		for (int i = 0; i < nuberOfRowsToUpdate; ++i) {
			// Get the matrix row that contains tiles that are out of sight, and move it in front of the player
			int matrixRowOrColToReuse = (m_matrixColumnCount + newMatrixFrontIndex + (i * -moveDirection)) % m_matrixColumnCount;
			if (moveDirection < 0) {
				// When moving "backwards", reuse the new bottom index instead
				matrixRowOrColToReuse = (matrixRowOrColToReuse + 1) % m_matrixColumnCount;
			}

			int tileCoordXorZ = moveDirection > 0 ?
				newTileCoord + m_matrixColumnCountHalf - i - 1 :
				newTileCoord - m_matrixColumnCountHalf + i;

			if (updateZAxis) {
				for (int col = 0; col < m_matrixColumnCount; ++col) {
					foreach (TileLayer tileLayer in m_tileLayerList) {
						Vector2 tileMatrixCoord = new Vector2(col, matrixRowOrColToReuse);
						Vector2 tileGridCoord = new Vector2(m_playerTileCoordX, tileCoordXorZ);
						Vector3 worldPos = new Vector3(tileCoordToWorldPos(m_playerTileCoordX), 0, tileCoordToWorldPos(tileCoordXorZ));
						tileLayer.moveTile(tileMatrixCoord, tileGridCoord, worldPos);
					}
				}
			} else {
				for (int row = 0; row < m_matrixColumnCount; ++row) {
					foreach (TileLayer tileLayer in m_tileLayerList) {
						Vector2 tileMatrixCoord = new Vector2(matrixRowOrColToReuse, row);
						Vector2 tileGridCoord = new Vector2(tileCoordXorZ, m_playerTileCoordZ);
						Vector3 worldPos = new Vector3(tileCoordToWorldPos(tileCoordXorZ), 0, tileCoordToWorldPos(m_playerTileCoordZ));
						tileLayer.moveTile(tileMatrixCoord, tileGridCoord, worldPos);
					}
				}
			}
		}

		matrixFrontIndex = newMatrixFrontIndex;
		oldTileCoord = newTileCoord;
	}

}
