﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerVoxelObjects : MonoBehaviour, ITileLayer 
{
	[Range (1, 100)]
	public int objectCount = 4;
	[Range (0, 100)]
	public float paddingBetweenObjects = 25;
	public GameObject prefab;
	public bool showInEditor = false;

	GameObject[,] m_tileMatrix;
	float m_pivotAdjustmentY = 0;

	public void OnValidate()
	{
		if (showInEditor) {
			TileEngine engine = GetComponentInParent<TileEngine>();
			if (engine)
				engine.rebuild();
		} else {
			removeAllTiles();
		}
	}

	public void OnLandscapeGeneratorUpdate()
	{
		if (!showInEditor)
			return;

		TileEngine engine = GetComponentInParent<TileEngine>();
		if (engine)
			engine.updateAllTiles();
	}

	public void initTileLayer(TileEngine engine)
	{
		int tileCount = engine.tileCount;
		m_tileMatrix = new GameObject[tileCount, tileCount];

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject goTile = new GameObject();
				goTile.name = "Tile " + x + ", " + z;
				goTile.transform.parent = transform;
				m_tileMatrix[x, z] = goTile;

				VoxelObject vo = goTile.AddComponent<VoxelObject>();
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
			GameObject goTile = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			goTile.transform.position = desc.worldPos;
			moveVoxelObjects(goTile);
			goTile.GetComponent<VoxelObject>().rebuildStandAlone();
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
		// TODO: create a bunch of voxel objects based on noise

		// Hide prefab so we don't create the voxel objects upon construction
		prefab.SetActive(false);

		int count = objectCount * objectCount;

		for (int i = 0; i < count; ++i) {
			GameObject go = new GameObject();
			go.transform.parent = goTile.transform;
			VoxelObject vo = go.AddComponent<VoxelObject>();
			vo.setIndex(prefab.name);
			vo.transform.localScale = prefab.transform.localScale;
			vo.gameObject.SetActive(false);
		}
	}

	private void moveVoxelObjects(GameObject goTile)
	{
		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				Vector3 worldPos = goTile.transform.position + new Vector3(x * paddingBetweenObjects, 0, z * paddingBetweenObjects);
				worldPos.y = LandscapeConstructor.getGroundHeight(worldPos.x, worldPos.z) + m_pivotAdjustmentY;
				Transform voTransform = goTile.transform.GetChild((int)(z * objectCount) + x);
				voTransform.position = worldPos;
			}
		}
	}
}
