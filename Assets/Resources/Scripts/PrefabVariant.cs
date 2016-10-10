using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Lod = System.Int32;
using PrefabVariantID = System.Int32;

public class PrefabVariant {
	public PrefabVariantID id { get; private set; }
	public string prefabName;
	public int[] atlasIndices;
	public GameObject prefab;

	static PrefabVariantID nextID = 0;
	static bool staticResourcesInitialized = false;
	static public Material materialExact;
	static public Material materialVolume;

	Mesh[] m_mesh = new Mesh[Root.kLodCount];

	public PrefabVariant(string prefabName)
	{
		if (!staticResourcesInitialized)
			initStaticResources();

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

	public GameObject createInstance()
	{
		// TODO: which lod to actually use run-time will change for
		// the GameObject we create. So we need to inject a script into it
		// that can ask this PrefabVariant for e.g getMesh(Lod lod). Once
		// this script is ready, perhaps most of the code in this function
		// can be refactored into it (like creating MeshFilter etc). It
		// will basically look a lot like VoxelObject.
		Lod lod = Root.kLod0;

		Mesh mesh = m_mesh[lod];
		if (!mesh) {
			Dictionary<int, int> atlasIndexSubstitutions = new Dictionary<int, int>();
			mesh = MeshManager.createCombinedMesh(prefab, Root.kLod0, atlasIndexSubstitutions);
			m_mesh[lod] = mesh;
		}
			
		GameObject go = new GameObject();
		MeshFilter meshFilter = (MeshFilter)go.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = (MeshRenderer)go.AddComponent<MeshRenderer>();
		meshFilter.sharedMesh = mesh;
		meshRenderer.sharedMaterial = (lod == Root.kLod0) ? materialExact : materialVolume;

		return go;
	}

	public PrefabVariant copy()
	{
		PrefabVariant clone = new PrefabVariant(prefabName);
		return clone;
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
