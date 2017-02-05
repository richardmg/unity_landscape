using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntityCreateTool : MonoBehaviour
{
	void Update()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		Vector3 worldPos;
		if (!getRayWorldHitPoint(out worldPos))
			return;
		
		Transform playerTransform = Root.instance.playerGO.transform;
		Vector3 rotation = playerTransform.rotation.eulerAngles;

		Root.instance.alignmentManager.align(ref worldPos, ref rotation);

		EntityClass entityClass = Root.instance.player.entityClassInUse;
		if (!entityClass)
			return;

		print(entityClass);
		
//		EntityClass entityClass = new EntityClass();
//		entityClass.voxelObjectRoot.add(new VoxelObject(0, 1f));

		EntityInstanceDescription desc = new EntityInstanceDescription(entityClass, worldPos, rotation);
		Root.instance.notificationManager.notifyEntityInstanceDescriptionAdded(desc);
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
