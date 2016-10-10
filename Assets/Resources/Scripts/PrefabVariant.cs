using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Lod = System.Int32;
using PrefabVariantID = System.Int32;

public class PrefabVariant : MonoBehaviour {
	public PrefabVariantID id { get; private set; }
	public string prefabName;
	public int[] atlasIndices;
	public GameObject prefab;

	public PrefabVariant() {}

	public void setPrefab(string prefabName)
	{
		Debug.Assert(prefab == null, "Don't call setPrefab twize!");

		// TODO: Since I never assign a prefab name to a voxel object index, they
		// will never be of type kPrefab, and hence, never be cached. But a better
		// idea all together is to not rely on the prefab cache at at all, but instead
		// let all gameobjects that share PrefabVariant actually share it (rather than
		// each creating their own instance, like now). This means that I need one extra
		// level of indirection; tile -> gameobject -> PrefabVariantPointer -> prefabVariant -> voxelobject.
		// then prefabvariant can go back to be a normal object instead of MonoBehaviour as well.
		// A prefabvariant will then cache the mesh making voxel object cache superfluos (which
		// it will be anyway, since voxel objects will be configured)
		this.prefabName = prefabName;
		prefab = Root.getPrefab(prefabName);

		// Allocate indices in the TextureAtlas for this prefab variant
		List<VoxelObject> uniqueVoxelObjects = getUniqueVoxelObjects();
		atlasIndices = new int[uniqueVoxelObjects.Count];
		for (int i = 0; i < uniqueVoxelObjects.Count; ++i)
			atlasIndices[i] = Root.instance.atlasManager.acquireIndex();
	}

	public List<VoxelObject> getUniqueVoxelObjects()
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

	public Mesh createMesh(Lod lod)
	{
		return prefab.GetComponent<VoxelObject>().createMesh(lod);
	}
}
