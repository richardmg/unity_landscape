using UnityEngine;
using System.Collections;

public class LandscapeConstructor : MonoBehaviour {

	public GameObject tile;
	public GameObject tile2;
	public GameObject player;

	TileEngine m_tileEngine;

	// Use this for initialization
	void Start()
	{
		m_tileEngine = new TileEngine(4, 10);
		m_tileEngine.addTileLayer(new TileGroundLayer(tile, tile2, m_tileEngine));
		m_tileEngine.startx(player.transform.position);
	}

	// Update is called once per frame
	void Update()
	{
		m_tileEngine.update(player.transform.position);
	}
}

public class TileGroundLayer : TileLayer
{
	GameObject[,] m_tileMatrix;

	public TileGroundLayer(GameObject tilePrefab, GameObject tilePrefab2, TileEngine tileEngine)
	{
		int count = tileEngine.columnCount();
		m_tileMatrix = new GameObject[count, count];
		for (int z = 0; z < count; ++z) {
			for (int x = 0; x < count; ++x)
				m_tileMatrix[x, z] = (GameObject)GameObject.Instantiate(((x + z) % 2) == 0 ? tilePrefab : tilePrefab2, Vector3.zero, Quaternion.identity);;
		}

		float w = m_tileMatrix[0, 0].GetComponent<Renderer>().bounds.size.x;
		Debug.AssertFormat(w == tileEngine.tileWidth(), "TileGroundLayer: tilePrefab needs to have the same size as tileEngine.tileWidth()");
	}

	public override void moveTile(Vector2 tileMatrixCoord, Vector2 tileGridCoord, Vector3 tileWorldPos)
	{
		GameObject tile = m_tileMatrix[(int)tileMatrixCoord.x, (int)tileMatrixCoord.y];
		tile.transform.position = tileWorldPos;
//		tile.GetComponent<TileGround>().onTileMoved();
	}
}