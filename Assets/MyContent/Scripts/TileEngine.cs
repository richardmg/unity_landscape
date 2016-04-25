using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileMoveDescription
{
	public Vector2 tileGridCoord = new Vector2();
	public Vector3 tileWorldPos = new Vector3();

	public Vector2 tileMatrixCoord = new Vector2();
	public Vector2 tileMatrixTopCoord = new Vector2();
	public Vector2 tileMatrixBottomCoord = new Vector2();
	public Vector2 tileMatrixLeftCoord = new Vector2();
	public Vector2 tileMatrixRightCoord = new Vector2();
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

	void gridCoordToWorldPos(ref Vector3 worldPos, Vector2 gridCoord)
	{
		float x = gridCoord.x * m_tileWorldWidth;
		float z = gridCoord.y * m_tileWorldWidth;
		worldPos.Set(x, 0, z);
	}

	void worldPosToGridCoord(ref Vector2 gridCoord, Vector3 worldPos)
	{
		int x = Mathf.FloorToInt(worldPos.x / m_tileWorldWidth);
		int z = Mathf.FloorToInt(worldPos.z / m_tileWorldWidth);
		gridCoord.Set(x, z);
	}

	int matrixPos(int top, int rows)
	{
		return (m_matrixRowCount + top + (rows % m_matrixRowCount)) % m_matrixRowCount;
	}

	public void start(Vector3 playerPos)
	{
		worldPosToGridCoord(ref m_centerTileCoord, playerPos + m_changeTileOffset);

		for (int z = 0; z < m_matrixRowCount; ++z) {
			for (int x = 0; x < m_matrixRowCount; ++x) {
				m_tileMoveDesc[x].tileMatrixCoord.Set(x, z);
				m_tileMoveDesc[x].tileGridCoord.Set(
					x + (int)m_centerTileCoord.x - m_matrixRowCountHalf,
					z + (int)m_centerTileCoord.y - m_matrixRowCountHalf);
				gridCoordToWorldPos(ref m_tileMoveDesc[x].tileWorldPos, m_tileMoveDesc[x].tileGridCoord);

				foreach (ITileLayer tileLayer in m_tileLayerList)
					tileLayer.moveTiles(m_tileMoveDesc, false);
			}
		}
	}

	public void update(Vector3 playerPos)
	{
		Vector2 prevCenterTileCoord = m_centerTileCoord;
		worldPosToGridCoord(ref m_centerTileCoord, playerPos + m_changeTileOffset);

		if (m_centerTileCoord.x != prevCenterTileCoord.x)
			updateTiles((int)m_centerTileCoord.x, (int)prevCenterTileCoord.x, ref m_matrixRightIndex, false);

		if (m_centerTileCoord.y != prevCenterTileCoord.y)
			updateTiles((int)m_centerTileCoord.y, (int)prevCenterTileCoord.y, ref m_matrixTopIndex, true);
	}

	private void updateTiles(int currentTileCoord, int prevTileCoord, ref int matrixFrontZ, bool updateZAxis)
	{
		int tilesCrossed = currentTileCoord - prevTileCoord;
		int moveDirection = tilesCrossed > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossed), m_matrixRowCount);

		// Update matrix pointer, which is passed as ref

		// todo: factor this out to update!!!!

		matrixFrontZ = matrixPos(matrixFrontZ, tilesCrossed);

		for (int i = 0; i < nuberOfRowsToUpdate; ++i) {
			int matrixLoopFrontZ = matrixPos(matrixFrontZ, i * -moveDirection);
			int matrixLoopFrontNeighbourNorthZ = -1;
			int matrixLoopFrontNeighbourSouthZ = -1;

			if (moveDirection > 0) {
				matrixLoopFrontNeighbourSouthZ = matrixPos(matrixLoopFrontZ, -1);
			} else {
				// When moving "backwards", reuse the new bottom index instead
				matrixLoopFrontZ = matrixPos(matrixLoopFrontZ, 1);
				matrixLoopFrontNeighbourNorthZ = matrixPos(matrixLoopFrontZ, matrixLoopFrontZ + 1);
			}

			int coordCenterZ = moveDirection > 0 ?
				currentTileCoord + m_matrixRowCountHalf - i - 1 :
				currentTileCoord - m_matrixRowCountHalf + i;

			if (updateZAxis) {
				int matrixFrontX = (int)m_centerTileCoord.x;
				int matrixBackX = (m_matrixRowCount + matrixFrontX - 1) % m_matrixRowCount;

				for (int j = 0; j < m_matrixRowCount; ++j) {
					int tileMatrixX = (m_matrixRowCount + m_matrixRightIndex - j) % m_matrixRowCount;

					bool onFrontEdge = (tileMatrixX == matrixFrontX);
					bool onBackEdge = (tileMatrixX == matrixBackX);

					int tileMatrixLeftX = onBackEdge ? -1 : (m_matrixRowCount + tileMatrixX - 1) % m_matrixRowCount;
					int tileMatrixRightX = onFrontEdge ? -1 :(tileMatrixX + 1) % m_matrixRowCount;
					int tileGridCoordX = (int)m_centerTileCoord.x + m_matrixRowCountHalf - j - 1;

					// jeg er her, test ut att ileMatrixLeftX og tileMatrixRightX er riktig

					m_tileMoveDesc[j].tileMatrixCoord.Set(tileMatrixX, matrixLoopFrontZ);
					m_tileMoveDesc[j].tileMatrixTopCoord.Set(matrixLoopFrontNeighbourNorthZ == -1 ? -1 : tileMatrixX, matrixLoopFrontNeighbourNorthZ);
					m_tileMoveDesc[j].tileMatrixBottomCoord.Set(matrixLoopFrontNeighbourSouthZ == -1 ? -1 : tileMatrixX, matrixLoopFrontNeighbourSouthZ);
					m_tileMoveDesc[j].tileMatrixLeftCoord.Set(tileMatrixLeftX, matrixLoopFrontZ);
					m_tileMoveDesc[j].tileMatrixRightCoord.Set(tileMatrixRightX, matrixLoopFrontZ);
					m_tileMoveDesc[j].tileGridCoord.Set(tileGridCoordX, coordCenterZ);
					gridCoordToWorldPos(ref m_tileMoveDesc[j].tileWorldPos, m_tileMoveDesc[j].tileGridCoord);
				}
			} else {
				for (int j = 0; j < m_matrixRowCount; ++j) {
					int tileX = (m_matrixRowCount + m_matrixTopIndex - j) % m_matrixRowCount;

					m_tileMoveDesc[j].tileMatrixCoord.Set(matrixLoopFrontZ, tileX);
					m_tileMoveDesc[j].tileMatrixLeftCoord.Set(matrixLoopFrontNeighbourSouthZ, matrixLoopFrontNeighbourSouthZ == -1 ? -1 : tileX);
					m_tileMoveDesc[j].tileMatrixRightCoord.Set(matrixLoopFrontNeighbourNorthZ, matrixLoopFrontNeighbourNorthZ == -1 ? -1 : tileX);
					m_tileMoveDesc[j].tileMatrixTopCoord.Set(-1, -1);
					m_tileMoveDesc[j].tileMatrixBottomCoord.Set(-1, -1);

					m_tileMoveDesc[j].tileGridCoord.Set(coordCenterZ, (int)m_centerTileCoord.y + m_matrixRowCountHalf - j - 1);
					gridCoordToWorldPos(ref m_tileMoveDesc[j].tileWorldPos, m_tileMoveDesc[j].tileGridCoord);
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

//		if (alongZ) {
//			MonoBehaviour.print("center: " + desc[0].tileMatrixCoord);
//			MonoBehaviour.print("top: " + desc[0].tileMatrixTopCoord);
//			MonoBehaviour.print("bottom: " + desc[0].tileMatrixBottomCoord);
//			MonoBehaviour.print("left: " + desc[0].tileMatrixLeftCoord);
//			MonoBehaviour.print("right " + desc[0].tileMatrixRightCoord);
//			MonoBehaviour.print("--------------------");
//		}

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
