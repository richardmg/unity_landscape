using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileGenerator : MonoBehaviour {

	public GameObject player;
	public GameObject tile;

	const int m_tileRadius = 2;
	const int m_tileDiameter = m_tileRadius * 2;
	const int m_tileCount = m_tileDiameter * m_tileDiameter;
	const int m_tileWidth = 10;

	int m_currentTileX;
	int m_currentTileZ;

	List<GameObject> m_tileList = new List<GameObject>();
	int m_topIndex = m_tileDiameter - 1;
	int m_leftIndex = 0;

	// Use this for initialization
	void Start () {
		Vector3 playerPos = player.transform.position;
		m_currentTileX = Mathf.FloorToInt(playerPos.x / m_tileWidth);
		m_currentTileZ = Mathf.FloorToInt(playerPos.z / m_tileWidth);

		for (int t = 0; t < m_tileCount; ++t) {
			GameObject tileObject = (GameObject)Instantiate(tile, Vector3.zero, Quaternion.identity);
			m_tileList.Add(tileObject);
			int x = (t % m_tileDiameter) - m_tileRadius;
			int z = (int)(t / m_tileDiameter) - m_tileRadius;
			float tilePosX = Mathf.FloorToInt(playerPos.x) + (x * m_tileWidth) + m_tileWidth;
			float tilePosZ = Mathf.FloorToInt(playerPos.z) + (z * m_tileWidth) + m_tileWidth;
			tileObject.transform.position = new Vector3(tilePosX, 0, tilePosZ);
		}
				
		float w = m_tileList[0].GetComponent<Renderer>().bounds.size.x;
		Debug.AssertFormat(m_tileWidth == w, "Game object tile width does not match const m_tileWidth");
	}

	// Update is called once per frame
	void Update () {
		Vector3 playerPos = player.transform.position;
		int tileX = Mathf.FloorToInt(playerPos.x / m_tileWidth);
		int tileZ = Mathf.FloorToInt(playerPos.z / m_tileWidth);
		if (tileX == m_currentTileX && tileZ == m_currentTileZ)
			return;

		int tileMoveX = tileX - m_currentTileX;
		int tileMoveZ = tileZ - m_currentTileZ;

		m_topIndex = (m_topIndex + tileMoveZ) % m_tileDiameter;
		m_topIndex += m_topIndex < 0 ? m_tileDiameter : 0;
		m_leftIndex = 0;//(m_leftIndex + tileMoveX) % m_tileDiameter;

		print(m_currentTileZ + ", " + tileZ + ", " + tileMoveZ + ", " + playerPos.z);

		// todo: handle if tileMoveZ > 1
		int bottomIndex = (m_topIndex + 1) % m_tileDiameter;
		int verticalIndex = tileMoveZ > 0 ? m_topIndex : bottomIndex;

		for (int x = 0; x < m_tileDiameter; ++x) {
			// Figure out which tile should be reused
			int tileLeftIndex = (m_leftIndex + x) % m_tileDiameter;
			int tileListIndex = (verticalIndex * m_tileDiameter) + tileLeftIndex;
			GameObject tileObject = m_tileList[tileListIndex];

			// Calculate where the tile should be moved 
			int zOffset = tileMoveZ > 0 ? m_tileRadius - 1 : -m_tileRadius;
			float tilePosZ = ((tileZ + zOffset) * m_tileWidth) + m_tileWidth;
			tileObject.transform.position = new Vector3(tileObject.transform.position.x, 0, tilePosZ);
		}

		m_currentTileX = tileX;
		m_currentTileZ = tileZ;
	}

	public abstract void updateTile(GameObject tile);
}

public class TileGenerator_FlatLand : TileGenerator {

	public override void updateTile(GameObject tile)
	{
	}
}
