using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerGrass : ITileLayer 
{
	GameObject m_prefab;
	GameObject m_itemsParent;
	GameObject[] m_items;
	const int max_items = 100;

	public TileLayerGrass(string name, GameObject prefab)
	{
		m_prefab = prefab;
		m_itemsParent = new GameObject(name);
	}

	public void initTileLayer(TileEngine engine)
	{
		m_items = new GameObject[max_items];
		m_itemsParent.transform.SetParent(engine.parentTransform());
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
//		for (int i = 0; i < tilesToMove.Length; ++i) {
//			GameObject gameObject = getGameObject(tilesToMove[i].matrixCoord);
//			gameObject.GetComponent<ITile>().moveTile(tilesToMove[i], gameObject);
//		}
	}
}
