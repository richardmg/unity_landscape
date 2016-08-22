using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerTrees : ITileLayer 
{
	GameObject m_prefab;
	GameObject[,] m_tileMatrix;
	float m_pivotAdjustmentY = 0;
	float m_prefabSize = 6;

	const int max_items = 100;

	public TileLayerTrees(GameObject prefab)
	{
		m_prefab = prefab;
		PivotAdjustment pa = m_prefab.GetComponent<PivotAdjustment>();
		if (pa != null)
			m_pivotAdjustmentY = pa.adjustY;
	}

	public void initTileLayer(TileEngine engine)
	{
		int tileCount = engine.tileCount();
		m_tileMatrix = new GameObject[tileCount, tileCount];
		GameObject goTileLayer = new GameObject(m_prefab.name + "Layer");
		goTileLayer.transform.SetParent(engine.parentTransform(), false);

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject goTile = new GameObject();
				goTile.name = "Tile " + x + ", " + z;
				goTile.transform.parent = goTileLayer.transform;
				m_tileMatrix[x, z] = goTile;
				VoxelObject vo = goTile.AddComponent<VoxelObject>();
				vo.index = VoxelObject.indexToString(VoxelObject.kIndexTopLevel);
				initVoxelObjects(goTile, engine.tileWorldSize());
			}
		}
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject goTile = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			goTile.transform.position = desc.worldPos;
			moveVoxelObjects(goTile, desc.tileWorldSize);
			goTile.GetComponent<VoxelObject>().reconstructGameObject();
		}
	}

	private void initVoxelObjects(GameObject goTile, float tileWorldSize)
	{
		// TODO: create a bunch of voxel objects based on noise

		// Hide prefab so we don't create the voxel objects upon construction
		m_prefab.SetActive(false);

		int objectsPerRow = 4;//(int)(tileWorldSize / m_prefabSize);
		int objectCount = objectsPerRow * objectsPerRow;

		for (int i = 0; i < objectCount; ++i) {
			GameObject vo = (GameObject)GameObject.Instantiate(m_prefab, Vector3.zero, Quaternion.identity); 
			vo.transform.parent = goTile.transform;
		}
	}

	private void moveVoxelObjects(GameObject goTile, float tileWorldSize)
	{
		int objectsPerRow = 4;//(int)(tileWorldSize / m_prefabSize);

		for (int z = 0; z < objectsPerRow; ++z) {
			for (int x = 0; x < objectsPerRow; ++x) {
				Transform voTransform = goTile.transform.GetChild((int)(z * objectsPerRow) + x);
				Vector3 localPos = new Vector3(x * m_prefabSize, 0, z * m_prefabSize);
				Vector3 worldPos = voTransform.TransformPoint(localPos);
				worldPos.y = LandscapeConstructor.getGroundHeight(worldPos.x, worldPos.z) + m_pivotAdjustmentY;
				voTransform.position = worldPos;
			}
		}
	}
}
