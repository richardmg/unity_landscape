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
	void removeAllTiles();
}

public interface ITileTerrainLayer : ITileLayer
{
	void updateTileNeighbours(TileDescription[] tilesWithNewNeighbours);
	float sampleHeight(Vector3 worldPos);
}

public class TileEngine : MonoBehaviour {
	[Range (2, 10)]
	public int tileCount = 4;
	[Range (1, 5000)]
	public float tileSize = 100;
	public GameObject player;
	public bool showInEditor = false;

	int m_tileCountHalf;
	Vector3 m_gridCenterOffset;
	Vector2 m_playerGridPos;
	Vector2 m_matrixTopRight = new Vector2();
	TileDescription[] m_tileMoveDesc;

	ITileLayer[] m_tileLayerArray;

	public void OnValidate()
	{
		removeAllTiles();
		if (!showInEditor)
			return;

		init();
		updateAllTiles();
	}

	void Start()
	{
		removeAllTiles();
		init();
		updateAllTiles();
	}

	void Update()
	{
		Vector2 prevPlayerGridPos = m_playerGridPos;
		setGridPosFromWorldPos(player.transform.position + m_gridCenterOffset, ref m_playerGridPos);

		if (m_playerGridPos == prevPlayerGridPos)
			return;

		int gridCrossedX = (int)m_playerGridPos.x - (int)prevPlayerGridPos.x;
		int gridCrossedZ = (int)m_playerGridPos.y - (int)prevPlayerGridPos.y;
		m_matrixTopRight.Set((float)matrixPos((int)m_matrixTopRight.x, gridCrossedX), (float)matrixPos((int)m_matrixTopRight.y, gridCrossedZ));

		if (gridCrossedX != 0)
			updateXTiles(gridCrossedX);

		if (gridCrossedZ != 0)
			updateZTiles(gridCrossedZ);
	}

	void init()
	{
		m_tileCountHalf = tileCount / 2;
		m_gridCenterOffset = Vector3.zero;// new Vector3(tileSize / 2, 0, tileSize / 2);
		m_tileMoveDesc = new TileDescription[tileCount];
		for (int i = 0; i < tileCount; ++i)
			m_tileMoveDesc[i] = new TileDescription();

		m_matrixTopRight.Set(tileCount - 1, tileCount - 1);
		setGridPosFromWorldPos(player.transform.position + m_gridCenterOffset, ref m_playerGridPos);

		m_tileLayerArray = GetComponentsInChildren<ITileLayer>();
		foreach (ITileLayer tileLayer in m_tileLayerArray)
			tileLayer.initTileLayer(this);
	}

	public void removeAllTiles()
	{
		ITileLayer[] tileLayers = GetComponentsInChildren<ITileLayer>();
		foreach (ITileLayer tileLayer in tileLayers)
			tileLayer.removeAllTiles();
	}

	void setWorldPosFromGridPos(Vector2 gridCoord, ref Vector3 worldPos)
	{
		float x = gridCoord.x * tileSize;
		float z = gridCoord.y * tileSize;
		worldPos.Set(x, 0, z);
	}

	void setGridPosFromWorldPos(Vector3 worldPos, ref Vector2 gridCoord)
	{
		int x = Mathf.FloorToInt(worldPos.x / tileSize);
		int z = Mathf.FloorToInt(worldPos.z / tileSize);
		gridCoord.Set(x, z);
	}

	int matrixPos(int top, int rows)
	{
		return (tileCount + top + (rows % tileCount)) % tileCount;
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

	public void updateAllTiles()
	{
		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				m_tileMoveDesc[x].matrixCoord.Set(x, z);
				m_tileMoveDesc[x].gridCoord.Set(
					x + (int)m_playerGridPos.x - m_tileCountHalf,
					z + (int)m_playerGridPos.y - m_tileCountHalf);

				setWorldPosFromGridPos(m_tileMoveDesc[x].gridCoord, ref m_tileMoveDesc[x].worldPos);
				setNeighbours(m_tileMoveDesc[x].matrixCoord, ref m_tileMoveDesc[x].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerArray) {
				tileLayer.moveTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

	private void updateXTiles(int tilesCrossedX)
	{
		int moveDirection = tilesCrossedX > 0 ? 1 : -1;
		int nuberOfColsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedX), tileCount);

		for (int i = 0; i <= nuberOfColsToUpdate; ++i) {
			int matrixFrontX = matrixPos((int)m_matrixTopRight.x, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontX = matrixPos(matrixFrontX, 1);

			int gridCoordX = moveDirection > 0 ?
				(int)m_playerGridPos.x + m_tileCountHalf - i - 1 :
				(int)m_playerGridPos.x - m_tileCountHalf + i;

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontZ = matrixPos((int)m_matrixTopRight.y, -j);
				int gridCoordZ = (int)m_playerGridPos.y + m_tileCountHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(gridCoordX, gridCoordZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontZ);

				setWorldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerArray) {
				if (i < nuberOfColsToUpdate)
					tileLayer.moveTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

	private void updateZTiles(int tilesCrossedZ)
	{
		int moveDirection = tilesCrossedZ > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedZ), tileCount);

		for (int i = 0; i <= nuberOfRowsToUpdate; ++i) {
			int matrixFrontZ = matrixPos((int)m_matrixTopRight.y, i * -moveDirection);
			if (moveDirection < 0)
				matrixFrontZ = matrixPos(matrixFrontZ, 1);

			int gridCoordZ = moveDirection > 0 ?
				(int)m_playerGridPos.y + m_tileCountHalf - i - 1 :
				(int)m_playerGridPos.y - m_tileCountHalf + i;

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontX = matrixPos((int)m_matrixTopRight.x, -j);
				int gridCoordX = (int)m_playerGridPos.x + m_tileCountHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(gridCoordX, gridCoordZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontZ);

				setWorldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
				setNeighbours(m_tileMoveDesc[j].matrixCoord, ref m_tileMoveDesc[j].neighbours);
			}

			foreach (ITileLayer tileLayer in m_tileLayerArray) {
				if (i < nuberOfRowsToUpdate)
					tileLayer.moveTiles(m_tileMoveDesc);
				if (tileLayer is ITileTerrainLayer)
					((ITileTerrainLayer)tileLayer).updateTileNeighbours(m_tileMoveDesc);
			}
		}
	}

	public TileDescription getTileDescription(Vector3 worldPos)
	{
		TileDescription desc = m_tileMoveDesc[0];

		setGridPosFromWorldPos(worldPos + m_gridCenterOffset, ref desc.gridCoord);
		Vector2 gridOffset = desc.gridCoord - m_playerGridPos;

		if (Mathf.Abs(gridOffset.x) > m_tileCountHalf || Mathf.Abs(gridOffset.y) > m_tileCountHalf) {
			desc.matrixCoord.Set(-1, -1);
			return desc;
		}

		int matrixX = matrixPos((int)m_matrixTopRight.x, (int)gridOffset.x - m_tileCountHalf + 1);
		int matrixY = matrixPos((int)m_matrixTopRight.y, (int)gridOffset.y - m_tileCountHalf + 1);
		desc.matrixCoord.Set(matrixX, matrixY);

//		setWorldPosFromGridPos(desc.gridCoord, ref desc.worldPos);
//		setNeighbours(desc.matrixCoord, ref desc.neighbours);

		return desc;
	}

	public ITileLayer getTileLayer(int index)
	{
		return m_tileLayerArray[index];
	}
}
