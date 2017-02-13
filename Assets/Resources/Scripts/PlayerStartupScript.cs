using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerStartupScript : MonoBehaviour, IProjectIOMember, IEntityInstanceDescriptionListener {
	public bool moveToGround = true;
	public EntityClass entityClassInUse = null;
	public List<EntityInstanceDescription> selectedEntityInstances = new List<EntityInstanceDescription>();

	public GameObject mainTool;
	public GameObject subTool;

	void Awake()
	{
		Root.instance.notificationManager.addEntityInstanceListener(this);
	}

	void Start()
	{
		if (moveToGround) {
			Vector3 worldPos = transform.position;
			worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos) + 1;
			transform.position = worldPos;
		}
	}

	public void setWalkSpeed(float speed)
	{
		GetComponent<FirstPersonController>().m_WalkSpeed = speed;
	}

	public void setDefaultWalkSpeed()
	{
		setWalkSpeed(4);
	}

	public void selectEntityInstance(EntityInstanceDescription entityInstance, bool unselectEverythingElse = false)
	{
		List<EntityInstanceDescription> oldSelection = new List<EntityInstanceDescription>(selectedEntityInstances);
		if (unselectEverythingElse)
			selectedEntityInstances.Clear();
		selectedEntityInstances.Add(entityInstance);
		Root.instance.notificationManager.notifySelectionChanged(oldSelection, selectedEntityInstances);
	}

	public void unselectEntityInstance(EntityInstanceDescription entityInstance)
	{
		List<EntityInstanceDescription> oldSelection = new List<EntityInstanceDescription>(selectedEntityInstances);
		selectedEntityInstances.Remove(entityInstance);
		Root.instance.notificationManager.notifySelectionChanged(oldSelection, selectedEntityInstances);
	}

	public void unselectAllEntityInstances()
	{
		List<EntityInstanceDescription> oldSelection = new List<EntityInstanceDescription>(selectedEntityInstances);
		selectedEntityInstances.Clear();
		Root.instance.notificationManager.notifySelectionChanged(oldSelection, selectedEntityInstances);
	}

	public void setEntityClassInUse(EntityClass entityClass)
	{
//		entityClassInUse = entityClass;
	}

	public void onEntityInstanceDescriptionRemoved(EntityInstanceDescription desc) {
		if (selectedEntityInstances.Find(d => d == desc) != null)
			unselectEntityInstance(desc);
	}

	public void onEntityInstanceDescriptionAdded(EntityInstanceDescription desc) {}
	public void onEntityInstanceDescriptionChanged(EntityInstanceDescription desc, EntityInstanceDescription.DirtyFlags flags) {}

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
