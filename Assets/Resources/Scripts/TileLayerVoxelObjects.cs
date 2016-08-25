using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TileLayerVoxelObjects : MonoBehaviour, ITileLayer 
{
	[Range (1, 100)]
	public int objectCount = 4;
	[Range (0, 100)]
	public float paddingBetweenObjects = 25;
	public GameObject prefab;

	GameObject[,] m_tileMatrix;
	float m_pivotAdjustmentY = 0;

	public void initTileLayer(TileEngine engine)
	{
		int tileCount = engine.tileCount;
		m_tileMatrix = new GameObject[tileCount, tileCount];

		for (int z = 0; z < tileCount; ++z) {
			for (int x = 0; x < tileCount; ++x) {
				GameObject goTile = new GameObject();
				goTile.name = "Tile " + x + ", " + z;
				goTile.transform.parent = transform;
				m_tileMatrix[x, z] = goTile;

				VoxelObject vo = goTile.AddComponent<VoxelObject>();
				vo.setIndex(VoxelObject.indexToString(VoxelObject.kIndexTopLevel));
				vo.initAsStandAlone();
				initVoxelObjects(goTile, engine.tileSize);
			}
		}

		PivotAdjustment pa = prefab.GetComponent<PivotAdjustment>();
		if (pa != null)
			m_pivotAdjustmentY = pa.adjustY;
	}

	public void moveTiles(TileDescription[] tilesToMove)
	{
		for (int i = 0; i < tilesToMove.Length; ++i) {
			TileDescription desc = tilesToMove[i];
			GameObject goTile = m_tileMatrix[(int)desc.matrixCoord.x, (int)desc.matrixCoord.y];
			goTile.transform.position = desc.worldPos;
			moveVoxelObjects(goTile, desc.tileWorldSize);
			goTile.GetComponent<VoxelObject>().rebuildStandAlone();
		}
	}

	public void removeAllTiles()
	{
		if (m_tileMatrix == null)
			return;

		int countX = m_tileMatrix.GetLength(0);
		int countZ = m_tileMatrix.GetLength(1);
		for (int z = 0; z < countZ; ++z) {
			for (int x = 0; x < countX; ++x)
				DestroyImmediate(m_tileMatrix[x, z]);
		}
	}

	private void initVoxelObjects(GameObject goTile, float tileWorldSize)
	{
		// TODO: create a bunch of voxel objects based on noise

		// Hide prefab so we don't create the voxel objects upon construction
		prefab.SetActive(false);

		int count = objectCount * objectCount;

		for (int i = 0; i < count; ++i) {
			GameObject go = new GameObject();
			go.transform.parent = goTile.transform;
			VoxelObject vo = go.AddComponent<VoxelObject>();
			vo.setIndex(prefab.name);
			vo.transform.localScale = prefab.transform.localScale;
			vo.gameObject.SetActive(false);
		}
	}

	private void moveVoxelObjects(GameObject goTile, float tileWorldSize)
	{
		for (int z = 0; z < objectCount; ++z) {
			for (int x = 0; x < objectCount; ++x) {
				Vector3 worldPos = goTile.transform.position + new Vector3(x * paddingBetweenObjects, 0, z * paddingBetweenObjects);
				worldPos.y = LandscapeConstructor.getGroundHeight(worldPos.x, worldPos.z) + m_pivotAdjustmentY;
				Transform voTransform = goTile.transform.GetChild((int)(z * objectCount) + x);
				voTransform.position = worldPos;
			}
		}
	}
}
