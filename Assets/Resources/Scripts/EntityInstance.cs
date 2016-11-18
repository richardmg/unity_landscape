using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class EntityInstanceDescription
{
	// EntityInstanceDescription is a shadow structure that doesn't
	// exist in the scene as a GameObject, like an EntityInstance needs
	// to be. This is more convenient when describing, saving, loading etc
	// entity instances that exists in the world.

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
		// It is possible to create entity instance directly from an
		// entity class, but a single entity instance description can
		// only reference one concrete instance.
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
		Debug.Assert(instance != null, "This description has no instance. Create and Destroy calls should be balanced.");
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
		// An entity instance does not include any mesh components by default, as
		// it might just be added to scene as an inactive, hidden game object
		// that will be grouped into a parent game object mesh through the 'createCombinedMesh'
		// function. But if the instance is supposed to dynamic, or otherwise not
		// be a part of a parent mesh, this function can be called to make it a proper game object.
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
		EntityClass.globalInstanceCount--;

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
