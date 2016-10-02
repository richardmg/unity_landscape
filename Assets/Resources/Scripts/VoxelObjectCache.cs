using UnityEngine;
using System.Collections;
using Lod = System.Int32;

public class VoxelObjectCache {

	static VoxelObjectCache s_instance;
	Hashtable m_hashTable = new Hashtable();

    const int maxLod = 1;

	private VoxelObjectCache()
	{
		s_instance = null;
	}

	static public VoxelObjectCache instance()
	{
		if (s_instance == null)
			s_instance = new VoxelObjectCache();
		return s_instance;
	}

	string getCacheID(string name, Lod lod)
    {
		return name.ToLower() + (lod == VoxelObject.kLod0 ? "0" : "1");
    }

	public Mesh getSharedMesh(string name, Lod lod)
	{
        Debug.Assert(lod <= maxLod);

		string cacheId = getCacheID(name, lod);
		Mesh mesh = (Mesh)m_hashTable[cacheId];

		if (mesh == null) {
			GameObject prefab = Global.getPrefab(name);
			if (prefab == null)
				return null;

			VoxelObject vo = prefab.GetComponent<VoxelObject>();
			mesh = vo.createTopLevelMesh(lod);
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
		}

		return mesh;
	}

	public int size()
	{
		return m_hashTable.Count;
	}

	public void clearCache(string name)
    {
        for (int i = 0; i <= maxLod; ++i)
		    m_hashTable.Remove(getCacheID(name, i));
    }

	public void clearCache()
	{
		m_hashTable.Clear();
	}
}
