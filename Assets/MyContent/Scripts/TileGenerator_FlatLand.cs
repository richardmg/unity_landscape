using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileGenerator : MonoBehaviour {

	public GameObject player;
	public GameObject tile;

	const int m_matrixColumnCount = 10;
	const int m_matrixColumnCountHalf = m_matrixColumnCount / 2;
	const int m_tileWidth = 10;

	int m_currentTileX;
	int m_currentTileZ;

	GameObject[,] m_tileMatrix = new GameObject[m_matrixColumnCount, m_matrixColumnCount];
	int m_matrixTopIndex = m_matrixColumnCount - 1;
	int m_matrixRightIndex = m_matrixColumnCount - 1;

	// Use this for initialization
	void Start () {
		Vector3 playerPos = player.transform.position;
		m_currentTileX = Mathf.FloorToInt(playerPos.x / m_tileWidth);
		m_currentTileZ = Mathf.FloorToInt(playerPos.z / m_tileWidth);

		for (int z = 0; z < m_matrixColumnCount; ++z) {
			for (int x = 0; x < m_matrixColumnCount; ++x) {
				GameObject tileObject = (GameObject)Instantiate(tile, Vector3.zero, Quaternion.identity);
				m_tileMatrix[x, z] = tileObject;
				float tilePosX = Mathf.FloorToInt(playerPos.x) + tilePosToWorldPos(x - m_matrixColumnCountHalf);
				float tilePosZ = Mathf.FloorToInt(playerPos.z) + tilePosToWorldPos(z - m_matrixColumnCountHalf);
				tileObject.transform.position = new Vector3(tilePosX, 0, tilePosZ);
			}
		}
				
		float w = m_tileMatrix[0, 0].GetComponent<Renderer>().bounds.size.x;
		Debug.AssertFormat(m_tileWidth == w, "Game object tile width does not match const m_tileWidth");
	}

	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown("q"))
			player.transform.position += new Vector3 (0, 0, 45);
		if (Input.GetKeyDown ("e"))
			player.transform.position += new Vector3 (0, 0, -42);

		Vector3 playerPos = player.transform.position;
		int playerTileX = Mathf.FloorToInt((playerPos.x + (m_tileWidth / 2)) / m_tileWidth);
		int playerTileZ = Mathf.FloorToInt((playerPos.z + (m_tileWidth / 2)) / m_tileWidth);

		updateTiles(ref m_currentTileZ, playerTileZ, ref m_matrixTopIndex, true);
		updateTiles(ref m_currentTileX, playerTileX, ref m_matrixRightIndex, false);
	}

	private void updateTiles(ref int oldTileCoord, int newTileCoord, ref int matrixFrontIndex, bool updateZAxis)
	{
		if (newTileCoord == oldTileCoord)
			return;
		
		int tilesCrossed = newTileCoord - oldTileCoord;
		int moveDirection = tilesCrossed > 0 ? 1 : -1;
		int nuberOfRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossed), m_matrixColumnCount);
		int newMatrixFrontIndex = (m_matrixColumnCount + matrixFrontIndex + (tilesCrossed % m_matrixColumnCount)) % m_matrixColumnCount;

		for (int i = 0; i < nuberOfRowsToUpdate; ++i) {
			// Get the matrix row that contains tiles that are out of sight, and move it in front of the player
			int matrixIndexToReuse = (m_matrixColumnCount + newMatrixFrontIndex + (i * -moveDirection)) % m_matrixColumnCount;
			if (moveDirection < 0) {
				// When moving "backwards", reuse the new bottom index instead
				matrixIndexToReuse = (matrixIndexToReuse + 1) % m_matrixColumnCount;
			}

			int tileCoord = moveDirection > 0 ?
				newTileCoord + m_matrixColumnCountHalf - i - 1 :
				newTileCoord - m_matrixColumnCountHalf + i;

			if (updateZAxis) {
				for (int col = 0; col < m_matrixColumnCount; ++col) {
					GameObject tileObject = m_tileMatrix[col, matrixIndexToReuse];
					tileObject.transform.position = new Vector3(tileObject.transform.position.x, 0, tilePosToWorldPos(tileCoord));
				}
			} else {
				for (int row = 0; row < m_matrixColumnCount; ++row) {
					// Get the game object representing the tile, and move it to it's new position
					GameObject tileObject = m_tileMatrix[matrixIndexToReuse, row];
					tileObject.transform.position = new Vector3(tilePosToWorldPos(tileCoord), 0, tileObject.transform.position.z);
				}
			}
		}

		matrixFrontIndex = newMatrixFrontIndex;
		oldTileCoord = newTileCoord;
	}

	private float tilePosToWorldPos(int pos)
	{
		// Return the 'bottom left' corner of the tile 
		return (pos * m_tileWidth) + (m_tileWidth / 2);
	}

	public abstract void updateTile(GameObject tile);
}

public class TileGenerator_FlatLand : TileGenerator {

	public override void updateTile(GameObject tile)
	{
	}
}
