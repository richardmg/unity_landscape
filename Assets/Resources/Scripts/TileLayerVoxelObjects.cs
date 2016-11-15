using UnityEngine;
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
//			destroyEntityInstances(tile);
//			createEntityInstances(tile, desc);
			rebuildTileMesh(tile);
		}
	}

	public void onEntityInstanceAdded(EntityInstanceDescription desc)
	{
		// Find out which tile is currently under the new things position
		float tileX, tileZ;
		int matrixX, matrixY;
		m_tileEngine.tileCoordAtWorldPos(desc.worldPos, out tileX, out tileZ);
		m_tileEngine.matrixCoordForTileCoord(tileX, tileZ, out matrixX, out matrixY);
		GameObject tile = m_tileMatrix[matrixX, matrixY];

		desc.createInstance(tile.transform);

		rebuildTileMesh(tile);
	}

	public void onEntityInstanceRemoved(EntityInstanceDescription desc)
	{
		desc.destroyInstance();
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

	GameObject getTileAtPos(Vector3 worldPos)
	{
		// Find out which tile is currently under the new things position
		float tileX, tileZ;
		int matrixX, matrixY;
		m_tileEngine.tileCoordAtWorldPos(worldPos, out tileX, out tileZ);
		m_tileEngine.matrixCoordForTileCoord(tileX, tileZ, out matrixX, out matrixY);
		return m_tileMatrix[matrixX, matrixY];
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

	void createEntityInstances(GameObject tile, TileDescription tileDesc)
	{
		List<EntityInstanceDescription> instanceDescriptions
			= Root.instance.entityInstanceManager.getEntityInstanceDescriptionsForWorldPos(tileDesc.worldPos);

		foreach (EntityInstanceDescription instanceDesc in instanceDescriptions)
			instanceDesc.createInstance(tile.transform);
	}

	void destroyEntityInstances(GameObject tile)
	{
		Transform transform = tile.transform;
		for (int i = 0; i < transform.childCount; ++i) {
			GameObject go = transform.GetChild(i).gameObject;
			EntityInstance instance = go.GetComponent<EntityInstance>();
			instance.entityInstanceDescription.destroyInstance();
		}
	}
}
