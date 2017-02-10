using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerVoxelObjects : MonoBehaviour, IEntityClassListener, IEntityInstanceDescriptionListener, IProjectListener 
{
	public int tileCount = 4;
	public float tileSize = 1000;

	GameObject[,] m_tileMatrix;
	TileEngine m_tileEngine;

	public void Awake()
	{
		m_tileEngine = new TileEngine(tileCount, tileSize, updateTiles, null);
		initTiles();
		m_tileEngine.updateAllTiles();

		Root.instance.notificationManager.addProjectListener(this);
		Root.instance.notificationManager.addEntityClassListener(this);
		Root.instance.notificationManager.addEntityInstanceListener(this);
	}

//	public void Start()
//	{
//		// Create some dummy trees for debug
//		for (int z = 0; z < tileCount; ++z) {
//			for (int x = 0; x < tileCount; ++x) {
//				Transform tileTransform = m_tileMatrix[x, z].transform;
//				EntityClass entityClass = Root.instance.entityClassManager.getEntity(0);
//				Vector3 pos = tileTransform.position;
//				pos.y = Root.instance.landscapeManager.sampleHeight(pos);
//				EntityInstanceDescription desc = new EntityInstanceDescription(entityClass, pos);
//				EntityInstance e = desc.createInstance(tileTransform);
//				e.makeStandalone(Root.kLod0);
//			}
//		}
//	}

	public void initTiles()
	{
		// Ensure that the tile size matches the tile size of the smallest tiles in entityInstanceManager
		float baseTileSize = Root.instance.entityInstanceManager.tileEngine.tileWorldSize / Root.instance.entityInstanceManager.tilesPerPage;
		Debug.Assert(baseTileSize == m_tileEngine.tileWorldSize, "tile size should match the tiles in entity instance manager");

		m_tileMatrix = new GameObject[tileCount, tileCount];
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

	public void Update()
	{
		m_tileEngine.updateTiles(Root.instance.player.transform.position);
	}

	void updateTiles(TileDescription[] tilesToUpdate)
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

	public void onEntityInstanceDescriptionAdded(EntityInstanceDescription desc)
	{
		// Find out which tile is currently under the new things position
		IntCoord matrixCoord = m_tileEngine.matrixCoordForWorldPos(desc.worldPos);
		GameObject tile = m_tileMatrix[matrixCoord.x, matrixCoord.y];
		createInstance(tile, desc);
		rebuildTileMesh(tile);
	}

	public void onEntityInstanceDescriptionRemoved(EntityInstanceDescription desc)
	{
		destroyInstance(desc);
	}

	public void onEntityInstanceDescriptionChanged(EntityInstanceDescription desc)
	{
		desc.instance.syncTransformWithDescription();
	}

	public void onEntityClassChanged(EntityClass entityClass)
	{
		// Rebuild all tiles, since we don't keep track which tiles contains which objects
		rebuildTileMeshes();
	}

	public void onEntityClassAdded(EntityClass entityClass)
	{}

	public void onEntityClassRemoved(EntityClass entityClass)
	{}

	public void onProjectLoaded()
	{
		m_tileEngine.updateAllTiles();
	}

	public void rebuildTileMesh(GameObject tile)
	{
		// Here we need to rebuild tile mesh in addition to self-standing
		// entity instance (complex (many individual voxel objects) or single
		// (combined mesh)). Currently we only handle complex entity instances.

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
			GameObject.Destroy(go);
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

	GameObject createInstance(GameObject tile, EntityInstanceDescription desc)
	{
		EntityClass entityClass = Root.instance.entityClassManager.getEntity(desc.entityClassID);
		return entityClass.createGameObject(tile.transform, desc, Root.kLod0);
	}

	void createEntityInstances(GameObject tile, TileDescription tileDesc)
	{
		List<EntityInstanceDescription> instanceDescriptions
			= Root.instance.entityInstanceManager.getEntityInstanceDescriptionsForWorldPos(tileDesc.worldPos);

		// TODO: get existing game objects / voxel objects from pool
		// TODO: In the pool, first check if an instance with correct class ID exists, before modifying a different one.
		foreach (EntityInstanceDescription instanceDesc in instanceDescriptions)
			createInstance(tile, instanceDesc);
	}

	void destroyInstance(EntityInstanceDescription desc)
	{
		if (desc.instance == null) {
			// This can happen because of an internal
			// 'destroy' delay inside unity, is seems
			return;
		}

		desc.instance.gameObject.hideAndDestroy();
		desc.instance = null;
	}

	void destroyEntityInstances(GameObject tile)
	{
		Transform transform = tile.transform;
		for (int i = 0; i < transform.childCount; ++i) {
			// TODO: add to pool
			GameObject go = transform.GetChild(i).gameObject;
			destroyInstance(go.GetComponent<EntityInstance>().entityInstanceDescription);
		}
	}
}
