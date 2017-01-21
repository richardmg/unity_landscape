using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntityCreateTool : MonoBehaviour
{

	public bool autoSelect = true;

	void Update()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		Vector3 worldPos;
		if (!getRayWorldHitPoint(out worldPos))
			return;
		
		// Vector3 worldPos = Camera.main.transform.position + (Camera.main.transform.forward * 5);
		// worldPos.y = Root.instance.landscapeManager.sampleHeight(worldPos);

		worldPos = Root.instance.worldScaleManager.align(worldPos);

		EntityClass entityClass = new EntityClass();
		entityClass.voxelObjectRoot.add(new VoxelObject(0, 4));

		EntityInstanceDescription desc = new EntityInstanceDescription(entityClass, worldPos);
		Root.instance.notificationManager.notifyEntityInstanceDescriptionAdded(desc);

		if (autoSelect) {
			Root.instance.entityToolManager.activateTool(Root.instance.entityToolManager.moveToolGo, gameObject);
			Root.instance.player.selectEntityInstance(desc);
		}
	}

	bool getRayWorldHitPoint(out Vector3 v)
	{
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));

		if (Physics.Raycast(ray, out hit, 10)) {
			v = hit.point;
			return true;
		}
		v = Vector3.zero;
		return false;
	}

	//	Vector3 getRayLandscapePos()
	//	{
	//		RaycastHit hit;
	//		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
	//		LayerMask layerMask = ~LayerMask.NameToLayer("LandscapeGround");
	//		return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) ? hit.point : Vector3.zero;
	//	}
}
