﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Lod = System.Int32;
using EntityClassID = System.Int32;

public class EntityClass {
	public EntityClassID id { get; private set; }
	public string prefabName;
	public int[] atlasIndices;
	public GameObject prefab;

	Mesh[] m_mesh = new Mesh[Root.kLodCount];

	DirtyFlags m_dirtyFlags;

	public enum DirtyFlags {
		Mesh = 1
	}

	public EntityClass(string prefabName)
	{
		this.prefabName = prefabName;
		prefab = Root.getPrefab(prefabName);
		Debug.Assert(prefab != null, "Could not find prefab: " + prefabName);

		// Allocate indices in the TextureAtlas for this prefab variant
		List<VoxelObject> uniqueVoxelObjects = getUniqueVoxelObjects();
		atlasIndices = new int[uniqueVoxelObjects.Count];
		for (int i = 0; i < uniqueVoxelObjects.Count; ++i)
			atlasIndices[i] = Root.instance.atlasManager.acquireIndex();
	}

	public GameObject createInstance(string name = "")
	{
		GameObject go = new GameObject(name);
		EntityInstance instance = go.AddComponent<EntityInstance>();
		instance.entityClass = this;
		return go;
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

	public void markDirty(DirtyFlags flags)
	{
		m_dirtyFlags &= flags;
	}

	public bool unmarkDirty(DirtyFlags flags)
	{
		bool dirty = (m_dirtyFlags & flags) != 0;
		m_dirtyFlags = m_dirtyFlags & ~flags;
		return dirty;
	}

	public Mesh getMesh(Lod lod)
	{
		Mesh mesh = m_mesh[lod];
		if (mesh == null || unmarkDirty(DirtyFlags.Mesh)) {
			mesh = prefab.GetComponent<VoxelObject>().createMesh(lod);
			m_mesh[lod] = mesh;
		}

		return mesh;
	}
}