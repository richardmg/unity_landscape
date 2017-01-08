using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerStartupScript : MonoBehaviour, IProjectIOMember {
	public bool moveToGround = true;
	public EntityClass entityClassInUse = null;
	public List<EntityInstance> selectedEntityInstances;

	void Start()
	{
		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}

	public void setEntityClassInUse(EntityClass entityClass)
	{
//		entityClassInUse = entityClass;
	}

	public void initNewProject()
	{
//		entityClassInUse = Root.instance.uiManager.entityClassPicker.getSelectedEntityClass();
		selectedEntityInstances = new List<EntityInstance>();
	}

	public void load(ProjectIO projectIO)
	{
//		entityClassInUse = Root.instance.entityClassManager.getEntity(projectIO.readInt());
		selectedEntityInstances = new List<EntityInstance>();
	}

	public void save(ProjectIO projectIO)
	{
//		projectIO.writeInt(entityClassInUse.id);
	}

}
