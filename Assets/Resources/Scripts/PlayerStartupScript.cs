using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerStartupScript : MonoBehaviour, IProjectIOMember {
	public bool moveToGround = true;
	public EntityClass entityClassInUse = null;
	public GameObject currentTool;
	public List<EntityInstance> selectedEntityInstances;

	void Start()
	{
		currentTool = Root.instance.entityToolManager.moveTool.gameObject;

		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}

	public void selectEntityInstance(EntityInstance entityInstance)
	{
		selectedEntityInstances.Add(entityInstance);
		Root.instance.notificationManager.notifyEntityInstanceSelectionChanged();
	}

	public void unselectEntityInstance(EntityInstance entityInstance)
	{
		if (entityInstance == null)
			selectedEntityInstances.Clear();
		else
			selectedEntityInstances.Remove(entityInstance);

		if (Root.instance.player.selectedEntityInstances.Count == 0)
			currentTool.SetActive(false);
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
