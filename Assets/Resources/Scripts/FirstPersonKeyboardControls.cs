using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstPersonKeyboardControls : MonoBehaviour
{
	void Update()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		if (Input.GetKey(KeyCode.LeftApple)) {
			EntityInstance entityInstance = getClickedEntityInstance();
			if (entityInstance)
				selectEntityInstance(entityInstance);
		} else if (!Root.instance.entityUiGO.activeSelf) {
			createNewEntityInstance();
		}
	}

	void selectEntityInstance(EntityInstance entityInstance)
	{
		Root.instance.player.selectedEntityInstances.Add(entityInstance);
		// todo: check which tool the user holds. But for now it will always be "move" tool
		GameObject ui = Root.instance.entityUiGO;
		ui.SetActive(true);
		ui.transform.SetParent(entityInstance.transform);
		ui.transform.localPosition = new Vector3(0, 0, 0);
	}

	void unselectEntityInstance(EntityInstance entityInstance)
	{
		if (entityInstance == null)
			Root.instance.player.selectedEntityInstances.Clear();
		else
			Root.instance.player.selectedEntityInstances.Remove(entityInstance);

		if (Root.instance.player.selectedEntityInstances.Count == 0) {
			GameObject ui = Root.instance.entityUiGO;
			ui.transform.SetParent(null);
			ui.SetActive(false);
		}
	}

	void createNewEntityInstance()
	{
		Vector3 worldPos = Camera.main.transform.position + (Camera.main.transform.forward * 5);
		worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos);

		EntityClass entityClass = new EntityClass();
		entityClass.voxelObjectRoot.add(new VoxelObject(0, 4));

		EntityInstanceDescription desc = new EntityInstanceDescription(entityClass, worldPos);
		Root.instance.notificationManager.notifyEntityInstanceDescriptionAdded(desc);
	}

	EntityInstance getClickedEntityInstance()
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
		if (Physics.Raycast(ray, out hit)) {
			GameObject go = hit.transform.gameObject;
			EntityInstance entityInstance = go.GetComponent<EntityInstance>();
			if (entityInstance)
				return entityInstance;

			// If the user clicked on a VoxelObject leaf, it
			// will always have  a VoxelObjectRoot as parent
			entityInstance = go.transform.parent.GetComponent<EntityInstance>();
			return entityInstance;
		}
		return null;
	}


//	Vector3 getRayLandscapePos()
//	{
//		RaycastHit hit;
//		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
//		LayerMask layerMask = ~LayerMask.NameToLayer("LandscapeGround");
//		return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) ? hit.point : Vector3.zero;
//	}
}
