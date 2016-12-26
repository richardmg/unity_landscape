using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using Lod = System.Int32;
using EntityClassID = System.Int32;

public class EntityClass {
	MeshCollider meshCollider;
	public string entityName;

	// instanceCount is the number of EntityInstanceDescriptions using
	// this EntityClass, not the number of EntityInstances in the scene.
	public int instanceDescriptionCount;

	// id is set by EntityManager
	public int id = -1;
	public bool removed = false;

	VoxelObjectRoot m_voxelObjectRoot;

	Mesh[] m_mesh = new Mesh[Root.kLodCount];

	DirtyFlags m_dirtyFlags;

	public enum DirtyFlags {
		Mesh = 1
	}

	public EntityClass(string name)
	{
		this.entityName = name;
		Root.instance.entityClassManager.addEntityClass(this);
	}

	public EntityClass(EntityClass originalEntityClass)
	{
		Debug.Log("not supported. Need to copy all children voxel objects");
		this.entityName = originalEntityClass.entityName + "_clone";
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
		go.transform.localPosition = Vector3.zero;

		EntityInstance instance = go.AddComponent<EntityInstance>();
		instance.entityClass = this;

		// Disable it as it will either be a part of a combined mesh, or
		// made stand-alone explicit
		go.SetActive(false);

		return instance;
	}

	public void setVoxelObjectRoot(VoxelObjectRoot root)
	{
		m_voxelObjectRoot = root;
		markDirty(DirtyFlags.Mesh);
	}

	public VoxelObjectRoot getVoxelObjectRoot()
	{
		return m_voxelObjectRoot;
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

	List<VoxelObject> getUniqueVoxelObjects()
	{
		VoxelObject[] voxelObjects = m_voxelObjectRoot.GetComponentsInChildren<VoxelObject>(true);
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
			mesh = m_voxelObjectRoot.createMesh(lod);
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
		instance.gameObject.hideAndDestroy();
		return snapshot;
	}

	public void takeSnapshot(SnapshotCamera camera, Texture2D destTexture, int destX, int destY)
	{
		EntityInstance instance = createInstance(null, "SnapshotEntity");
		instance.makeStandalone(Root.kLodLit);
		camera.takeSnapshot(instance.gameObject, m_voxelObjectRoot.snapshotOffset, destTexture, destX, destY);
		instance.gameObject.hideAndDestroy();
	}

	public static EntityClass load(ProjectIO projectIO, bool notify = true)
	{
		EntityClass c = new EntityClass();
		c.initFromLoad(projectIO, notify);
		return c;
	}

	void initFromLoad(ProjectIO projectIO, bool notify)
	{
		Debug.Log("Load entity class not implemented");

		id = projectIO.readInt();
		entityName = projectIO.readString();
		instanceDescriptionCount = projectIO.readInt();

		m_voxelObjectRoot = null;

		Root.instance.entityClassManager.addEntityClass(this, id, notify);
	}

	public void save(ProjectIO projectIO)
	{
		Debug.Log("Save entity class not implemented");

		projectIO.writeInt(id);
		projectIO.writeString(entityName);
	}

	override public string ToString()
	{
		return entityName + " (ID: " + id + ")";
	}
}
