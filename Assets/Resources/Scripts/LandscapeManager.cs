﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LandscapeType = System.Int32;

public class LandscapePiece
{
	public LandscapeType landscapeType;
	public float density;
	public float height;
}

public class Thing
{
	public Vector3 worldPos;
	public string index;
}

public class WorldTile
{
	public List<Thing> things = new List<Thing>();
}

public class LandscapeManager : MonoBehaviour {

	[Range (0, 700)]
	public float tileHeightOct0 = 200;
	[Range (0, 200)]
	public float tileHeightOct1 = 10;
	[Range (0, 20)]
	public float tileHeightOct2 = 1;

	[Range (0, 1000)]
	public float treeBorder = 1;

	[HideInInspector]
	public float noiseScaleOct0 = 0.003f;
	[HideInInspector]
	public float noiseScaleOct1 = 0.02f;
	[HideInInspector]
	public float noiseScaleOct2 = 0.1f;

	public const LandscapeType kEmpty = 0;
	public const LandscapeType kSea = 1;
	public const LandscapeType kGrassLand = 2;
	public const LandscapeType kMeadow = 3;
	public const LandscapeType kForrest = 4;
	public const LandscapeType kLake = 5;

	private WorldTile[,] worldMatrix;

	void Awake()
	{
		// Create a matrix that holds all explicit things
		worldMatrix = new WorldTile[10, 10];
		for (int x = 0; x < worldMatrix.GetLength(0); ++x) {
			for (int y = 0; y < worldMatrix.GetLength(1); ++y) {
				worldMatrix[x, y] = new WorldTile();
			}
		}
	}

	void OnValidate()
	{
		foreach (TileEngine tileEngine in GetComponentsInChildren<TileEngine>()) {
			if (tileEngine.showInEditor)
				tileEngine.updateAllTiles();
		}
	}

	public float calculateHeight(float x, float z)
	{
		float oct0 = Mathf.PerlinNoise(x * noiseScaleOct0, z * noiseScaleOct0) * tileHeightOct0;
		float oct1 = Mathf.PerlinNoise(x * noiseScaleOct1, z * noiseScaleOct1) * tileHeightOct1;
		float oct2 = Mathf.PerlinNoise(x * noiseScaleOct2, z * noiseScaleOct2) * tileHeightOct2;
		return oct0 + oct1 + oct2;
	}

	public float sampleHeight(Vector3 worldPos)
	{
		return TileLayerTerrain.worldTerrain.sampleHeight(worldPos);
	}

	public LandscapeType getLandscapeType(Vector3 worldPos)
	{
		if (calculateHeight(worldPos.x, worldPos.z) > treeBorder)
			return kForrest;
		else
			return kMeadow;
	}

	public void addThing(Thing thing)
	{
		worldMatrix[0, 0].things.Add(thing);
		Root.instance.notificationManager.notifyThingAdded(thing);
	}

}