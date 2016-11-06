using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LandscapeType = System.Int32;

public class TileLayer1Static
{
	public LandscapeType landscapeType;
	public float height;
	public int entityClassID;
	public Transform transform;
//	public List<GameObject> entityInstances = new List<GameObject>();
}

public class TileLayer0
{
	public TileLayer1Static[,] m_squares;

	public TileLayer0()
	{
		m_squares = new TileLayer1Static[10, 10];
		for (int x = 0; x < m_squares.GetLength(0); ++x) {
			for (int y = 0; y < m_squares.GetLength(1); ++y) {
				m_squares[x, y] = new TileLayer1Static();
			}
		}
	}
}

public class LandscapeManager : MonoBehaviour,  IProjectIOMember, ITileLayer {

	[Range (0, 700)]
	public float tileHeightOct0 = 200;
	[Range (0, 200)]
	public float tileHeightOct1 = 10;
	[Range (0, 20)]
	public float tileHeightOct2 = 1;

	[Range (0, 1)]
	public float treeBorder = 0.5f;

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

	private TileLayer0[,] m_tiles;

	TileEngine m_tileEngine;

	void Awake()
	{
		m_tileEngine = new TileEngine();
		m_tileEngine.tileCount = 4;
		m_tileEngine.tileSize = 100;
		m_tileEngine.addTileLayer(this);
		m_tileEngine.init();

//		clearTiles();
	}

	void OnValidate()
	{
		foreach (TileEngine tileEngine in GetComponentsInChildren<TileEngine>()) {
			if (tileEngine.showInEditor)
				tileEngine.updateAllTiles();
		}
	}

	void clearTiles()
	{
		m_tiles = new TileLayer0[10, 10];
		for (int x = 0; x < m_tiles.GetLength(0); ++x) {
			for (int y = 0; y < m_tiles.GetLength(1); ++y) {
				m_tiles[x, y] = new TileLayer0();
			}
		}
	}

	public void initTileLayer(TileEngine engine)
	{
		Debug.Assert(false, "Not implemented");
	}

	public void updateTiles(TileDescription[] tilesToUpdate)
	{
		Debug.Assert(false, "Not implemented");
	}

	public void removeAllTiles()
	{
		Debug.Assert(false, "Not implemented");
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
		if (calculateHeight(worldPos.x, worldPos.z) < (treeBorder * (tileHeightOct0 + tileHeightOct1 + tileHeightOct2)))
			return kForrest;
		else
			return kMeadow;
	}

	public void addEntityInstance(EntityInstance entityInstance)
	{
//		m_tiles[0, 0].entityInstances.Add(entityInstance.gameObject);
//		Root.instance.notificationManager.notifyEntityInstanceAdded(entityInstance);
	}

	public void swapEntityInstance(EntityInstance from, EntityInstance to)
	{
//		to.gameObject.transform.position = from.gameObject.transform.position;
//		to.gameObject.transform.rotation = from.gameObject.transform.rotation;
//		m_tiles[0, 0].entityInstances.Remove(from.gameObject);
//		m_tiles[0, 0].entityInstances.Add(to.gameObject);
//		Root.instance.notificationManager.notifyEntityInstanceSwapped(from, to);
	}

	public void initNewProject()
	{
		clearTiles();	
	}

	public void load(ProjectIO projectIO)
	{
		clearTiles();	
	}

	public void save(ProjectIO projectIO)
	{
	}

}
