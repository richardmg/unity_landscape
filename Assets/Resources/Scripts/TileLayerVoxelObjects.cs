﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerVoxelObjects : MonoBehaviour, ITileLayer, ThingSubscriber 
{
	[Range (1, 100)]
	public int objectCount = 4;
	[Range (0, 100)]
	public float paddingBetweenObjects = 25;
	public GameObject prefab;

	GameObject[,] m_tileMatrix;
	float m_pivotAdjustmentY;
	TileEngine m_tileEngine;

	PrefabVariant m_prefabVariant;

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

		// Create one prefab variant that we can create many GameObject instances from
		m_prefabVariant = new PrefabVariant(prefab.name);
		// Hide prefab so we don't create the voxel objects upon construction
		prefab.SetActive(false);

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject tile = new GameObject();
				MeshFilter meshFilter = (MeshFilter)tile.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = (MeshRenderer)tile.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterial = VoxelObject.materialExact;

				tile.name = "Tile " + x + ", " + z;
				tile.transform.parent = transform;
				m_tileMatrix[x, z] = tile;

				initVoxelObjects(tile);
			}
		}

		PivotAdjustment pa = prefab.GetComponent<PivotAdjustment>();
		if (pa != null)
			m_pivotAdjustmentY = pa.adjustY;
	}

	public void onThingAdded(Thing thing)
	{
// TODO: add PrefabVariant into thing

		// Find out which tile is currently under the new things position
//		Vector2 matrixCoord = new Vector2();
//		m_tileEngine.matrixCoordFromWorldPos(thing.worldPos, ref matrixCoord);
//		GameObject tile = m_tileMatrix[(int)matrixCoord.x, (int)matrixCoord.y];
//
//		// Create and position an instance of the thing as a child of the tile
//		GameObject newThing = createPrefabVariantInstance(tile, thing.index, "Created on the fly!");
//		newThing.transform.position = thing.worldPos;
//
//		// Now that the tile has a new child, rebuild it
//		VoxelObject vo = m_voxelObjectMatrix[(int)matrixCoord.x, (int)matrixCoord.y];
//		vo.rebuildStandAlone();

//		Debug.Log("Added " + thing.index + " in tile " + tile.name + " at world pos " + thing.worldPos);
	}

	public void onPrefabVariantChanged(PrefabVariant prefabVariant)
	{
//		// Rebuild all tiles, since we don't keep track which tiles contains which objects
//		int tileCount = m_tileEngine.tileCount;
//		for (int z = 0; z < tileCount; ++z) {
//			for (int x = 0; x < tileCount; ++x) {
//				m_voxelObjectMatrix[x, z].rebuildStandAlone();
//			}
//		}
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject tile = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			tile.transform.position = desc.worldPos;
			moveVoxelObjects(tile);
			Mesh mesh = Root.instance.meshManager.createCombinedMesh(tile, Root.kLod0, null);
			tile.GetComponent<MeshFilter>().sharedMesh = mesh;
		}
	}

	public void removeAllTiles()
	{
		for (int i = 0; i < transform.childCount; ++i) {
			GameObject go = transform.GetChild(i).gameObject;
			UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(go); };
		}
	}

	private void initVoxelObjects(GameObject tile)
	{
		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				createPrefabVariantInstance(tile, m_prefabVariant, "VoxelObject: " + x + ", " + z);
			}
		}
	}

	private GameObject createPrefabVariantInstance(GameObject tile, PrefabVariant prefabVariant, string name)
	{
		GameObject go = prefabVariant.createInstance();
		go.name = name;
		go.transform.parent = tile.transform;
		go.SetActive(false);
		return go;
	}

	private void moveVoxelObjects(GameObject tile)
	{
		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				Vector3 worldPos = tile.transform.position + new Vector3(x * paddingBetweenObjects, 0, z * paddingBetweenObjects);
				Transform voTransform = tile.transform.GetChild((int)(z * objectCount) + x);
				int type = Root.instance.landscapeManager.getLandscapeType(worldPos);
				if (type == LandscapeManager.kForrest) {
					worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + m_pivotAdjustmentY;
					voTransform.position = worldPos;
//					voTransform.gameObject.GetComponent<VoxelObject>().setIndex(prefab.name);
				} else {
//					voTransform.gameObject.GetComponent<VoxelObject>().setIndex("clear");
				}

//				debug til engine
//				Vector2 matrixCoord = new Vector3();
//				terrainTileEngine.GetComponent<TileEngine>().matrixCoordFromWorldPos(worldPos, ref matrixCoord);
//				voTransform.gameObject.name = "Sample from: " + terrain.gameObject.name;
			}
		}
	}
}
