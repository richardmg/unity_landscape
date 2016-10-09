using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PrefabVariant {
	public string prefabName;
	public int[] atlasIndices;
	public GameObject prefab;

	public PrefabVariant(string prefabName)
	{
		this.prefabName = prefabName;
		prefab = Root.getPrefab(prefabName);
		List<VoxelObject> unique = getUniqueVoxelObjects();

		MonoBehaviour.print("unique: " + unique.Count);
		// Allocate faces in atlas (does not need to be trailing), and assign to atlasIndices
	}

	PrefabVariant()
	{
	}

	List<VoxelObject> getUniqueVoxelObjects()
	{
		VoxelObject[] voxelObjects = prefab.GetComponentsInChildren<VoxelObject>(true);
		List<VoxelObject> uniqueVoxelObjects = new List<VoxelObject>();

		for (int i = 0; i < voxelObjects.Length; ++i) {
			int atlasIndex = voxelObjects[i].resolvedIndex();
			if (atlasIndex < 0)
				continue;
			
			bool unique = true;
			for (int v = 0; v < uniqueVoxelObjects.Count; ++v) {
				if (uniqueVoxelObjects[v].resolvedIndex() == atlasIndex) {
					unique = false;
					break;
				}
			}
			if (unique)
				uniqueVoxelObjects.Add(voxelObjects[i]);
		}

		if (uniqueVoxelObjects.Count == 0)
			MonoBehaviour.print("Could not find any non-toplevel voxel objects in prefab: " + prefabName);

		return uniqueVoxelObjects;
	}

	public void setIndex(int childIndex, int atlasIndex)
	{
	}

	public PrefabVariant clone()
	{
		PrefabVariant clone = new PrefabVariant();
		clone.prefabName = prefabName;
		clone.atlasIndices = atlasIndices;
		return clone;
	}
}
