using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using LandscapeType = System.Int32;

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

public class EntityInstanceManager : MonoBehaviour, IProjectIOMember, IEntityInstanceListener
{
	public int pageCount = 4;
	public float pageSize = 1000;
	public int tilesPerPage = 100;

	[HideInInspector]
	public TileEngine tileEngine;

	private Page[,] m_pages;

	void Awake()
	{
		tileEngine = new TileEngine(pageCount, pageSize, updateTiles, null);
		initTiles();
		tileEngine.updateAllTiles();

		Root.instance.notificationManager.addEntityInstanceListener(this);
	}

	public void initTiles()
	{
		m_pages = new Page[pageCount, pageCount];
	}

	public void Update()
	{
		tileEngine.updateTiles(Root.instance.player.transform.position);
	}

	public void updateTiles(TileDescription[] tilesToUpdate)
	{
//		if (Root.instance.player.transform.position.x != 0 && Root.instance.player.transform.position.z != 0) {
//			Debug.Assert(false, "Moving outside entity mananger start tiles is not yet supported!");
//			// TODO: load/save tile data from disk async
//		}

		for (int i = 0; i < tilesToUpdate.Length; ++i) {
			TileDescription desc = tilesToUpdate[i];
			Page page = new Page(tileEngine.tileWorldSize, tilesPerPage, desc.worldPos);
			m_pages[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y] = page;
		}
	}

	public List<EntityInstanceDescription> getEntityInstanceDescriptionsForWorldPos(Vector3 worldPos)
	{
		IntCoord matrixCoord = tileEngine.matrixCoordForWorldPos(worldPos);
		Tile tile = m_pages[matrixCoord.x, matrixCoord.y].getTileForWorldPos(worldPos);
		return tile.entityInstanceDescriptions;
	}

	public void onEntityInstanceAdded(EntityInstanceDescription desc)
	{
		IntCoord matrixCoord = tileEngine.matrixCoordForWorldPos(desc.worldPos);
		Tile tile = m_pages[matrixCoord.x, matrixCoord.y].getTileForWorldPos(desc.worldPos);
		tile.entityInstanceDescriptions.Add(desc);
//		Debug.Log("add, world pos: " + desc.worldPos);
//		Debug.Log("add: tile coord:" + tileEngine.tileCoordForMatrixCoord(matrixCoord) + ", " + tileEngine.tileCoordAtWorldPos(desc.worldPos));
//		Debug.Log("add: matrix coord: " + matrixCoord);
//		Debug.Log("add: page size: " + tileEngine.tileWorldSize);
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

	public void onEntityInstanceRemoved(EntityInstanceDescription desc)
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
