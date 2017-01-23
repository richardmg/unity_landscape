using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LandscapeType = System.Int32;

public class LandscapeManager : MonoBehaviour
{
	[Range (0, 700)] public float tileHeightOct0 = 200;
	[Range (0, 200)] public float tileHeightOct1 = 10;
	[Range (0,  20)] public float tileHeightOct2 = 1;
	[Range (0,   1)] public float treeBorder = 0.5f;

	[HideInInspector] public float noiseScaleOct0 = 0.003f;
	[HideInInspector] public float noiseScaleOct1 = 0.02f;
	[HideInInspector] public float noiseScaleOct2 = 0.1f;

	public const LandscapeType kEmpty = 0;
	public const LandscapeType kSea = 1;
	public const LandscapeType kGrassLand = 2;
	public const LandscapeType kMeadow = 3;
	public const LandscapeType kForrest = 4;
	public const LandscapeType kLake = 5;

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
		Debug.Assert(false, "Needs reenginering!");
		if (calculateHeight(worldPos.x, worldPos.z) < (treeBorder * (tileHeightOct0 + tileHeightOct1 + tileHeightOct2)))
			return kForrest;
		else
			return kMeadow;
	}

}
