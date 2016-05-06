using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerGrass : ITileLayer 
{
	GameObject m_prefab;
	GameObject m_itemsParent;
	GameObject[] m_items;
	const int max_items = 100;

	public TileLayerGrass(string name, GameObject prefab, Transform parentTransform)
	{
		m_prefab = prefab;
		m_itemsParent = new GameObject(name);
		m_itemsParent.transform.SetParent(parentTransform);
		m_items = new GameObject[max_items];
	}

	public void initTiles(TileDescription[] tilesToInit)
	{
//		for (int i = 0; i < tilesToInit.Length; ++i) {
//			GameObject grassObject = (GameObject)GameObject.Instantiate(m_prefab, Vector3.zero, Quaternion.identity);
//			m_items[i] = grassObject;
//			ITile tileScript = (ITile)grassObject.GetComponent<ITile>();
//			Debug.AssertFormat(tileScript != null, this.GetType().Name + ": prefab needs to have a script attached that implements ITile");
//			tileScript.initTile(tilesToInit[i], grassObject);
//		}
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
//		for (int i = 0; i < tilesToMove.Length; ++i) {
//			GameObject gameObject = getGameObject(tilesToMove[i].matrixCoord);
//			gameObject.GetComponent<ITile>().moveTile(tilesToMove[i], gameObject);
//		}
	}
}
