using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntitySelectionTool : MonoBehaviour
{
	[HideInInspector] public RaycastHit lastHit;

	public void selectSingleObjectUnderPointer()
	{
		PlayerStartupScript player = Root.instance.player;

		if (player.selectedEntityInstances.Count > 0) {
			player.unselectAllEntityInstances();
			return;
		}

		EntityInstance entityInstance = getClickedEntityInstance();
		if (!entityInstance)
			return;
		
		EntityInstanceDescription desc = entityInstance.entityInstanceDescription;
		player.selectEntityInstance(desc, true);
	}

	public void selectMultipleObjectsUnderPointer()
	{
		Debug.Assert(false, "not implemented");
		//bool unselectEverythingElse = !Input.GetKey(KeyCode.LeftShift);
	}

	EntityInstance getClickedEntityInstance()
	{
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
		if (Physics.Raycast(ray, out lastHit)) {
			GameObject go = lastHit.transform.gameObject;

			EntityInstance entityInstance = go.GetComponent<EntityInstance>();
			if (entityInstance)
				return entityInstance;

			if (go.GetComponent<VoxelObjectMonoBehaviour>()) {
				// A VoxelObject leaf should always be in a
				// EntityInstance->VoxelObjectRoot->VoxelObject relation.
				// This relation is created by the EntityClass when creating an instance.
				entityInstance = go.transform.parent.parent.GetComponent<EntityInstance>();
				Debug.Assert(entityInstance);
				return entityInstance;
			}
		}
		return null;
	}
}
