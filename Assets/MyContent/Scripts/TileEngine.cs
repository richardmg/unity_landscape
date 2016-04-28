using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileNeighbours
{
	public Vector2 left = new Vector2();
	public Vector2 right = new Vector2();
	public Vector2 top = new Vector2();
	public Vector2 bottom = new Vector2();
}

public class TileDescription
{
	public Vector2 gridCoord = new Vector2();
	public Vector3 worldPos = new Vector3();
	public Vector2 matrixCoord = new Vector2();
	public TileNeighbours neighbours = new TileNeighbours();
}

public interface ITileLayer
{
	void initTiles(TileDescription[] tilesToInit);
	void moveTiles(TileDescription[] tilesToMove);
	void updateTileNeighbours(TileDescription[] tilesWithNewNeighbours);
}

public interface ITile
{
	void initTile(TileDescription desc, GameObject gameObject);
	void moveTile(TileDescription desc, GameObject gameObject);
}

public class TileEngine {

	int m_matrixSize;
	int m_matrixSizeHalf;
	int m_worldTileWidth;
	Vector3 m_gridCenterOffset;
	Vector2 m_gridCenter;
	Vector2 m_matrixTopRight = new Vector2();
	TileDescription[] m_tileMoveDesc;

	List<ITileLayer> m_tileLayerList;

	public TileEngine(int rowCount, int tileWorldWidth)
	{
		m_tileLayerList = new List<ITileLayer>();
		m_matrixSize = rowCount;
		m_matrixSizeHalf = m_matrixSize / 2;
		m_worldTileWidth = tileWorldWidth;
		m_gridCenterOffset = new Vector3(m_worldTileWidth / 2, 0, m_worldTileWidth / 2);
		m_tileMoveDesc = new TileDescription[m_matrixSize];
		for (int i = 0; i < m_matrixSize; ++i)
			m_tileMoveDesc[i] = new TileDescription();

		Debug.AssertFormat(m_matrixSize >= 2, "TileEngine: column count must be greater than or equal to 2");
		Debug.AssertFormat(m_worldTileWidth > 0, "TileEngine: tile width must be greater than 0");
	}

	public void addTileLayer(ITileLayer tileLayer)
	{
		m_tileLayerList.Add(tileLayer);
	}

	void setWorldPosFromGridPos(Vector2 gridCoord, ref Vector3 worldPos)
	{
		float x = gridCoord.x * m_worldTileWidth;
		float z = gridCoord.y * m_worldTileWidth;
		worldPos.Set(x, 0, z);
	}

	void setGridPosFromWorldPos(Vector3 worldPos, ref Vector2 gridCoord)
	{
		int x = Mathf.FloorToInt(worldPos.x / m_worldTileWidth);
		int z = Mathf.FloorToInt(worldPos.z / m_worldTileWidth);
		gridCoord.Set(x, z);
	}

	int matrixPos(int top, int rows)
	{
		return (m_matrixSize + top + (rows % m_matrixSize)) % m_matrixSize;
	}

	void setNeighbours(Vector2 pos, ref TileNeighbours result)
	{
		int matrixTopEdge = (int)m_matrixTopRight.y;
		int matrixBottomEdge = matrixPos(matrixTopEdge, 1);
		int matrixRightEdge = (int)m_matrixTopRight.x;
		int matrixLeftEdge = matrixPos(matrixRightEdge, 1);

		bool onTopEdge = ((int)pos.y == matrixTopEdge);
		bool onBottomEdge = ((int)pos.y == matrixBottomEdge);
		bool onLeftEdge = ((int)pos.x == matrixLeftEdge);
		bool onRightEdge = ((int)pos.x == matrixRightEdge);

		if (onTopEdge) result.top.Set(-1, -1); else result.top.Set(pos.x, matrixPos((int)pos.y, 1));
		if (onBottomEdge) result.bottom.Set(-1, -1); else result.bottom.Set(pos.x, matrixPos((int)pos.y, -1));
		if (onLeftEdge) result.left.Set(-1, -1); else result.left.Set(matrixPos((int)pos.x, -1), pos.y);
		if (onRightEdge) result.right.Set(-1, -1); else result.right.Set(matrixPos((int)pos.x, 1), pos.y);
	}

	public void start(Vector3 playerPos)
	{
		m_matrixTopRight.Set(m_matrixSize - 1, m_matrixSize - 1);
		setGridPosFromWorldPos(playerPos + m_gridCenterOffset, ref m_gridCenter);

		for (int z = 0; z < m_matrixSize; ++z) {
			for (int x = 0; x < m_matrixSize; ++x) {
				m_tileMoveDesc[x].matrixCoord.Set(x, z);
				m_tileMoveDesc[x].gridCoord.Set(
					x + (int)m_gridCenter.x - m_matrixSizeHalf,
					z + (int)m_gridCenter.y - m_matrixSizeHalf);

				setWorldPosFromGridPos(m_tileMoveDesc[x].gridCoord, ref m_tileMoveDesc[x].worldPos);
				setNeighbours(m_tileMoveDesc[x].matrixCoord, ref m_tileMoveDesc[x].neighbours);

				foreach (ITileLayer tileLayer in m_tileLayerList) {
					tileLayer.initTiles(m_tileMoveDesc);
					tileLayer.updateTileNeighbours(m_tileMoveDesc);
				}
			}
		}
	}

	public void update(Vector3 playerPos)
	{
		Vector2 gridCenterPrev = m_gridCenter;
		setGridPosFromWorldPos(playerPos + m_gridCenterOffset, ref m_gridCenter);

		if (m_gridCenter == gridCenterPrev)
			return;

		int gridCrossedX = (int)m_gridCenter.x - (int)gridCenterPrev.x;
		int gridCrossedZ = (int)m_gridCenter.y - (int)gridCenterPrev.y;
		m_matrixTopRight.Set((float)matrixPos((int)m_matrixTopRight.x, gridCrossedX), (float)matrixPos((int)m_matrixTopRight.y, gridCrossedZ));

		if (gridCrossedX != 0)
			updateTilesX(gridCrossedX);

		if (gridCrossedZ != 0)
			updateTilesZ(gridCrossedZ);
	}

	private void updateTilesX(int tilesCrossedX)
	{
		int moveDirection = tilesCrossedX > 0 ? 1 : -1;
		int nuberOfColsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedX), m_matrixSize);

		for (int i = 0; i <= nuberOfColsToUpdate; ++i) {
			int matrixLoopFrontX = matrixPos((int)m_matrixTopRight.x, i * -moveDirection);
			if (moveDirection < 0)
				matrixLoopFrontX = matrixPos(matrixLoopFrontX, 1);

			int coordCenterX = moveDirection > 0 ?
				(int)m_gridCenter.x + m_matrixSizeHalf - i - 1 :
				(int)m_gridCenter.x - m_matrixSizeHalf + i;

			for (int j = 0; j < m_matrixSize; ++j) {
				int matrixLoopFrontZ = matrixPos((int)m_matrixTopRight.y, -j);
				int coordCenterZ = (int)m_gridCenter.y + m_matrixSizeHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(coordCenterX, coordCenterZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixLoopFrontX, matrixLoopFrontZ);

				setWorldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerList) {
				if (i < nuberOfColsToUpdate)
					tileLayer.moveTiles(m_tileMoveDesc);
				tileLayer.updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

	private void updateTilesZ(int tilesCrossedZ)
	{
		int moveDirection = tilesCrossedZ > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedZ), m_matrixSize);

		for (int i = 0; i <= nuberOfRowsToUpdate; ++i) {
			int matrixLoopFrontZ = matrixPos((int)m_matrixTopRight.y, i * -moveDirection);
			if (moveDirection < 0)
				matrixLoopFrontZ = matrixPos(matrixLoopFrontZ, 1);

			int coordCenterZ = moveDirection > 0 ?
				(int)m_gridCenter.y + m_matrixSizeHalf - i - 1 :
				(int)m_gridCenter.y - m_matrixSizeHalf + i;

			for (int j = 0; j < m_matrixSize; ++j) {
				int matrixLoopFrontX = matrixPos((int)m_matrixTopRight.x, -j);
				int coordCenterX = (int)m_gridCenter.x + m_matrixSizeHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(coordCenterX, coordCenterZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixLoopFrontX, matrixLoopFrontZ);

				setWorldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerList) {
				if (i < nuberOfRowsToUpdate)
					tileLayer.moveTiles(m_tileMoveDesc);
				tileLayer.updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}
}
