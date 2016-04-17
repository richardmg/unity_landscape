using UnityEngine;
using System.Collections;

public class LandscapeConstructor : MonoBehaviour {

	public int rows = 4;
	public int tileWidth = 10;
	public float perlinScale = 0.02f;
	public float tileHeightScale = 20f;
	public GameObject tile;
	public GameObject player;

	TileEngine m_tileEngine;

	static public LandscapeConstructor instance;

	public static float getGroundHeight(float x, float z)
	{
		return Mathf.PerlinNoise(x * instance.perlinScale, z * instance.perlinScale) * instance.tileHeightScale;
	}

	// Use this for initialization
	void Start()
	{
		Debug.AssertFormat(!instance, "LandscapeConstructor needs to be singleton");
		instance = this;

		// Move player on top of landscape
		Vector3 playerPos = player.transform.position;
		playerPos.y = getGroundHeight(playerPos.x, playerPos.z) + 1;
		player.transform.position = playerPos;

		m_tileEngine = new TileEngine(rows, tileWidth);
		m_tileEngine.addTileLayer(new TileGroundLayer(tile));
		m_tileEngine.start(player.transform.position);
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

	public TileGroundLayer(GameObject tilePrefab)
	{
		int count = LandscapeConstructor.instance.rows;
		m_tileMatrix = new GameObject[count, count];
		for (int z = 0; z < count; ++z) {
			for (int x = 0; x < count; ++x)
				m_tileMatrix[x, z] = (GameObject)GameObject.Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
		}

//		float w = m_tileMatrix[0, 0].GetComponent<Renderer>().bounds.size.x;
//		Debug.AssertFormat(w == tileEngine.tileWidth(), "TileGroundLayer: tilePrefab needs to have the same size as tileEngine.tileWidth()");
	}

	public override void moveTile(Vector2 tileMatrixCoord, Vector2 tileGridCoord, Vector3 tileWorldPos)
	{
		MonoBehaviour.print(tileGridCoord);
		GameObject tile = m_tileMatrix[(int)tileMatrixCoord.x, (int)tileMatrixCoord.y];
		tile.GetComponent<TileGround>().moveTile(tileGridCoord, tileWorldPos);
	}
}