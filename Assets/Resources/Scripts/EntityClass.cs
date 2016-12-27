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

	public EntityClass(string name = "", int id = -1, bool notify = true)
	{
		entityName = name != "" ? name : "EntityClass " + id;
		m_voxelObjectRoot = new VoxelObjectRoot();
		Root.instance.entityClassManager.addEntityClass(this, notify);
	}

	public EntityClass(EntityClass originalEntityClass)
	{
		Debug.Log("not supported. Need to copy all children voxel objects");
		this.entityName = originalEntityClass.entityName + "_clone";
		m_voxelObjectRoot = new VoxelObjectRoot();
		Root.instance.entityClassManager.addEntityClass(this);
	}

	public void remove()
	{
		Root.instance.entityClassManager.removeEntityClass(this);
	}

	public GameObject createGameObject(Transform parent, Lod lod, string name = "")
	{
		GameObject go = m_voxelObjectRoot.createGameObject(parent, lod);
		Vector3 localScale = go.transform.localScale;
		localScale.Scale(Root.instance.entityBaseScale);
		go.transform.localScale = localScale;

		EntityInstance instance = go.AddComponent<EntityInstance>();
		instance.entityClass = this;
		return go;
	}

	public GameObject createGameObject(Transform parent, EntityInstanceDescription desc, Lod lod, string name = "")
	{
		GameObject go = createGameObject(parent, lod, name);
		go.transform.position = desc.worldPos;
		go.transform.rotation = desc.rotation;
		go.isStatic = desc.isStatic;

		EntityInstance instance = go.GetComponent<EntityInstance>();
		instance.entityInstanceDescription = desc;
		return go;
	}

	public void setVoxelObjectRoot(VoxelObjectRoot root)
	{
		Debug.Assert(root != null);
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

		Debug.Assert(false, "This needs rework!");
		return new Mesh();

//		if (unmarkDirty(DirtyFlags.Mesh))
//			m_mesh = new Mesh[Root.kLodCount];
//
//		Mesh mesh = m_mesh[lod];
//		if (mesh == null) {
//			mesh = m_voxelObjectRoot.createCombinedMesh(lod);
//			m_mesh[lod] = mesh;
//		}
//
//		return mesh;
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
		GameObject go = createGameObject(null, Root.kLodLit, "SnapshotEntity");
		Texture2D snapshot = camera.takeSnapshot(go, m_voxelObjectRoot.snapshotOffset);
		go.hideAndDestroy();
		return snapshot;
	}

	public void takeSnapshot(SnapshotCamera camera, Texture2D destTexture, int destX, int destY)
	{
		GameObject go = createGameObject(null, Root.kLodLit, "SnapshotEntity");
		camera.takeSnapshot(go, m_voxelObjectRoot.snapshotOffset, destTexture, destX, destY);
		go.hideAndDestroy();
	}

	public static EntityClass load(ProjectIO projectIO)
	{
		int id = projectIO.readInt();
		string name = projectIO.readString();
		EntityClass c = new EntityClass(name, id, false);
		c.initFromLoad(projectIO);
		return c;
	}

	void initFromLoad(ProjectIO projectIO)
	{
		Debug.Log("Load entity class not implemented");

		m_voxelObjectRoot = null;
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
