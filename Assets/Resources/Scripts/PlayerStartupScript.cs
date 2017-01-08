using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerStartupScript : MonoBehaviour, IProjectIOMember {
	public bool moveToGround = true;
	public EntityClass entityClassInUse = null;
	public GameObject currentTool;
	public List<EntityInstanceDescription> selectedEntityInstances;

	void Start()
	{
		currentTool = Root.instance.entityToolManager.moveTool.gameObject;

		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}

	public void selectEntityInstance(EntityInstanceDescription entityInstance)
	{
		selectedEntityInstances.Add(entityInstance);
		Root.instance.notificationManager.notifySelectionChanged();
	}

	public void unselectEntityInstance(EntityInstanceDescription entityInstance)
	{
		if (entityInstance == null)
			selectedEntityInstances.Clear();
		else
			selectedEntityInstances.Remove(entityInstance);

		Root.instance.notificationManager.notifySelectionChanged();
	}

	public void setEntityClassInUse(EntityClass entityClass)
	{
//		entityClassInUse = entityClass;
	}

	public void initNewProject()
	{
//		entityClassInUse = Root.instance.uiManager.entityClassPicker.getSelectedEntityClass();
		selectedEntityInstances = new List<EntityInstanceDescription>();
	}

	public void load(ProjectIO projectIO)
	{
//		entityClassInUse = Root.instance.entityClassManager.getEntity(projectIO.readInt());
		selectedEntityInstances = new List<EntityInstanceDescription>();
	}

	public void save(ProjectIO projectIO)
	{
//		projectIO.writeInt(entityClassInUse.id);
	}

}
