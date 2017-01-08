using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntitySelectionTool : MonoBehaviour
{
	void Update()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		if (Input.GetKey(KeyCode.LeftApple)) {
			EntityInstance entityInstance = getClickedEntityInstance();
			if (entityInstance)
				Root.instance.player.selectEntityInstance(entityInstance.entityInstanceDescription);
		} else if (!Root.instance.player.currentTool.activeSelf) {
			// FACTOR OUT IN SEPARATE TOOL
			createNewEntityInstance();
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
