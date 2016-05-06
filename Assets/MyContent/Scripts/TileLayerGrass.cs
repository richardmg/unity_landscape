using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerGrass : ITileLayer 
{
	GameObject m_itemsParent;
	GameObject[] m_items;
	const int max_items = 100;

	public TileLayerGrass(string name, GameObject tilePrefab, Transform parentTransform)
	{
		m_itemsParent = new GameObject(name);
		m_itemsParent.transform.SetParent(parentTransform);
	}

	public void initTiles(TileDescription[] tilesToInit)
	{
		for (int i = 0; i < tilesToInit.Length; ++i) {
//			ITile tileScript = (ITile)getGameObject(tilesToInit[i].matrixCoord).GetComponent<ITile>();
//			Debug.AssertFormat(tileScript != null, "TileTerrainLayer: tilePrefab needs to have a script attached that implements ITile");
//			tileScript.initTile(tilesToInit[i], getGameObject(tilesToInit[i].matrixCoord));
		}
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
//			GameObject gameObject = getGameObject(tilesToMove[i].matrixCoord);
//			gameObject.GetComponent<ITile>().moveTile(tilesToMove[i], gameObject);
		}
	}
}
