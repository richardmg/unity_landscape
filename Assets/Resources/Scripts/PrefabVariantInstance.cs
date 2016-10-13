using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class PrefabVariantInstance : MonoBehaviour {
	public PrefabVariant prefabVariant;

	public static GameObject create(PrefabVariant prefabVariant)
	{
		GameObject go = new GameObject();
		PrefabVariantInstance instance = go.AddComponent<PrefabVariantInstance>();
		instance.prefabVariant = prefabVariant;
		return go;
	}

	public static Mesh createCombinedMesh(GameObject root, Lod lod)
	{
		PrefabVariantInstance[] selfAndchildren = root.GetComponentsInChildren<PrefabVariantInstance>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = root.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			PrefabVariantInstance prefabVariantInstance = selfAndchildren[i];
			combine[i].mesh = prefabVariantInstance.prefabVariant.getMesh(lod);
			combine[i].transform = parentTransform * prefabVariantInstance.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}
}
