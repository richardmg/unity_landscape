﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Lod = System.Int32;
using EntityClassID = System.Int32;

public class EntityClass {
	public Dictionary<int, int> indexSubstitutions;
	public GameObject prefab;
	public string entityName;

	VoxelObjectRoot m_voxelObjectRoot;

	Mesh[] m_mesh = new Mesh[Root.kLodCount];

	DirtyFlags m_dirtyFlags;

	public enum DirtyFlags {
		Mesh = 1
	}

	public EntityClass(string prefabName, bool keepExistingAtlasInidicies = false)
	{
		entityName = prefabName;
		prefab = Root.getPrefab(prefabName);
		Debug.Assert(prefab != null, "Could not find prefab: " + prefabName);
		m_voxelObjectRoot = prefab.GetComponent<VoxelObjectRoot>();

		// Allocate indices in the TextureAtlas for this prefab variant
		List<VoxelObject> uniqueVoxelObjects = getUniqueVoxelObjects();
		indexSubstitutions = new Dictionary<int, int>();

		if (keepExistingAtlasInidicies) {
			for (int i = 0; i < uniqueVoxelObjects.Count; ++i) {
				var atlasIndex = uniqueVoxelObjects[i].atlasIndex;
				indexSubstitutions[atlasIndex] = atlasIndex;
			}
		} else {
			for (int i = 0; i < uniqueVoxelObjects.Count; ++i)
				indexSubstitutions[uniqueVoxelObjects[i].atlasIndex] = Root.instance.atlasManager.acquireIndex();
		}

		Root.instance.entityManager.addEntityClass(this);

//		List<int> indices = atlasIndexList();
//		Debug.Log("Created new entity class from prefab: " + prefabName + ". Index range: " + indices[0] + " -> " + indices[indices.Count - 1]);
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

	public Texture2D takeSnapshot()
	{
		EntityInstance instance = createInstance(null, "SnapshotEntity");
		instance.makeStandalone();
		Texture2D snapshot = Root.instance.snapshotCamera.takeSnapshot(instance.gameObject, new Vector3(0, 0, -10));
		instance.hideAndDestroy();
		return snapshot;
	}

	public void takeSnapshot(Texture2D destTexture, Rect destRect)
	{
		EntityInstance instance = createInstance(null, "SnapshotEntity");
		instance.makeStandalone();
		Root.instance.snapshotCamera.takeSnapshot(instance.gameObject, new Vector3(0, 0, -10), destTexture, destRect);
		instance.hideAndDestroy();
	}
}
