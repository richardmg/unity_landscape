﻿using UnityEngine;
using System.Collections;

public class LandscapeConstructor : MonoBehaviour {

	public int rows = 4;
	public float tileWidth = 1000;

	public float noiseScaleOct0 = 0.003f;
	public float noiseScaleOct1 = 0.02f;
	public float noiseScaleOct2 = 0.1f;

	public float tileHeightOct0 = 200;
	public float tileHeightOct1 = 10;
	public float tileHeightOct2 = 1;

	public int groundResolution = 33;

	public Texture2D terrainTexture;
	public GameObject grassPrefab;
	public GameObject player;

	TileEngine m_tileEngine;

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

	public void constructLandscape()
	{
		m_tileEngine = new TileEngine(rows, tileWidth, transform);
		m_tileEngine.addLayer(new TileLayerTerrain("Ground", LandscapeTools.createGroundTerrainData()));
		m_tileEngine.addLayer(new TileLayerGrass("Grass", grassPrefab));
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
