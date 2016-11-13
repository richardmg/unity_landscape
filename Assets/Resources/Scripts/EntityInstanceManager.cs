using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LandscapeType = System.Int32;

public class EntityInstanceDescription
{
	public int entityClassID;	
	public Transform transform;
}

public class Tile
{
	public List<EntityInstanceDescription> entityInstanceDescriptions = new List<EntityInstanceDescription>();
}

public class Page
{
	public Tile[,] m_tiles;
	public Vector3 m_pageWorldPos;
	float m_tileSize;

	public Page(float tileSize, int tileCount, Vector3 pageWorldPos)
	{
		m_pageWorldPos = pageWorldPos;
		m_tileSize = tileSize;
		m_tiles = new Tile[tileCount, tileCount];
		for (int x = 0; x < tileCount; ++x) {
			for (int y = 0; y < tileCount; ++y) {
				m_tiles[x, y] = new Tile();
			}
		}
	}

	public Tile getTileForWorldPos(Vector3 worldPos)
	{
		int coordX = Mathf.FloorToInt((worldPos.x - m_pageWorldPos.x) / m_tileSize);
		int coordY = Mathf.FloorToInt((worldPos.z - m_pageWorldPos.z) / m_tileSize);
		return m_tiles[coordX, coordY];
	}
}

public class EntityInstanceManager : MonoBehaviour, IProjectIOMember, ITileLayer, IEntityInstanceListener
{
	[HideInInspector]
	public TileEngine tileEngine;

	private Page[,] m_pages;

	// This manager has a set of pages. Each page is divided into a a number of tiles. And each
	// tile contains a list of EntityInstanceDescriptions. Pages are supposed to be big, and will
	// be loaded and saved to disk as the player moves around. The tiles are small, and will be
	// aligned with the tiles that contain actual EntityInstances in the scene, so that instance
	// engines will cover a certain amount of tiles. But they will not be as big as a page.
	public int tilesPerPage = 100;

	void Awake()
	{
		Root.instance.notificationManager.addEntityInstanceListener(this);
	}

	public void initTileLayer(TileEngine engine)
	{
		tileEngine = engine;
		m_pages = new Page[engine.tileCount, engine.tileCount];
		engine.updateAllTiles();
	}

	public void updateTiles(TileDescription[] tilesToUpdate)
	{
		if (Root.instance.player.transform.position.x != 0 && Root.instance.player.transform.position.z != 0) {
			Debug.Assert(false, "Moving outside entity mananger start tiles is not yet supported!");
			// TODO: load/save tile data from disk async
		}

		for (int i = 0; i < tilesToUpdate.Length; ++i) {
			TileDescription desc = tilesToUpdate[i];
			Page page = new Page(tileEngine.tileWorldSize, tilesPerPage, desc.worldPos);
			m_pages[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y] = page;
		}
	}

	public void removeAllTiles()
	{
		Debug.Assert(false, "Not implemented");
	}

	public List<EntityInstanceDescription> getEntityInstanceDescriptionsForWorldPos(Vector3 worldPos)
	{
		int matrixX, matrixY;
		tileEngine.matrixCoordFromWorldPos(worldPos, out matrixX, out matrixY);
		Tile tile = m_pages[matrixX, matrixY].getTileForWorldPos(worldPos);
		return tile.entityInstanceDescriptions;
	}

	public void onEntityInstanceAdded(EntityInstance entityInstance)
	{
		EntityInstanceDescription desc = new EntityInstanceDescription();
		desc.entityClassID = entityInstance.entityClass.id;
		desc.transform = entityInstance.transform;

		int x, y;
		Vector3 worldPos = entityInstance.transform.position;
		tileEngine.matrixCoordFromWorldPos(worldPos, out x, out y);
		Tile tile = m_pages[x, y].getTileForWorldPos(worldPos);
		tile.entityInstanceDescriptions.Add(desc);
	}

	public void onEntityInstanceSwapped(EntityInstance from, EntityInstance to)
	{
		Debug.Assert(false, "Not implemented!");
		//		to.gameObject.transform.position = from.gameObject.transform.position;
		//		to.gameObject.transform.rotation = from.gameObject.transform.rotation;
		//		m_tiles[0, 0].entityInstances.Remove(from.gameObject);
		//		m_tiles[0, 0].entityInstances.Add(to.gameObject);
		//		Root.instance.notificationManager.notifyEntityInstanceSwapped(from, to);
	}

	public void onEntityInstanceRemoved(EntityInstance entityInstance)
	{
		Debug.Assert(false, "Not implemented!");
	}

	public void initNewProject()
	{
	}

	public void load(ProjectIO projectIO)
	{
	}

	public void save(ProjectIO projectIO)
	{
	}

}
