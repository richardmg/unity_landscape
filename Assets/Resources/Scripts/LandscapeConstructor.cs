using UnityEngine;
using System.Collections;

using LandscapeType = System.Int32;

public class LandscapePiece
{
	public LandscapeType landscapeType;
	public float density;
	public float height;
}

public class LandscapeConstructor : MonoBehaviour {

	[Range (0, 700)]
	public float tileHeightOct0 = 200;
	[Range (0, 200)]
	public float tileHeightOct1 = 10;
	[Range (0, 20)]
	public float tileHeightOct2 = 1;

	[HideInInspector]
	public float noiseScaleOct0 = 0.003f;
	[HideInInspector]
	public float noiseScaleOct1 = 0.02f;
	[HideInInspector]
	public float noiseScaleOct2 = 0.1f;

	public const LandscapeType kEmpty = 0;
	public const LandscapeType kSea = 1;
	public const LandscapeType kGrassLand = 2;
	public const LandscapeType kMeadow = 2;
	public const LandscapeType kForrest = 2;
	public const LandscapeType kLake = 2;

	static public LandscapeConstructor instance;
	public LandscapeConstructor()
	{
		instance = this;
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
		float oct0 = Mathf.PerlinNoise(x * instance.noiseScaleOct0, z * instance.noiseScaleOct0) * instance.tileHeightOct0;
		float oct1 = Mathf.PerlinNoise(x * instance.noiseScaleOct1, z * instance.noiseScaleOct1) * instance.tileHeightOct1;
		float oct2 = Mathf.PerlinNoise(x * instance.noiseScaleOct2, z * instance.noiseScaleOct2) * instance.tileHeightOct2;
		return oct0 + oct1 + oct2;
	}

	public float sampleHeight(Vector3 worldPos)
	{
		return TileLayerTerrain.worldTerrain.sampleHeight(worldPos);
	}

	public LandscapeType getLandscapeType(float x, float z)
	{
		return kForrest;
	}
}
