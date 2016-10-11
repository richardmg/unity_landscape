using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class MeshManager {

	Hashtable m_hashTable = new Hashtable();

	public MeshManager()
	{
	}

	public static Mesh createCombinedMesh(GameObject root, Lod lod, Dictionary<int, int> atlasIndexSubstitutions)
	{
		PrefabVariantRef[] selfAndchildren = root.GetComponentsInChildren<PrefabVariantRef>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = root.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			PrefabVariantRef prefabVariantRef = selfAndchildren[i];
			combine[i].mesh = prefabVariantRef.prefabVariant.getMesh(lod);
			combine[i].transform = parentTransform * prefabVariantRef.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}
}
