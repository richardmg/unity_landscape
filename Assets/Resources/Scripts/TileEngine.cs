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
	Terrain getTerrainTile(Vector2 matrixCoord);
}

public class TileEngine : MonoBehaviour {
	[Range (2, 10)]
	public int tileCount = 4;
	[Range (1, 5000)]
	public float tileSize = 100;
	public GameObject player;
	public bool showInEditor = false;

	float m_tileCountHalf;
	Vector3 m_worldToGridOffset;
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
		Vector3 playerWorldPos = player.transform.position;
		Vector2 prevPlayerGridPos = m_playerGridPos;
		gridPosFromWorldPosAsInt(playerWorldPos, ref m_playerGridPos);
		int gridCrossedX = (int)(m_playerGridPos.x - prevPlayerGridPos.x);
		int gridCrossedZ = (int)(m_playerGridPos.y - prevPlayerGridPos.y);

		if (gridCrossedX == 0 && gridCrossedZ == 0)
			return;

		m_matrixTopRight.Set((float)matrixPos((int)m_matrixTopRight.x, gridCrossedX), (float)matrixPos((int)m_matrixTopRight.y, gridCrossedZ));

		if (gridCrossedX != 0)
			updateXTiles(gridCrossedX);

		if (gridCrossedZ != 0)
			updateZTiles(gridCrossedZ);
	}

	void init()
	{
		m_tileCountHalf = tileCount / 2f;
		m_worldToGridOffset = new Vector3(tileSize / 2f, 0, tileSize / 2f);
		m_tileMoveDesc = new TileDescription[tileCount];
		m_tileLayerArray = GetComponentsInChildren<ITileLayer>();

		m_matrixTopRight.Set(tileCount - 1, tileCount - 1);
		gridPosFromWorldPosAsInt(player.transform.position, ref m_playerGridPos);

		for (int i = 0; i < tileCount; ++i)
			m_tileMoveDesc[i] = new TileDescription();

		foreach (ITileLayer tileLayer in m_tileLayerArray)
			tileLayer.initTileLayer(this);
	}

	public void removeAllTiles()
	{
		ITileLayer[] tileLayers = GetComponentsInChildren<ITileLayer>();
		foreach (ITileLayer tileLayer in tileLayers)
			tileLayer.removeAllTiles();
	}

	void worldPosFromGridPos(Vector2 gridCoord, ref Vector3 worldPos)
	{
		worldPos.Set(gridCoord.x * tileSize, 0, gridCoord.y * tileSize);
	}

	void gridPosFromWorldPosAsInt(Vector3 worldPos, ref Vector2 gridCoord)
	{
		// Center align the grid onto the world
		gridCoord.Set(
			Mathf.FloorToInt((worldPos.x + m_worldToGridOffset.x) / tileSize),
			Mathf.FloorToInt((worldPos.z + m_worldToGridOffset.z) / tileSize));
	}

	public void matrixCoordFromWorldPos(Vector3 worldPos, ref Vector2 matrixCoord)
	{
		// Note that there will be four tiles underneath each grid
		// unit, since the grid is center aligned onto the world.
		int gridX = Mathf.FloorToInt(worldPos.x / tileSize);
		int gridY = Mathf.FloorToInt(worldPos.z / tileSize);
		int gridOffsetX = gridX - (int)m_playerGridPos.x;
		int gridOffsetY = gridY - (int)m_playerGridPos.y;

		Debug.Assert(Mathf.Abs(gridOffsetX) <= m_tileCountHalf && Mathf.Abs(gridOffsetY) <= m_tileCountHalf, "Worldpos outside current tiles");

		int matrixX = matrixPos((int)m_matrixTopRight.x, (int)(gridOffsetX - m_tileCountHalf + 1));
		int matrixY = matrixPos((int)m_matrixTopRight.y, (int)(gridOffsetY - m_tileCountHalf + 1));
		matrixCoord.Set(matrixX, matrixY);
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
				m_tileMoveDesc[x].gridCoord.Set(x + m_playerGridPos.x - m_tileCountHalf, z + m_playerGridPos.y - m_tileCountHalf);
				worldPosFromGridPos(m_tileMoveDesc[x].gridCoord, ref m_tileMoveDesc[x].worldPos);
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

			float gridCoordX = moveDirection > 0 ?
				m_playerGridPos.x + m_tileCountHalf - i - 1 :
				m_playerGridPos.x - m_tileCountHalf + i;

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontZ = matrixPos((int)m_matrixTopRight.y, -j);
				float gridCoordZ = m_playerGridPos.y + m_tileCountHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(gridCoordX, gridCoordZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontZ);

				worldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
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

			float gridCoordZ = moveDirection > 0 ?
				m_playerGridPos.y + m_tileCountHalf - i - 1 :
				m_playerGridPos.y - m_tileCountHalf + i;

			for (int j = 0; j < tileCount; ++j) {
				int matrixFrontX = matrixPos((int)m_matrixTopRight.x, -j);
				float gridCoordX = m_playerGridPos.x + m_tileCountHalf - j - 1;

				m_tileMoveDesc[j].gridCoord.Set(gridCoordX, gridCoordZ);
				m_tileMoveDesc[j].matrixCoord.Set(matrixFrontX, matrixFrontZ);

				worldPosFromGridPos(m_tileMoveDesc[j].gridCoord, ref m_tileMoveDesc[j].worldPos);
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

	public ITileLayer getTileLayer(int index)
	{
		return m_tileLayerArray[index];
	}
}
