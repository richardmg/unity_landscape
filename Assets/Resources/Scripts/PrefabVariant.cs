﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class PrefabVariant {
	public int id { get; private set; }
	public string prefabName;
	public int[] atlasIndices;
	public GameObject prefab;

	Mesh[] m_mesh = new Mesh[2];

	static int nextID = 0;

	public PrefabVariant(string prefabName)
	{
		id = nextID++;
		this.prefabName = prefabName;
		prefab = Root.getPrefab(prefabName);
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

	public GameObject createInstance(Lod lod)
	{
		Mesh mesh = m_mesh[lod];
		if (!mesh) {
			mesh = createMesh(lod);
			m_mesh[lod] = mesh;
		}
			
		GameObject go = new GameObject();
		MeshFilter meshFilter = (MeshFilter)go.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = (MeshRenderer)go.AddComponent<MeshRenderer>();
		meshFilter.sharedMesh = mesh;
		m_meshRenderer.sharedMaterial = (lod == VoxelObject.kLod0) ? materialExact : materialVolume;

		return go;
	}

	Mesh createMesh(Lod lod)
	{
		// Return a mesh that is a combination of this object and all its children

		VoxelObject[] selfAndchildren = prefab.GetComponentsInChildren<VoxelObject>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = prefab.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			VoxelObject vo = selfAndchildren[i];
			vo.setLod(lod);
			combine[i].mesh = vo.createMeshNonRecursive(lod);
			combine[i].transform = parentTransform * vo.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);
		return topLevelMesh;
	}

	public PrefabVariant clone()
	{
		PrefabVariant clone = new PrefabVariant();
		clone.prefabName = prefabName;
		clone.atlasIndices = atlasIndices;
		return clone;
	}
}
