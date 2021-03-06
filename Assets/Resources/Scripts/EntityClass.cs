﻿using UnityEngine;
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

	[FlagsAttribute] public enum DirtyFlags {
		Mesh = 1
	}

	public EntityClass()
	{
	}

	public EntityClass(string name = "", int id = -1, bool notify = true)
	{
		// 'id' will be overwritten by entityClassManager if it's -1
		this.id = id;
		entityName = name != "" ? name : "Unnamed EntityClass";
		m_voxelObjectRoot = new VoxelObjectRoot();
		Root.instance.entityClassManager.addEntityClass(this, notify);
	}

	public void remove()
	{
		Root.instance.entityClassManager.removeEntityClass(this);
	}

	public GameObject createGameObject(Transform parent, Lod lod, string name = "EntityInstance")
	{
		// Create a gameobject with a EntityInstance component, and with a VoxelObjectRoot as
		// the only child. Then, for now, add one game object per voxel object under the root.
		GameObject entityInstance = new GameObject(name);
		entityInstance.transform.SetParent(parent, false);
		Vector3 worldScale = Vector3.one;
		//worldScale.Scale(localScale);
		worldScale.Scale(Root.instance.alignmentManager.voxelSize);
		entityInstance.transform.localScale = worldScale;

		GameObject rootGo = m_voxelObjectRoot.createGameObject(lod, "VoxelObjectRoot");
		rootGo.transform.SetParent(entityInstance.transform, false);

		EntityInstance entityInstanceMonoBehaviour = entityInstance.AddComponent<EntityInstance>();
		entityInstanceMonoBehaviour.entityClass = this;
		return entityInstance;
	}

	public GameObject createGameObject(Transform parent, EntityInstanceDescription desc, Lod lod, string name = "EntityInstance")
	{
		Debug.Assert(desc.instance == null, "This description already carries an instance");

		GameObject go = createGameObject(parent, lod, name);
		go.transform.position = desc.worldPos;
		go.transform.setVoxelRotation(desc.voxelRotation);
		go.isStatic = desc.isStatic;

		desc.instance = go.GetComponent<EntityInstance>();
		desc.instance.entityInstanceDescription = desc;
		return go;
	}

	public VoxelObjectRoot voxelObjectRoot
	{
		get { return m_voxelObjectRoot; }
		set {
			Debug.Assert(value != null);
			m_voxelObjectRoot = value;
			markDirty(DirtyFlags.Mesh);
		}
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
		Debug.Assert(!removed, "This entity class has been removed from project. The caller has and old reference!");

		if (unmarkDirty(DirtyFlags.Mesh))
			m_mesh = new Mesh[Root.kLodCount];

		Mesh mesh = m_mesh[lod];
		if (mesh == null) {
			// todo; optimize this part, if possible
			GameObject go = m_voxelObjectRoot.createCombinedGameObject(Root.kLod0);
			m_mesh[lod] = go.GetComponent<MeshFilter>().sharedMesh;
			go.hideAndDestroy();
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
        m_voxelObjectRoot.add(new VoxelObject(0, 1f));
        Debug.Log("Load entity class not implemented");
	}

	public void save(ProjectIO projectIO)
	{
		Debug.Log("Save entity class not implemented");

		projectIO.writeInt(id);
		projectIO.writeString(entityName);
	}

	public EntityClass createDeepClone(bool notify = true)
	{
		EntityClass clone = new EntityClass();
		clone.entityName = entityName + "_clone";
		clone.voxelObjectRoot = m_voxelObjectRoot.createDeepClone();
		Root.instance.entityClassManager.addEntityClass(clone, notify);
		return clone;
	}

	override public string ToString()
	{
		return entityName + " (ID: " + id + ")";
	}

	public static bool operator !(EntityClass entityClass)
	{
		return entityClass == null;
	}
}
