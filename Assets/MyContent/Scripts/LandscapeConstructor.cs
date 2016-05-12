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

	public Texture2D terrainTexture;
	public GameObject grassPrefab;
	public GameObject player;

	TileEngine m_tileEngine;

	static public LandscapeConstructor m_instance;

	public static float getGroundHeight(float x, float z)
	{
		float firstOctave = Mathf.PerlinNoise(x * m_instance.perlinLargeScale, z * m_instance.perlinLargeScale) * m_instance.landscapeHeightLargeScale;
		float secondOctave = Mathf.PerlinNoise(x * m_instance.perlinMediumScale, z * m_instance.perlinMediumScale) * m_instance.landscapeHeightMediumScale;
		float thirdOctave = Mathf.PerlinNoise(x * m_instance.perlinSmallScale, z * m_instance.perlinSmallScale) * m_instance.landscapeSmallScale;
		return firstOctave + secondOctave + thirdOctave;
	}

	public void constructLandscape()
	{
		if (m_instance)
			return;

		m_instance = this;

		m_tileEngine = new TileEngine(rows, tileWidth);
		m_tileEngine.addTileLayer(new TileLayerTerrain("Ground", LandscapeTools.createGroundTerrainData(), transform));
		m_tileEngine.addTileLayer(new TileLayerGrass("Grass", grassPrefab, transform));
		m_tileEngine.start(player.transform.position);
	}

	public void movePlayerOnTop()
	{
		// Move player on top of landscape
		Vector3 playerPos = player.transform.position;
		playerPos.y = getGroundHeight(playerPos.x, playerPos.z) + 1;
		player.transform.position = playerPos;
	}

	// Use this for initialization
	void Start()
	{
		constructLandscape();
		movePlayerOnTop();
	}

	// Update is called once per frame
	public void Update()
	{
		m_tileEngine.update(player.transform.position);
	}
}
