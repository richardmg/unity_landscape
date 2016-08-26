using UnityEngine;
using System.Collections;

public class LandscapeConstructor : MonoBehaviour {

	[Range (0, 500)]
	public float tileHeightOct0 = 200;
	[Range (0, 100)]
	public float tileHeightOct1 = 10;
	[Range (0, 10)]
	public float tileHeightOct2 = 1;

	public bool showInEditor = false;

	[HideInInspector]
	public float noiseScaleOct0 = 0.003f;
	[HideInInspector]
	public float noiseScaleOct1 = 0.02f;
	[HideInInspector]
	public float noiseScaleOct2 = 0.1f;

	static public LandscapeConstructor m_instance;
	public LandscapeConstructor()
	{
		m_instance = this;
	}

	void OnValidate()
	{
		if (showInEditor) {
			foreach (TileEngine tileEngine in GetComponentsInChildren<TileEngine>())
				tileEngine.updateAllTiles();
		} else {
			foreach (TileEngine tileEngine in GetComponentsInChildren<TileEngine>(true))
				tileEngine.removeAllTiles();
		}
	}

	public static float getGroundHeight(float x, float z)
	{
		float oct0 = Mathf.PerlinNoise(x * m_instance.noiseScaleOct0, z * m_instance.noiseScaleOct0) * m_instance.tileHeightOct0;
		float oct1 = Mathf.PerlinNoise(x * m_instance.noiseScaleOct1, z * m_instance.noiseScaleOct1) * m_instance.tileHeightOct1;
		float oct2 = Mathf.PerlinNoise(x * m_instance.noiseScaleOct2, z * m_instance.noiseScaleOct2) * m_instance.tileHeightOct2;
		return oct0 + oct1 + oct2;
	}
}
