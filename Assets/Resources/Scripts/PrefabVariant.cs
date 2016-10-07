using UnityEngine;
using System.Collections;

public class PrefabVariant {
	public string prefabName;
	public int[] atlasIndices;

	public PrefabVariant(string prefabName)
	{
		this.prefabName = prefabName;
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
		clone.prefabName = prefabName;
		clone.atlasIndices = atlasIndices;
		return clone;
	}
}
