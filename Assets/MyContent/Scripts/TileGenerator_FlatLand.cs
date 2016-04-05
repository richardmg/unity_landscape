using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileGenerator : MonoBehaviour {

	public GameObject player;
	public GameObject tile;

	const int m_matrixColumnCount = 4;
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
			player.transform.position += new Vector3 (0, 0, 25);
		if (Input.GetKeyDown ("e"))
			player.transform.position += new Vector3 (0, 0, -22);

		Vector3 playerPos = player.transform.position;
		int playerTileX = Mathf.FloorToInt(playerPos.x / m_tileWidth);
		int playerTileZ = Mathf.FloorToInt(playerPos.z / m_tileWidth);

		if (playerTileZ != m_currentTileZ) {
			int tilesCrossedZ = playerTileZ - m_currentTileZ;
			int moveDirectionZ = tilesCrossedZ > 0 ? 1 : -1;
			int nuberOfTileRowsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedZ), m_matrixColumnCount);
			m_matrixTopIndex = (m_matrixTopIndex + tilesCrossedZ) % m_matrixColumnCount;
			m_matrixTopIndex += (m_matrixTopIndex < 0) ? m_matrixColumnCount : 0;

			for (int row = 0; row < nuberOfTileRowsToUpdate; ++row) {
				// Get the matrix row that contains the tiles that are now
				// out of sight, and should be moved in front of the player
				int indexOfRowToReuse = (m_matrixTopIndex + (row * -moveDirectionZ)) % m_matrixColumnCount;
				indexOfRowToReuse = moveDirectionZ > 0 ? indexOfRowToReuse : (indexOfRowToReuse + 1) % m_matrixColumnCount;
				// For each tile in the row of tiles we're going to reuse, calculate the new tile z coordinate
				int tileZ = moveDirectionZ > 0 ? playerTileZ + m_matrixColumnCountHalf - row - 1 : playerTileZ - m_matrixColumnCountHalf + row;

				for (int col = 0; col < m_matrixColumnCount; ++col) {
					// Get the game object representing the tile, and move it to it's new position
					GameObject tileObject = m_tileMatrix[col, indexOfRowToReuse];
					tileObject.transform.position = new Vector3(tileObject.transform.position.x, 0, tilePosToWorldPos(tileZ));
				}
			}
			m_currentTileZ = playerTileZ;
		}

		if (playerTileX != m_currentTileX) {
			int tilesCrossedX = playerTileX - m_currentTileX;
			int moveDirectionX = tilesCrossedX > 0 ? 1 : -1;
			int nuberOfTileColsToUpdate = Mathf.Min(Mathf.Abs(tilesCrossedX), m_matrixColumnCount);
			m_matrixRightIndex = (m_matrixRightIndex + tilesCrossedX) % m_matrixColumnCount;
			m_matrixRightIndex += (m_matrixRightIndex < 0) ? m_matrixColumnCount : 0;

			for (int col = 0; col < nuberOfTileColsToUpdate; ++col) {
				// Get the matrix col that contains the tiles that are now
				// out of sight, and should be moved in front of the player
				int indexOfColToReuse = (m_matrixRightIndex + (col * -moveDirectionX)) % m_matrixColumnCount;
				indexOfColToReuse = moveDirectionX > 0 ? indexOfColToReuse : (indexOfColToReuse + 1) % m_matrixColumnCount;
				// For each tile in the row of tiles we're going to reuse, calculate the new tile z coordinate
				int tileX = moveDirectionX > 0 ? playerTileX + m_matrixColumnCountHalf - col - 1 : playerTileX - m_matrixColumnCountHalf + col;

				print(indexOfColToReuse + ", " + moveDirectionX);
				for (int row = 0; row < m_matrixColumnCount; ++row) {
					// Get the game object representing the tile, and move it to it's new position
					GameObject tileObject = m_tileMatrix[indexOfColToReuse, row];
					tileObject.transform.position = new Vector3(tilePosToWorldPos(tileX), 0, tileObject.transform.position.z);
				}
			}
			m_currentTileX = playerTileX;
		}
	}

	public float tilePosToWorldPos(int pos)
	{
		// Return the 'bottom left' corner of the tile 
		return (pos * m_tileWidth) + (m_tileWidth / 2);
	}

//	public float listIndex(int x, int y)
//	{
//		int tileRowIndex = (m_matrixLeftIndex + col) % m_matrixColumnCount;
//		int listIndex = (indexOfRowToReuse * m_matrixColumnCount) + tileRowIndex;
//	}

	public abstract void updateTile(GameObject tile);
}

public class TileGenerator_FlatLand : TileGenerator {

	public override void updateTile(GameObject tile)
	{
	}
}
