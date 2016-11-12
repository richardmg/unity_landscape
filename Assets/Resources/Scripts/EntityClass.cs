using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Lod = System.Int32;
using EntityClassID = System.Int32;

public class EntityClass {
	public Dictionary<int, int> indexSubstitutions;
	public GameObject prefab;
	MeshCollider meshCollider;
	public string prefabName;
	public string entityName;
	public int instanceCount;

	// id is set by EntityManager
	public int id = -1;
	public bool removed = false;

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
		prefab = Root.instance.entityClassManager.getEntityPrefab(prefabName);
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

		Root.instance.entityClassManager.addEntityClass(this);

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

		Root.instance.entityClassManager.addEntityClass(this);
	}

	private EntityClass()
	{}

	public void remove()
	{
		Root.instance.entityClassManager.removeEntityClass(this);
	}

	public EntityInstance createInstance(Transform parent = null, string name = "")
	{
		GameObject go = new GameObject(name);
		go.transform.parent = parent;
		Vector3 localScale = m_voxelObjectRoot.transform.localScale;
		localScale.Scale(Root.instance.entityBaseScale);
		go.transform.localScale = localScale;

		EntityInstance instance = go.AddComponent<EntityInstance>();
		instance.entityClass = this;
		instanceCount++;

//		go.SetActive(false);
		instance.makeStandalone(Root.kLod0);

		return instance;
	}

	public int getVertexCount(Lod lod)
	{
		if (lod >= Root.kLodCount)
			return -1;
		Mesh m = m_mesh[lod];
		if (m == null)
			return -1;
		return m.vertexCount;
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
		Debug.Assert(!removed, "This entity class has beed removed from project. The caller has and old reference!");

		if (unmarkDirty(DirtyFlags.Mesh))
			m_mesh = new Mesh[Root.kLodCount];

		Mesh mesh = m_mesh[lod];
		if (mesh == null) {
			mesh = m_voxelObjectRoot.createMesh(lod, indexSubstitutions);
			m_mesh[lod] = mesh;
		}

		return mesh;
	}

	public MeshCollider getMeshCollider()
	{
		// NOTE: this collider needs to be translated to the instance to be usable!
		if (meshCollider == null)
			meshCollider = new MeshCollider();
		meshCollider.sharedMesh = getMesh(Root.kLod0);
		return meshCollider;
	}

	public Texture2D takeSnapshot(SnapshotCamera camera)
	{
		EntityInstance instance = createInstance(null, "SnapshotEntity");
		instance.makeStandalone(Root.kLodLit);
		Texture2D snapshot = camera.takeSnapshot(instance.gameObject, m_voxelObjectRoot.snapshotOffset);
		instance.hideAndDestroy();
		return snapshot;
	}

	public void takeSnapshot(SnapshotCamera camera, Texture2D destTexture, int destX, int destY)
	{
		EntityInstance instance = createInstance(null, "SnapshotEntity");
		instance.makeStandalone(Root.kLodLit);
		camera.takeSnapshot(instance.gameObject, m_voxelObjectRoot.snapshotOffset, destTexture, destX, destY);
		instance.hideAndDestroy();
	}

	public static EntityClass load(ProjectIO projectIO, bool notify = true)
	{
		EntityClass c = new EntityClass();
		c.initFromLoad(projectIO, notify);
		return c;
	}

	void initFromLoad(ProjectIO projectIO, bool notify)
	{
		id = projectIO.readInt();
		prefabName = projectIO.readString();
		entityName = projectIO.readString();

		prefab = Root.instance.entityClassManager.getEntityPrefab(prefabName);
		Debug.Assert(prefab != null, "Could not find prefab: " + prefabName);
		m_voxelObjectRoot = prefab.GetComponent<VoxelObjectRoot>();

		indexSubstitutions = new Dictionary<int, int>();
		int substitutionKeysCount = projectIO.readInt();
		for (int i = 0; i < substitutionKeysCount; ++i) {
			int atlasIndex = projectIO.readInt();
			int substitution = projectIO.readInt();
			indexSubstitutions[atlasIndex] = substitution;
		}

		Root.instance.entityClassManager.addEntityClass(this, id, notify);
	}

	public void save(ProjectIO projectIO)
	{
		projectIO.writeInt(id);
		projectIO.writeString(prefabName);
		projectIO.writeString(entityName);

		var atlasIndexList = indexSubstitutions.Keys;
		projectIO.writeInt(atlasIndexList.Count);
		foreach (int atlasIndex in atlasIndexList) {
			projectIO.writeInt(atlasIndex);
			projectIO.writeInt(indexSubstitutions[atlasIndex]);
		}
	}
}
