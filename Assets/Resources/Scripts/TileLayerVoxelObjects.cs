﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerVoxelObjects : MonoBehaviour, ITileLayer, EntityListener, ProjectListener 
{
	[Range (1, 100)]
	public int objectCount = 4;
	[Range (0, 100)]
	public float paddingBetweenObjects = 25;

	GameObject[,] m_tileMatrix;
	TileEngine m_tileEngine;
	EntityClass m_entityClass;

	public void initTileLayer(TileEngine engine)
	{
		Debug.Assert(paddingBetweenObjects * (objectCount - 1) < engine.tileSize, "Warning: placing voxel objects outside tile bounds: " + gameObject.name);

		m_tileEngine = engine;
		int tileCount = engine.tileCount;
		m_tileMatrix = new GameObject[tileCount, tileCount];
		Root.instance.notificationManager.addProjectListener(this);
		Root.instance.notificationManager.addEntityListener(this);
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject tile = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			tile.transform.position = desc.worldPos;
			moveEntityInstances(tile);
			rebuildTileMesh(tile);
		}
	}

	public void onEntityInstanceAdded(EntityInstance entityInstance)
	{
		// Find out which tile is currently under the new things position
		Vector2 matrixCoord = new Vector2();
		m_tileEngine.matrixCoordFromWorldPos(entityInstance.gameObject.transform.position, ref matrixCoord);
		GameObject tile = m_tileMatrix[(int)matrixCoord.x, (int)matrixCoord.y];

		// Create and position an instance of the thing as a child of the tile
		entityInstance.gameObject.transform.parent = tile.transform;
		entityInstance.gameObject.SetActive(false);
		rebuildTileMesh(tile);
	}

	public void onEntityClassChanged(EntityClass entityClass)
	{
		// Rebuild all tiles, since we don't keep track which tiles contains which objects
		rebuildTileMeshes();
	}

	public void onEntityClassAdded(EntityClass entityClass)
	{
	}

	public void onEntityClassRemoved(EntityClass entityClass)
	{
	}

	public void onProjectLoaded()
	{
		m_entityClass = Root.instance.entityManager.getEntity(0);
		removeAllTiles();
		createAllTiles();
		m_tileEngine.updateAllTiles();
	}

	public void rebuildTileMesh(GameObject tile)
	{
		Mesh mesh = EntityInstance.createCombinedMesh(tile, Root.kLod0);
		tile.GetComponent<MeshFilter>().sharedMesh = mesh;
	}

	public void removeAllTiles()
	{
		for (int i = 0; i < transform.childCount; ++i) {
			GameObject go = transform.GetChild(i).gameObject;
			UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(go); };
		}
	}

	public void createAllTiles()
	{
		int tileCount = m_tileEngine.tileCount;
		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject tile = new GameObject();
				tile.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = (MeshRenderer)tile.AddComponent<MeshRenderer>();
				meshRenderer.sharedMaterial = Root.instance.voxelMaterialForLod(Root.kLod0);

				tile.name = "Tile " + x + ", " + z;
				tile.transform.parent = transform;
				m_tileMatrix[x, z] = tile;

				createEntityInstances(tile);
			}
		}
	}

	public void rebuildTileMeshes()
	{
		int tileCount = m_tileEngine.tileCount;
		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject tile = m_tileMatrix[x, z];
				rebuildTileMesh(tile);
			}
		}
	}

	void createEntityInstances(GameObject tile)
	{
		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				m_entityClass.createInstance(tile.transform, "VoxelObject: " + x + ", " + z);
			}
		}
	}

	private void moveEntityInstances(GameObject tile)
	{
		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				Vector3 worldPos = tile.transform.position + new Vector3(x * paddingBetweenObjects, 0, z * paddingBetweenObjects);
				Transform voTransform = tile.transform.GetChild((int)(z * objectCount) + x);
				int type = Root.instance.landscapeManager.getLandscapeType(worldPos);
				if (type == LandscapeManager.kForrest) {
					worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos);
					voTransform.position = worldPos;
					voTransform.gameObject.GetComponent<EntityInstance>().instanceHidden = false;
				} else {
					voTransform.gameObject.GetComponent<EntityInstance>().instanceHidden = true;
				}

//				debug til engine
//				Vector2 matrixCoord = new Vector3();
//				terrainTileEngine.GetComponent<TileEngine>().matrixCoordFromWorldPos(worldPos, ref matrixCoord);
//				voTransform.gameObject.name = "Sample from: " + terrain.gameObject.name;
			}
		}
	}
}
