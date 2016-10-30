using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class EntityInstance : MonoBehaviour {
	public EntityClass entityClass;
	public bool instanceHidden = false;

	public void makeStandalone(Lod lod)
	{
		gameObject.SetActive(true);
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		if (!meshFilter)
			meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = entityClass.getMesh(lod);
		MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
		if (!meshRenderer)
			meshRenderer = gameObject.AddComponent<MeshRenderer>();
		MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
		if (!meshCollider)
			meshCollider = gameObject.AddComponent<MeshCollider>();
		meshCollider.sharedMesh = meshFilter.sharedMesh;

		meshRenderer.sharedMaterial = Root.instance.voxelMaterialForLod(lod);
	}

	public void hideAndDestroy()
	{
		gameObject.SetActive(false);
		GameObject.Destroy(this.gameObject);
	}

	public static Mesh createCombinedMesh(GameObject root, Lod lod)
	{
		EntityInstance[] selfAndchildren = root.GetComponentsInChildren<EntityInstance>(true);
		CombineInstance[] combine = new CombineInstance[selfAndchildren.Length];
		Matrix4x4 parentTransform = root.transform.worldToLocalMatrix;

		for (int i = 0; i < selfAndchildren.Length; ++i) {
			EntityInstance entityClassInstance = selfAndchildren[i];
			EntityClass entityClass = entityClassInstance.entityClass;
			combine[i].mesh = entityClassInstance.instanceHidden ? new Mesh() : entityClass.getMesh(lod);
			combine[i].transform = parentTransform * entityClassInstance.transform.localToWorldMatrix;
		}

		Mesh topLevelMesh = new Mesh();
		topLevelMesh.CombineMeshes(combine);

		return topLevelMesh;
	}
}
