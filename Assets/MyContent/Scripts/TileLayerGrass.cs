using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerGrass : ITileLayer 
{
	GameObject m_prefab;
	GameObject m_layerRoot;
	GameObject[,] m_tileMatrix;

	const int max_items = 100;

	public TileLayerGrass(string name, GameObject prefab)
	{
		m_prefab = prefab;
		m_layerRoot = new GameObject(name);
	}

	public void initTileLayer(TileEngine engine)
	{
		int tileCount = engine.tileCount();
		m_tileMatrix = new GameObject[tileCount, tileCount];
		m_layerRoot.transform.SetParent(engine.parentTransform());

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				m_tileMatrix[x, z] = (GameObject)GameObject.Instantiate(m_prefab, Vector3.zero, Quaternion.identity); 
				m_tileMatrix[x, z].transform.SetParent(m_layerRoot.transform);
			}
		}
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject tileObject = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			Vector3 worldPos = desc.worldPos;
			worldPos.y = LandscapeConstructor.getGroundHeight(worldPos.x, worldPos.z);
			tileObject.transform.position = worldPos;
		}
	}
}
