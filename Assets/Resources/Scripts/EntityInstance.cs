using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class EntityInstanceDescription
{
	// A more lightweight structure than EntityInstance for easy storage

	public int entityClassID;	
	public Vector3 position;
	public Quaternion rotation;

	public EntityInstanceDescription()
	{}

	public EntityInstanceDescription(EntityInstance instance)
	{
		entityClassID = instance.entityClass.id;
		position = instance.transform.position;
		rotation = instance.transform.rotation;
	}

	public EntityInstance createInstance(Transform parentTransform)
	{
		EntityClass entityClass = Root.instance.entityClassManager.getEntity(entityClassID);
		EntityInstance entityInstance = entityClass.createInstance(parentTransform);
		entityInstance.transform.position = position;
		entityInstance.transform.rotation = rotation;
		return entityInstance;
	}
}

public class EntityInstance : MonoBehaviour {
	public EntityClass entityClass;
	public EntityInstanceDescription entityInstanceDescription;

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
		entityClass.instanceCount--;
		gameObject.SetActive(false);
		GameObject.Destroy(this.gameObject);
	}

	public void updateMesh()
	{
		MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
		MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
		meshFilter.sharedMesh = entityClass.getMesh(Root.kLod0);
		meshCollider.sharedMesh = meshFilter.sharedMesh;
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
