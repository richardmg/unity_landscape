using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerVoxelObjects : MonoBehaviour, ITileLayer 
{
	[Range (1, 100)]
	public int objectCount = 4;
	[Range (0, 100)]
	public float paddingBetweenObjects = 25;
	public GameObject prefab;

	GameObject[,] m_tileMatrix;
	VoxelObject[,] m_voxelObjectMatrix;
	float m_pivotAdjustmentY;
	TileEngine m_tileEngine;

	public void OnValidate()
	{
		if (m_tileEngine == null)
			return;
		if (!m_tileEngine.showInEditor)
			return;

		int currentObjectCount = (int)Mathf.Sqrt(transform.GetChild(0).childCount);
		if (currentObjectCount != objectCount)
			m_tileEngine.OnValidate();
		else
			m_tileEngine.updateAllTiles();
	}

	public void initTileLayer(TileEngine engine)
	{
		Debug.Assert(paddingBetweenObjects * (objectCount - 1) < engine.tileSize, "Warning: placing voxel objects outside tile bounds: " + gameObject.name);

		m_tileEngine = engine;
		int tileCount = engine.tileCount;
		m_tileMatrix = new GameObject[tileCount, tileCount];
		m_voxelObjectMatrix = new VoxelObject[tileCount, tileCount];

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject goTile = new GameObject();
				goTile.name = "Tile " + x + ", " + z;
				goTile.transform.parent = transform;
				m_tileMatrix[x, z] = goTile;

				VoxelObject vo = goTile.AddComponent<VoxelObject>();
				m_voxelObjectMatrix[x, z] = vo;
				vo.setIndex(VoxelObject.indexToString(VoxelObject.kIndexTopLevel));
				vo.initAsStandAlone();
				initVoxelObjects(goTile);
			}
		}

		PivotAdjustment pa = prefab.GetComponent<PivotAdjustment>();
		if (pa != null)
			m_pivotAdjustmentY = pa.adjustY;
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject go = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			go.transform.position = desc.worldPos;
			moveVoxelObjects(go);
			VoxelObject vo = m_voxelObjectMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			vo.rebuildStandAlone();
		}
	}

	public void removeAllTiles()
	{
		for (int i = 0; i < transform.childCount; ++i) {
			GameObject go = transform.GetChild(i).gameObject;
			UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(go); };
		}
	}

	private void initVoxelObjects(GameObject goTile)
	{
		// Hide prefab so we don't create the voxel objects upon construction
		prefab.SetActive(false);

		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				GameObject go = new GameObject();
				go.name = "VoxelObject: " + x + ", " + z;
				go.transform.parent = goTile.transform;
				VoxelObject vo = go.AddComponent<VoxelObject>();
				vo.setIndex(prefab.name);
				vo.transform.localScale = prefab.transform.localScale;
				vo.gameObject.SetActive(false);
			}
		}
	}

	private void moveVoxelObjects(GameObject goTile)
	{
		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				Vector3 worldPos = goTile.transform.position + new Vector3(x * paddingBetweenObjects, 0, z * paddingBetweenObjects);
				Transform voTransform = goTile.transform.GetChild((int)(z * objectCount) + x);
				int type = LandscapeConstructor.instance.getLandscapeType(worldPos);
				if (type == LandscapeConstructor.kForrest) {
					worldPos.y = LandscapeConstructor.instance.sampleHeight(worldPos) + m_pivotAdjustmentY;
					voTransform.position = worldPos;
					voTransform.gameObject.GetComponent<VoxelObject>().setIndex(prefab.name);
				} else {
					voTransform.gameObject.GetComponent<VoxelObject>().setIndex("clear");
				}

//				debug til engine
//				Vector2 matrixCoord = new Vector3();
//				terrainTileEngine.GetComponent<TileEngine>().matrixCoordFromWorldPos(worldPos, ref matrixCoord);
//				voTransform.gameObject.name = "Sample from: " + terrain.gameObject.name;
			}
		}
	}
}
