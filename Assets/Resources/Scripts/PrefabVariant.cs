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

	static PrefabVariantID nextID = 0;
	static bool staticResourcesInitialized = false;
	static public Material materialExact;
	static public Material materialVolume;

	Mesh[] m_mesh = new Mesh[Root.kLodCount];

	public PrefabVariant() {}

	public void setPrefab(string prefabName)
	{
		if (!staticResourcesInitialized)
			initStaticResources();

		Debug.Assert(prefab == null, "Don't call setPrefab twize!");

		id = nextID++;
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

	public void initStaticResources()
	{
		materialExact = (Material)Resources.Load("Materials/VoxelObjectExact", typeof(Material));
		materialVolume = (Material)Resources.Load("Materials/VoxelObjectVolume", typeof(Material));

		Debug.Assert(materialExact != null);
		Debug.Assert(materialVolume != null);
		Debug.Assert(materialExact.mainTexture != null);
		Debug.Assert(materialVolume.mainTexture != null);

		materialVolume.CopyPropertiesFromMaterial(materialExact);
		staticResourcesInitialized = true;
	}
}
