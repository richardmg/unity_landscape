using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;

public class EntityInstanceDescription
{
	// EntityInstanceDescription is a data structure that describes an
	// entity instance in the world. While the world will be populated with
	// EntityInstanceDescriptions (tracked by EntityInstanceManager), only a
	// subset of them will at any time be realized as visible EntityInstances
	// (normally by a tile layer). And those EntityInstances will live as
	// components of GameObjects.

	public int entityClassID;	
	public Vector3 worldPos;
	public Quaternion rotation;
	public bool isStatic;

	// There is a one-to-one mapping between an EntityInstance and
	// a EntityInstanceDescription for simplicity. This can change if we
	// want to support e.g split-screen multiplayer in the future.
	// Note that 'instance' is owned by the tile layer that creates the
	// instance, and is placed here for easy bookkeeping only.
	public EntityInstance instance;

	public EntityInstanceDescription()
	{}

	~EntityInstanceDescription()
	{
		Root.instance.entityClassManager.getEntity(entityClassID).instanceDescriptionCount--;
	}

	public EntityInstanceDescription(EntityClass entityClass, Vector3 worldPos, bool isStatic = true)
	{
		entityClass.instanceDescriptionCount++;
		entityClassID = entityClass.id;
		this.worldPos = worldPos;
		rotation = new Quaternion();
		this.isStatic = isStatic;
	}
}

public class EntityInstance : MonoBehaviour {
	public EntityClass entityClass;
	public EntityInstanceDescription entityInstanceDescription;
	public bool instanceHidden = false;

	public void changeEntityClass(EntityClass toEntityClass)
	{
		entityClass = toEntityClass;
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

	override public string ToString()
	{
		return "Instance from " +  entityClass.ToString();
	}
}
