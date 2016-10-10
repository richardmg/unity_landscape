using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class MeshManager {

	Hashtable m_hashTable = new Hashtable();
	static VoxelMeshFactory voxelMeshFactory = new VoxelMeshFactory();

    const int maxLod = 1;

	public MeshManager()
	{
	}

	string getCacheID(PrefabVariant prefabVariant, Lod lod)
    {
		// TODO: fix name
		string name = prefabVariant.prefabName;
		return name.ToLower() + (lod == Root.kLod0 ? "0" : "1");
    }

	public Mesh getSharedMesh(PrefabVariant prefabVariant, Lod lod)
	{
		Debug.Assert(false, "Needs to change!");
		return null;

//        Debug.Assert(lod <= maxLod);
//
//		// TODO: fix name
//		string name = prefabVariant.prefabName;
//
//		string cacheId = getCacheID(prefabVariant, lod);
//		Mesh mesh = (Mesh)m_hashTable[cacheId];
//
//		if (mesh == null) {
//			GameObject prefab = Root.getPrefab(name);
//			if (prefab == null)
//				return null;
//
//			VoxelObject vo = prefab.GetComponent<VoxelObject>();
//			mesh = vo.createMesh(lod);
//			m_hashTable[cacheId] = mesh;
//
////			if (Application.isPlaying) {
////				MonoBehaviour.print("Caching " + cacheId);
////				foreach(string key in m_hashTable.Keys)
////					MonoBehaviour.print("   " + key);
////			}
//
//			if (m_hashTable.Count > 10) {
//				// Reminder for later...
//				MonoBehaviour.print("REMEMBER TO CLEAR CACHE FOR UNUSED VOXELOBJECTS! Cache size: " + m_hashTable.Count);
//			}
//		}
//
//		return mesh;
	}

	public int size()
	{
		return m_hashTable.Count;
	}

	public void clearCache(PrefabVariant prefabVariant)
    {
        for (int i = 0; i <= maxLod; ++i)
			m_hashTable.Remove(getCacheID(prefabVariant, i));
    }

	public void clearCache()
	{
		m_hashTable.Clear();
	}

	public static Mesh createCombinedMesh(GameObject root, Lod lod, Dictionary<int, int> atlasIndexSubstitutions)
	{
		MeshFilter[] selfAndchildren = root.GetComponentsInChildren<MeshFilter>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = root.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			MeshFilter meshFilter = selfAndchildren[i];
			combine[i].mesh = meshFilter.sharedMesh;
			combine[i].transform = parentTransform * meshFilter.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}

	public static Mesh createMeshFromAtlasIndex(int atlasIndex, Lod lod, float voxelDepth)
	{
		voxelMeshFactory.atlasIndex = atlasIndex;
		voxelMeshFactory.voxelDepth = voxelDepth;
		voxelMeshFactory.xFaces = voxelDepth != 0;
		voxelMeshFactory.yFaces = voxelDepth != 0;

		switch (lod) {
		case Root.kLod0:
			voxelMeshFactory.useVolume = false;
			voxelMeshFactory.simplify = false;
			break;
		case Root.kLod1:
			voxelMeshFactory.useVolume = true;
			voxelMeshFactory.simplify = true;
			break;
		}

		return voxelMeshFactory.createMesh();
	}
}
