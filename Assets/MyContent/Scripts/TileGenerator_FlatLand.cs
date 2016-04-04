using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class TileGenerator : MonoBehaviour {

	public GameObject player;
	public GameObject tile;

	const int m_tileRadius = 2;
	int m_tileWidth;
	Vector3 m_playerStartPos;

	List<GameObject> m_tilePool = new List<GameObject>();

	// Use this for initialization
	void Start () {
		for (int t = 0; t < m_tileRadius * m_tileRadius * 4; ++t)
			m_tilePool.Add((GameObject)Instantiate(tile, Vector3.zero, Quaternion.identity));
				
		float w = m_tilePool[0].GetComponent<Renderer>().bounds.size.x;
		m_tileWidth = (int)w;
		Debug.AssertFormat(m_tileWidth == w, "Tile width needs to be integer");
		m_playerStartPos = player.transform.position;

		int tileIndex = 0;
		for (int xTile = -m_tileRadius; xTile < m_tileRadius; ++xTile) {
			for (int zTile = -m_tileRadius; zTile < m_tileRadius; ++zTile) {
				float tilePosX = (int)m_playerStartPos.x + (xTile * m_tileWidth) + (m_tileWidth / 2);
				float tilePosZ = (int)m_playerStartPos.z + (zTile * m_tileWidth) + (m_tileWidth / 2);
				GameObject tileObject = m_tilePool[tileIndex];
				tileObject.transform.position = new Vector3(tilePosX, 0, tilePosZ);
				updateTile(tileObject);
				tileIndex++;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		Vector3 currentPlayerPos = player.transform.position;
		int xMove = (int)(currentPlayerPos.x - m_playerStartPos.x);
		int zMove = (int)(currentPlayerPos.z - m_playerStartPos.z);
		if (Mathf.Abs(xMove) < m_tileWidth && Mathf.Abs(zMove) < m_tileWidth)
			return;


		/*
		Finn nytt center (player player pos % m_tileRadius)
			center + m_tileWidth => top

			rad som kan frigjøres: (center - gammelt center) > 0 ? center - m_tileRadius : center + m_tileRadius;
*/

		print("update needed");
		m_playerStartPos = currentPlayerPos;

		int tileIndex = 0;
		for (int xTile = -m_tileRadius; xTile < m_tileRadius; ++xTile) {
			for (int zTile = -m_tileRadius; zTile < m_tileRadius; ++zTile) {
				int tilePosX = (int)m_playerStartPos.x + (xTile * m_tileWidth) + (m_tileWidth / 2);
				int tilePosZ = (int)m_playerStartPos.z + (zTile * m_tileWidth) + (m_tileWidth / 2);
				GameObject tileObject = m_tilePool[tileIndex];
				tileObject.transform.position = new Vector3(tilePosX, 0, tilePosZ);
				updateTile(tileObject);
				tileIndex++;
			}
		}
	}

	public abstract void updateTile(GameObject tile);
}

public class TileGenerator_FlatLand : TileGenerator {

	public override void updateTile(GameObject tile)
	{
		print("update tile" + tile.transform.position.x);
	}
}
