using UnityEngine;
using System.Collections;

public class PrefabVariant {
	GameObject m_prefab;
	int[] m_atlasIndices;

	public PrefabVariant(string prefabName)
	{
	}

	PrefabVariant()
	{
	}

	public void setIndex(int childIndex, int atlasIndex)
	{
	}

	public PrefabVariant clone()
	{
		PrefabVariant clone = new PrefabVariant();
		clone.m_prefab = m_prefab;
		clone.m_atlasIndices = m_atlasIndices;
		return clone;
	}
}
