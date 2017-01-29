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
		EntityInstance entityInstance = getClickedEntityInstance();

		if (entityInstance) {
			EntityInstanceDescription desc = entityInstance.entityInstanceDescription;
			if (!player.selectedEntityInstances.Contains(desc))
				player.selectEntityInstance(desc, true);
			else
				player.unselectAllEntityInstances();
		} else {
			player.unselectAllEntityInstances();
		}
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

			// If the user clicked on a VoxelObject leaf, it
			// will always have  a VoxelObjectRoot as parent
			entityInstance = go.transform.parent.GetComponent<EntityInstance>();
			return entityInstance;
		}
		return null;
	}
}
