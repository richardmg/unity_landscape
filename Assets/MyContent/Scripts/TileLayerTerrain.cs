using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerTerrain : ITileTerrainLayer
{
	GameObject[,] m_tileMatrix;

	public TileLayerTerrain(string name, GameObject tilePrefab, Transform parentTransform)
	{
		int count = LandscapeConstructor.m_instance.rows;
		m_tileMatrix = new GameObject[count, count];
		Transform childRoot = parentTransform.Find(name);

		// If a child with name already exists, adopt its
		// children instead of creating new ones.
		if (childRoot == null)
			constructNewGameobjects(name, tilePrefab, parentTransform);
		else
			adoptChildren(childRoot);

	}

	public void constructNewGameobjects(string name, GameObject tilePrefab, Transform parentTransform)
	{
		Transform tilesParent = new GameObject(name).transform;
		tilesParent.SetParent(parentTransform);
		int count = m_tileMatrix.GetLength(0);

		for (int z = 0; z < count; ++z) {
			for (int x = 0; x < count; ++x) {
				m_tileMatrix[x, z] = (GameObject)GameObject.Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
				m_tileMatrix[x, z].transform.SetParent(tilesParent);
			}
		}
	}

	public void adoptChildren(Transform childrenRoot)
	{
		int i = 0;
		int count = m_tileMatrix.GetLength(0);

		foreach (Transform child in childrenRoot) {
			m_tileMatrix[i % count, (int)(i / count)] = child.gameObject;
			i++;
		}
	}

	GameObject getGameObject(Vector2 matrixPos)
	{
		return (int)matrixPos.x == -1 ? null : m_tileMatrix[(int)matrixPos.x, (int)matrixPos.y];
	}

	Terrain getTerrain(Vector2 matrixPos)
	{
		return (int)matrixPos.x == -1 ? null : m_tileMatrix[(int)matrixPos.x, (int)matrixPos.y].GetComponent<Terrain>();
	}

	public void initTiles(TileDescription[] tilesToInit)
	{
		for (int i = 0; i < tilesToInit.Length; ++i) {
			ITile tileScript = (ITile)getGameObject(tilesToInit[i].matrixCoord).GetComponent<ITile>();
			Debug.AssertFormat(tileScript != null, "TileTerrainLayer: tilePrefab needs to have a script attached that implements ITile");
			tileScript.initTile(tilesToInit[i], getGameObject(tilesToInit[i].matrixCoord));
		}
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			GameObject gameObject = getGameObject(tilesToMove[i].matrixCoord);
			gameObject.GetComponent<ITile>().moveTile(tilesToMove[i], gameObject);
		}
	}

	public void updateTileNeighbours(TileDescription[] tilesWithNewNeighbours)
	{
		for (int i = 0; i < tilesWithNewNeighbours.Length; ++i) {
			Terrain t = getTerrain(tilesWithNewNeighbours[i].matrixCoord);
			TileNeighbours tn = tilesWithNewNeighbours[i].neighbours;

			Terrain top = getTerrain(tn.top);
			Terrain bottom = getTerrain(tn.bottom);
			Terrain left = getTerrain(tn.left);
			Terrain right = getTerrain(tn.right);

			t.SetNeighbors(left, top, right, bottom);
		}
	}
}
