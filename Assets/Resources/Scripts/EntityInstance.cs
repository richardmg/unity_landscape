using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class EntityClassInstance : MonoBehaviour {
	public EntityClass entityClass;

	public static GameObject create(EntityClass entityClass)
	{
		GameObject go = new GameObject();
		EntityClassInstance instance = go.AddComponent<EntityClassInstance>();
		instance.entityClass = entityClass;
		return go;
	}

	public static Mesh createCombinedMesh(GameObject root, Lod lod)
	{
		EntityClassInstance[] selfAndchildren = root.GetComponentsInChildren<EntityClassInstance>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = root.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			EntityClassInstance entityClassInstance = selfAndchildren[i];
			combine[i].mesh = entityClassInstance.entityClass.getMesh(lod);
			combine[i].transform = parentTransform * entityClassInstance.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}
}
