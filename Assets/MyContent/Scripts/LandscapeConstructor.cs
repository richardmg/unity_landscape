using UnityEngine;
using System.Collections;

public class LandscapeConstructor : MonoBehaviour {

	public int rows = 4;
	public int tileWidth = 1000;

	public float perlinLargeScale = 0.003f;
	public float perlinMediumScale = 0.02f;
	public float perlinSmallScale = 0.1f;
	public float landscapeHeightLargeScale = 40f;
	public float landscapeHeightMediumScale = 4f;
	public float landscapeSmallScale = 0.3f;

	public GameObject terrainTile;
	public GameObject player;

	TileEngine m_tileEngine;

	static public LandscapeConstructor instance;

	public static float getGroundHeight(float x, float z)
	{
		float firstOctave = Mathf.PerlinNoise(x * instance.perlinLargeScale, z * instance.perlinLargeScale) * instance.landscapeHeightLargeScale;
		float secondOctave = Mathf.PerlinNoise(x * instance.perlinMediumScale, z * instance.perlinMediumScale) * instance.landscapeHeightMediumScale;
		float thirdOctave = Mathf.PerlinNoise(x * instance.perlinSmallScale, z * instance.perlinSmallScale) * instance.landscapeSmallScale;
		return firstOctave + secondOctave + thirdOctave;
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
		m_tileEngine.addTileLayer(new TileTerrainLayer(terrainTile));
		m_tileEngine.start(player.transform.position);
	}

	// Update is called once per frame
	void Update()
	{
		m_tileEngine.update(player.transform.position);
	}
}
