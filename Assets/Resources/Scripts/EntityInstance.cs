using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class EntityInstance : MonoBehaviour {
	public EntityClass entityClass;
	public bool instanceHidden = false;

	public void makeStandalone()
	{
		gameObject.SetActive(true);
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = entityClass.getMesh(Root.kLod0);
		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (!meshRenderer)
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = Root.instance.voxelMaterialExact;
	}

	public void hideAndDestroy()
	{
		gameObject.SetActive(false);
		GameObject.Destroy(this);
	}

	public static Mesh createCombinedMesh(GameObject root, Lod lod)
	{
		EntityInstance[] selfAndchildren = root.GetComponentsInChildren<EntityInstance>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = root.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			EntityInstance entityClassInstance = selfAndchildren[i];
			combine[i].mesh = entityClassInstance.instanceHidden ? new Mesh() : entityClassInstance.entityClass.getMesh(lod);
			combine[i].transform = parentTransform * entityClassInstance.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}
}
