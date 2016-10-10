using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class MeshManager {

	Hashtable m_hashTable = new Hashtable();

	public MeshManager()
	{
	}

	string getCacheID(string name, Lod lod)
    {
		return name.ToLower() + (lod == Root.kLod0 ? "0" : "1");
    }

	public Mesh getSharedMesh(string name, Lod lod)
	{
		Debug.Assert(lod < Root.kLodCount);

		string cacheId = getCacheID(name, lod);
		Mesh mesh = (Mesh)m_hashTable[cacheId];

		if (mesh == null) {
			MonoBehaviour.print("create in cache: " + cacheId);
			GameObject prefab = Root.getPrefab(name);
			if (prefab == null)
				return null;

			VoxelObject vo = prefab.GetComponent<VoxelObject>();
			mesh = vo.createMesh(lod);
			m_hashTable[cacheId] = mesh;

//			if (Application.isPlaying) {
//				MonoBehaviour.print("Caching " + cacheId);
//				foreach(string key in m_hashTable.Keys)
//					MonoBehaviour.print("   " + key);
//			}

			if (m_hashTable.Count > 10) {
				// Reminder for later...
				MonoBehaviour.print("REMEMBER TO CLEAR CACHE FOR UNUSED VOXELOBJECTS! Cache size: " + m_hashTable.Count);
			}
		} else {
			MonoBehaviour.print("Found in cache: " + cacheId);
		}

		return mesh;
	}

	public int size()
	{
		return m_hashTable.Count;
	}

	public void clearCache(PrefabVariant prefabVariant)
    {
		for (int i = 0; i < Root.kLodCount; ++i)
			m_hashTable.Remove(getCacheID(prefabVariant.prefabName, i));
    }

	public void clearCache()
	{
		m_hashTable.Clear();
	}

	public static Mesh createCombinedMesh(GameObject root, Lod lod, Dictionary<int, int> atlasIndexSubstitutions)
	{
		PrefabVariant[] selfAndchildren = root.GetComponentsInChildren<PrefabVariant>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = root.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			PrefabVariant prefabVariant = selfAndchildren[i];
			combine[i].mesh = prefabVariant.createMesh(lod);
			combine[i].transform = parentTransform * prefabVariant.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}
}
