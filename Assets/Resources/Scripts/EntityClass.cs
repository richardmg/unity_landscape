using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Lod = System.Int32;
using EntityClassID = System.Int32;

public class EntityClass : IProjectIOMember {
	public Dictionary<int, int> indexSubstitutions;
	public GameObject prefab;
	public string prefabName;
	public string entityName;

	// id is set by EntityManager
	public int id = -1;

	VoxelObjectRoot m_voxelObjectRoot;

	Mesh[] m_mesh = new Mesh[Root.kLodCount];

	DirtyFlags m_dirtyFlags;

	public enum DirtyFlags {
		Mesh = 1
	}

	public EntityClass(string prefabName)
	{
		this.prefabName = prefabName;
		this.entityName = prefabName;
		prefab = Root.instance.getPrefab(prefabName);
		Debug.Assert(prefab != null, "Could not find prefab: " + prefabName);
		m_voxelObjectRoot = prefab.GetComponent<VoxelObjectRoot>();

		// Allocate indices in the TextureAtlas for this prefab variant
		List<VoxelObject> uniqueVoxelObjects = getUniqueVoxelObjects();
		indexSubstitutions = new Dictionary<int, int>();

		for (int i = 0; i < uniqueVoxelObjects.Count; ++i) {
			int baseIndex = uniqueVoxelObjects[i].atlasIndex;
			int newIndex = Root.instance.atlasManager.acquireIndex();
			Root.instance.atlasManager.copySubImageFromProjectToProject(baseIndex, newIndex);
			indexSubstitutions[baseIndex] = newIndex;
		}

		Root.instance.entityManager.addEntityClass(this);

//		List<int> indices = atlasIndexList();
//		Debug.Log("Created new entity class from prefab: " + prefabName + ". Index range: " + indices[0] + " -> " + indices[indices.Count - 1]);
	}

	public EntityClass(EntityClass originalEntityClass)
	{
		this.prefabName = originalEntityClass.prefabName;
		this.entityName = originalEntityClass.entityName + "_clone";
		prefab = originalEntityClass.prefab;
		m_voxelObjectRoot = prefab.GetComponent<VoxelObjectRoot>();

		// Allocate indices in the TextureAtlas for this prefab variant
		List<VoxelObject> uniqueVoxelObjects = getUniqueVoxelObjects();
		indexSubstitutions = new Dictionary<int, int>();

		for (int i = 0; i < uniqueVoxelObjects.Count; ++i) {
			int baseIndex = uniqueVoxelObjects[i].atlasIndex;
			int newIndex = Root.instance.atlasManager.acquireIndex();
			int indexToCopy = originalEntityClass.indexSubstitutions[baseIndex];
			Root.instance.atlasManager.copySubImageFromProjectToProject(indexToCopy, newIndex);
			indexSubstitutions[baseIndex] = newIndex;
		}

		Root.instance.entityManager.addEntityClass(this);
	}

	public EntityClass(bool notify = true)
	{
		Root.instance.entityManager.addEntityClass(this, notify);
	}

	public void remove()
	{
		Root.instance.entityManager.removeEntityClass(this);
	}

	public EntityInstance createInstance(Transform parent = null, string name = "")
	{
		GameObject go = new GameObject(name);
		go.transform.parent = parent;
		go.transform.localScale = Root.instance.entityBaseScale;
		go.SetActive(false);
		EntityInstance instance = go.AddComponent<EntityInstance>();
		instance.entityClass = this;
		return instance;
	}

	public List<int> atlasIndexList()
	{
		var list = new List<int>();
		list.AddRange(indexSubstitutions.Values);
		return list;
	}

	List<VoxelObject> getUniqueVoxelObjects()
	{
		VoxelObject[] voxelObjects = prefab.GetComponentsInChildren<VoxelObject>(true);
		List<VoxelObject> uniqueVoxelObjects = new List<VoxelObject>();

		for (int i = 0; i < voxelObjects.Length; ++i) {
			int atlasIndex = voxelObjects[i].atlasIndex;
			if (atlasIndex < 0)
				continue;
			
			bool unique = true;
			for (int v = 0; v < uniqueVoxelObjects.Count; ++v) {
				if (uniqueVoxelObjects[v].atlasIndex == atlasIndex) {
					unique = false;
					break;
				}
			}
			if (unique)
				uniqueVoxelObjects.Add(voxelObjects[i]);
		}

		if (uniqueVoxelObjects.Count == 0)
			MonoBehaviour.print("Could not find any non-toplevel voxel objects in prefab: " + prefab.name);

		return uniqueVoxelObjects;
	}

	public void markDirty(DirtyFlags flags)
	{
		m_dirtyFlags |= flags;
	}

	public bool unmarkDirty(DirtyFlags flags)
	{
		bool dirty = (m_dirtyFlags & flags) != 0;
		m_dirtyFlags &= ~flags;
		return dirty;
	}

	public Mesh getMesh(Lod lod)
	{
		Mesh mesh = m_mesh[lod];
		if (mesh == null || unmarkDirty(DirtyFlags.Mesh)) {
			mesh = m_voxelObjectRoot.createMesh(lod, indexSubstitutions);
			m_mesh[lod] = mesh;
		}

		return mesh;
	}

	public Texture2D takeSnapshot(SnapshotCamera camera)
	{
		EntityInstance instance = createInstance(null, "SnapshotEntity");
		instance.makeStandalone(Root.kLodLit);
		Texture2D snapshot = camera.takeSnapshot(instance.gameObject);
		instance.hideAndDestroy();
		return snapshot;
	}

	public void takeSnapshot(SnapshotCamera camera, Texture2D destTexture, int destX, int destY)
	{
		EntityInstance instance = createInstance(null, "SnapshotEntity");
		instance.makeStandalone(Root.kLodLit);
		camera.takeSnapshot(instance.gameObject, destTexture, destX, destY);
		instance.hideAndDestroy();
	}

	public void initNewProject()
	{}

	public void load(ProjectIO projectIO)
	{
		Debug.Assert(id == projectIO.readInt());
		prefabName = projectIO.readString();
		entityName = projectIO.readString();

		prefab = Root.instance.getPrefab(prefabName);
		Debug.Assert(prefab != null, "Could not find prefab: " + prefabName);
		m_voxelObjectRoot = prefab.GetComponent<VoxelObjectRoot>();

		// Allocate indices in the TextureAtlas for this prefab variant
		List<VoxelObject> uniqueVoxelObjects = getUniqueVoxelObjects();
		indexSubstitutions = new Dictionary<int, int>();

		for (int i = 0; i < uniqueVoxelObjects.Count; ++i) {
			int baseIndex = uniqueVoxelObjects[i].atlasIndex;
			int newIndex = Root.instance.atlasManager.acquireIndex();
			indexSubstitutions[baseIndex] = newIndex;
		}
	}

	public void save(ProjectIO projectIO)
	{
		projectIO.writeInt(id);
		projectIO.writeString(prefabName);
		projectIO.writeString(entityName);
	}
}
