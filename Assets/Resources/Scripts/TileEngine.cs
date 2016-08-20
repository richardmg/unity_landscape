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
	void initTileLayer(TileEngine engine);
	void moveTiles(TileDescription[] tilesToMove);
}

public interface ITileTerrainLayer : ITileLayer
{
	void updateTileNeighbours(TileDescription[] tilesWithNewNeighbours);
}

public class TileEngine {
	int m_tileCount;
	int m_tileCountHalf;
	float m_tileWorldSize;
	Vector3 m_gridCenterOffset;
	Vector2 m_gridCenter;
	Vector2 m_matrixTopRight = new Vector2();
	TileDescription[] m_tileMoveDesc;
	Transform m_parentTransform;

	List<ITileLayer> m_tileLayerList;

	public TileEngine(int tileCount, float tileWorldSize, Transform parentTransform)
	{
		m_tileLayerList = new List<ITileLayer>();
		m_tileCount = tileCount;
		m_tileCountHalf = m_tileCount / 2;
		m_tileWorldSize = tileWorldSize;
		m_gridCenterOffset = new Vector3(m_tileWorldSize / 2, 0, m_tileWorldSize / 2);
		m_parentTransform = parentTransform;
		m_tileMoveDesc = new TileDescription[m_tileCount];
		for (int i = 0; i < m_tileCount; ++i)
			m_tileMoveDesc[i] = new TileDescription();

		Debug.AssertFormat(m_tileCount >= 2, "TileEngine: column count must be greater than or equal to 2");
		Debug.AssertFormat(m_tileWorldSize > 0, "TileEngine: tile width must be greater than 0");
	}

	public int tileCount() { return m_tileCount; }
	public float tileWorldSize() { return m_tileWorldSize; }
	public Transform parentTransform() { return m_parentTransform; }

	public void addLayer(ITileLayer tileLayer)
	{
		m_tileLayerList.Add(tileLayer);
	}

	void setWorldPosFromGridPos(Vector2 gridCoord, ref Vector3 worldPos)
	{
		float x = gridCoord.x * m_tileWorldSize;
		float z = gridCoord.y * m_tileWorldSize;
		worldPos.Set(x, 0, z);
	}

	void setGridPosFromWorldPos(Vector3 worldPos, ref Vector2 gridCoord)
	{
		int x = Mathf.FloorToInt(worldPos.x / m_tileWorldSize);
		int z = Mathf.FloorToInt(worldPos.z / m_tileWorldSize);
		gridCoord.Set(x, z);
	}

	int matrixPos(int top, int rows)
	{
		return (m_tileCount + top + (rows % m_tileCount)) % m_tileCount;
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
		m_matrixTopRight.Set(m_tileCount - 1, m_tileCount - 1);
		setGridPosFromWorldPos(playerPos + m_gridCenterOffset, ref m_gridCenter);

		foreach (ITileLayer tileLayer in m_tileLayerList)
			tileLayer.initTileLayer(this);

		updateAllTiles();
	}

	public void updateAllTiles()
	{
		for (int z = 0; z < m_tileCount; ++z) {
			for (int x = 0; x < m_tileCount; ++x) {
				m_tileMoveDesc[x].matrixCoord.Set(x, z);
				m_tileMoveDesc[x].gridCoord.Set(
					x + (int)m_gridCenter.x - m_tileCountHalf,
					z + (int)m_gridCenter.y - m_tileCountHalf);

				setWorldPosFromGridPos(m_tileMoveDesc[x].gridCoord, ref m_tileMoveDesc[x].worldPos);
				setNeighbours(m_tileMoveDesc[x].matrixCoord, ref m_tileMoveDesc[x].neighbours);

				foreach (ITileLayer tileLayer in m_tileLayerList) {
					tileLayer.moveTiles(m_tileMoveDesc);
					if (tileLayer is ITileTerrainLayer)
						((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
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
		int nuberOfColsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedX), m_tileCount);

		for (int i = 0; i <= nuberOfColsToUpdate; ++i) {
			int matrixLoopFrontX = matrixPos((int)m_matrixTopRight.x, i * -moveDirection);
			if (moveDirection < 0)
				matrixLoopFrontX = matrixPos(matrixLoopFrontX, 1);

			int coordCenterX = moveDirection > 0 ?
				(int)m_gridCenter.x + m_tileCountHalf - i - 1 :
				(int)m_gridCenter.x - m_tileCountHalf + i;

			for (int j = 0; j < m_tileCount; ++j) {
				int matrixLoopFrontZ = matrixPos((int)m_matrixTopRight.y, -j);
				int coordCenterZ = (int)m_gridCenter.y + m_tileCountHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(coordCenterX, coordCenterZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixLoopFrontX, matrixLoopFrontZ);

				setWorldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerList) {
				if (i < nuberOfColsToUpdate)
					tileLayer.moveTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

	private void updateTilesZ(int tilesCrossedZ)
	{
		int moveDirection = tilesCrossedZ > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedZ), m_tileCount);

		for (int i = 0; i <= nuberOfRowsToUpdate; ++i) {
			int matrixLoopFrontZ = matrixPos((int)m_matrixTopRight.y, i * -moveDirection);
			if (moveDirection < 0)
				matrixLoopFrontZ = matrixPos(matrixLoopFrontZ, 1);

			int coordCenterZ = moveDirection > 0 ?
				(int)m_gridCenter.y + m_tileCountHalf - i - 1 :
				(int)m_gridCenter.y - m_tileCountHalf + i;

			for (int j = 0; j < m_tileCount; ++j) {
				int matrixLoopFrontX = matrixPos((int)m_matrixTopRight.x, -j);
				int coordCenterX = (int)m_gridCenter.x + m_tileCountHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(coordCenterX, coordCenterZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixLoopFrontX, matrixLoopFrontZ);

				setWorldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerList) {
				if (i < nuberOfRowsToUpdate)
					tileLayer.moveTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

}
