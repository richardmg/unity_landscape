using UnityEngine;
using System.Collections;

public class LandscapeConstructor : MonoBehaviour {

	public float flyOffset = 0;
	public float zOffset = 0;

	public bool showLandscape = true;
	public bool showFarTiles = true;
	public bool showNearTiles = true;

	public int tileCountLandscape = 4;
	public int tileCountFarObjects = 4;
	public int tileCountNearObjects = 4;

	public float tileSizeLandscape = 1000;
	public float tileSizeFarObjects = 100;
	public float tileSizeNearObjects = 10;

	public float tileHeightOct0 = 200;
	public float tileHeightOct1 = 10;
	public float tileHeightOct2 = 1;

	public int groundResolution = 33;

	public GameObject player;
	public Texture2D terrainTexture;
	public GameObject grassPrefab;
	public GameObject treePrefab;

	[HideInInspector]
	public float noiseScaleOct0 = 0.003f;
	[HideInInspector]
	public float noiseScaleOct1 = 0.02f;
	[HideInInspector]
	public float noiseScaleOct2 = 0.1f;

	TileEngine m_tileEngineLandscape;
	TileEngine m_tileEngineNear;
	TileEngine m_tileEngineFar;

	static public LandscapeConstructor m_instance;
	public LandscapeConstructor()
	{
		m_instance = this;
	}

	public static float getGroundHeight(float x, float z)
	{
		float oct0 = Mathf.PerlinNoise(x * m_instance.noiseScaleOct0, z * m_instance.noiseScaleOct0) * m_instance.tileHeightOct0;
		float oct1 = Mathf.PerlinNoise(x * m_instance.noiseScaleOct1, z * m_instance.noiseScaleOct1) * m_instance.tileHeightOct1;
		float oct2 = Mathf.PerlinNoise(x * m_instance.noiseScaleOct2, z * m_instance.noiseScaleOct2) * m_instance.tileHeightOct2;
		return oct0 + oct1 + oct2;
	}

	public void rebuildLandscape()
	{
		while (transform.childCount > 0)
			GameObject.DestroyImmediate(transform.GetChild(0).gameObject);

		m_tileEngineLandscape = new TileEngine(tileCountLandscape, tileSizeLandscape, transform);
		m_tileEngineNear = new TileEngine(tileCountNearObjects, tileSizeNearObjects, transform);
		m_tileEngineFar = new TileEngine(tileCountFarObjects, tileSizeFarObjects, transform);

		if (showLandscape)
			m_tileEngineLandscape.addLayer(new TileLayerTerrain("Ground", LandscapeTools.createGroundTerrainData()));

		if (showFarTiles)
			m_tileEngineFar.addLayer(new TileLayerTrees("Trees", treePrefab));

		m_tileEngineLandscape.start(player.transform.position);
		m_tileEngineNear.start(player.transform.position);
		m_tileEngineFar.start(player.transform.position);
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
		rebuildLandscape();
		movePlayerOnTop();
	}

	// Update is called once per frame
	public void Update()
	{
		Vector3	pos = player.transform.position;
		if (flyOffset != 0) {
			pos.y = getGroundHeight(pos.x, pos.z) + flyOffset;
			player.transform.position = pos;
		}

		pos.z += zOffset;

		m_tileEngineLandscape.update(pos);
		m_tileEngineNear.update(pos);
		m_tileEngineFar.update(pos);
	}
}
