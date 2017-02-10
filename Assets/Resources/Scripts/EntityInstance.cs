using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Lod = System.Int32;
using VoxelRotation = UnityEngine.Vector3;

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

	// Steps to apply voxelRotation:
	// transform.rotation = Quaternion.Euler(0, 0, 0);
	// transform.Rotate(voxelRotation.x, 0, 0, Space.Self);
	// transform.Rotate(0, voxelRotation.y, 0, Space.World);
	// transform.Rotate(0, voxelRotation.y, 0, Space.Self);
	public VoxelRotation voxelRotation;

	public bool isStatic;

	private static int s_debugId = 0;
	private int debugId;

	// There is a one-to-one mapping between an EntityInstance and
	// a EntityInstanceDescription for simplicity. This can change if we
	// want to support e.g split-screen multiplayer in the future.
	// Note that 'instance' is owned by the tile layer that creates the
	// instance, and is placed here for easy bookkeeping only.
	public EntityInstance instance;

	public EntityInstanceDescription()
	{
		debugId = s_debugId++;
	}

	~EntityInstanceDescription()
	{
		Root.instance.entityClassManager.getEntity(entityClassID).instanceDescriptionCount--;
	}

	public EntityInstanceDescription(EntityClass entityClass, Vector3 worldPos, Vector3 rotation, bool isStatic = true)
	{
		debugId = s_debugId++;
		entityClass.instanceDescriptionCount++;
		entityClassID = entityClass.id;
		this.worldPos = worldPos;
		voxelRotation = rotation;
		this.isStatic = isStatic;
	}

	override public string ToString()
	{
		return "EntityInstanceDescription " + debugId + " (" + (instance ? instance.ToString() : "null") + ")";
	}
}

public class EntityInstance : MonoBehaviour {
	[HideInInspector] public EntityClass entityClass;
	[HideInInspector] public EntityInstanceDescription entityInstanceDescription;
	[HideInInspector] public bool instanceHidden = false;

	// Using a combined mesh is treated per instance, and should
	// probably be the case for instances that are not selected
	// but still close to the camera. Instances far away should
	// not be realized as EntityInstances as all, but instead
	// be a part of a tiles combined mesh.
	// todo: Currently this is not in use
	public bool hasCombinedMesh = false;

	public void syncTransformWithDescription()
	{
		transform.position = entityInstanceDescription.worldPos;
		transform.setVoxelRotation(entityInstanceDescription.voxelRotation);
	}

	public void updateMesh()
	{
		if (hasCombinedMesh) {
			MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
			MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
			meshFilter.sharedMesh = entityClass.getMesh(Root.kLod0);
			meshCollider.sharedMesh = meshFilter.sharedMesh;
		} else {
			// Slow path
			Debug.Log("taking slow path");
			GetComponentInChildren<VoxelObjectRootMonoBehaviour>().transform.gameObject.hideAndDestroy();
			entityClass.voxelObjectRoot.createGameObject(transform, Root.kLod0, "VoxelObjectRoot");
		}
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

	public static bool operator !(EntityInstance entityInstance)
	{
		return entityInstance == null;
	}
}
