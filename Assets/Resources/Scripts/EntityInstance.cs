using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class EntityInstanceDescription
{
	// EntityInstanceDescription is a shadow structure that doesn't
	// exist in the scene as a GameObject, like an EntityInstance will be.
	// This is more convenient when describing, saving, loading etc
	// entity instances that exists in the world.
	// It is still possible to create entity instance directly from an
	// entity class, but a single entity instance description can
	// only be used to create one instance.

	public int entityClassID;	
	public Vector3 worldPos;
	public Quaternion rotation;
	public bool isStatic;

	EntityInstance instance;

	public EntityInstanceDescription()
	{}

	public EntityInstanceDescription(EntityClass entityClass, Vector3 worldPos, bool isStatic = true)
	{
		entityClassID = entityClass.id;
		this.worldPos = worldPos;
		rotation = new Quaternion();
		this.isStatic = isStatic;
	}

	public EntityInstance createInstance(Transform parentTransform)
	{
		Debug.Assert(instance == null, "This description has an instance already");
		EntityClass entityClass = Root.instance.entityClassManager.getEntity(entityClassID);
		instance = entityClass.createInstance(parentTransform);
		instance.entityInstanceDescription = this;
		instance.transform.position = worldPos;
		instance.transform.rotation = rotation;
		instance.gameObject.isStatic = isStatic;
		return instance;
	}

	public void destroyInstance()
	{
		if (!instance)
			return;

		instance.hideAndDestroy();
		instance = null;
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
//		GameObject.Destroy(this.gameObject);
		UnityEditor.EditorApplication.delayCall += ()=> { DestroyImmediate(gameObject); };
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
