﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerVoxelObjects : MonoBehaviour, ITileLayer, IEntityClassListener, IEntityInstanceListener, IProjectListener 
{
	GameObject[,] m_tileMatrix;
	TileEngine m_tileEngine;

	public void initTileLayer(TileEngine engine)
	{
		// Ensure that the tile size matches the tile size of the smallest tiles in entityInstanceManager
		float baseTileSize = Root.instance.entityInstanceManager.tileEngine.tileWorldSize / Root.instance.entityInstanceManager.tilesPerPage;
		Debug.Assert(baseTileSize == engine.tileWorldSize, "tile size should match the tiles in entity instance manager");

		m_tileEngine = engine;
		int tileCount = engine.tileCount;
		m_tileMatrix = new GameObject[tileCount, tileCount];
		createAllTiles();

		Root.instance.notificationManager.addProjectListener(this);
		Root.instance.notificationManager.addEntityClassListener(this);
		Root.instance.notificationManager.addEntityInstanceListener(this);
	}

	public void updateTiles(TileDescription[] tilesToUpdate)
	{
		for (int i = 0; i < tilesToUpdate.Length; ++i) {
			TileDescription desc = tilesToUpdate[i];
			GameObject tile = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			tile.transform.position = desc.worldPos;
			destroyEntityInstances(tile);
			createEntityInstances(tile, desc);
			rebuildTileMesh(tile);
		}
	}

	public void onEntityInstanceAdded(EntityInstance entityInstance)
	{
		// Find out which tile is currently under the new things position
		int x, y;
		m_tileEngine.matrixCoordFromWorldPos(entityInstance.gameObject.transform.position, out x, out y);
		GameObject tile = m_tileMatrix[x, y];

		// Create and position an instance of the thing as a child of the tile
		entityInstance.gameObject.transform.parent = tile.transform;
		rebuildTileMesh(tile);
	}

	public void onEntityInstanceRemoved(EntityInstance entityInstance)
	{
	}

	public void onEntityInstanceSwapped(EntityInstance from, EntityInstance to)
	{
		GameObject tile = getTileAtPos(from.gameObject.transform.position);
		from.hideAndDestroy();
		to.gameObject.transform.parent = tile.transform;
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
		m_tileEngine.updateAllTiles();
	}

	GameObject getTileAtPos(Vector3 pos)
	{
		// Find out which tile is currently under the new things position
		int x, y;
		m_tileEngine.matrixCoordFromWorldPos(pos, out x, out y);
		return m_tileMatrix[x, y];
	}

	public void rebuildTileMesh(GameObject tile)
	{
//		Mesh mesh = EntityInstance.createCombinedMesh(tile, Root.kLod0);
//		tile.GetComponent<MeshFilter>().sharedMesh = mesh;

		EntityInstance[] selfAndchildren = tile.GetComponentsInChildren<EntityInstance>(true);
		for (int i = 0; i < selfAndchildren.Length; ++i)
			selfAndchildren[i].updateMesh();
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

	void destroyEntityInstances(GameObject tile)
	{
		Transform transform = tile.transform;
		for (int i = 0; i < transform.childCount; ++i) {
			GameObject go = transform.GetChild(i).gameObject;
			UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(go); };
		}
	}

	void createEntityInstances(GameObject tile, TileDescription tileDesc)
	{
		List<EntityInstanceDescription> instanceDescriptions
			= Root.instance.entityInstanceManager.getEntityInstanceDescriptionsForWorldPos(tileDesc.worldPos);

		foreach (EntityInstanceDescription instanceDesc in instanceDescriptions) {
			EntityClass entityClass = Root.instance.entityClassManager.getEntity(instanceDesc.entityClassID);
			EntityInstance entityInstance = entityClass.createInstance(tile.transform);
			entityInstance.gameObject.isStatic = true;
			entityInstance.transform.position = instanceDesc.transform.position;
			entityInstance.transform.rotation = instanceDesc.transform.rotation;
		}
	}
}
