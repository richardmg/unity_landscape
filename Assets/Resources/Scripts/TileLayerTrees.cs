using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerTrees : ITileLayer 
{
	GameObject m_prefab;
	GameObject[,] m_tileMatrix;
	float m_pivotAdjustmentY = 0;

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
				vo.atlasIndex = VoxelObject.kTopLevel;
				initVoxelObjects(goTile);
				vo.rebuild();
			}
		}
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject goTile = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			moveVoxelObjects(goTile, desc);
		}
	}

	private void initVoxelObjects(GameObject goTile)
	{
		// TODO: create a bunch of voxel objects based on noise

		// Hide prefab so we don't create the voxel objects upon construction
		m_prefab.SetActive(false);
		GameObject vo = (GameObject)GameObject.Instantiate(m_prefab, Vector3.zero, Quaternion.identity); 
		vo.transform.parent = goTile.transform;
	}

	private void moveVoxelObjects(GameObject goTile, TileDescription desc)
	{
		Vector3 worldPos = desc.worldPos;
		worldPos.y = LandscapeConstructor.getGroundHeight(worldPos.x, worldPos.z) + m_pivotAdjustmentY;
		goTile.transform.localPosition = worldPos;
	}
}
