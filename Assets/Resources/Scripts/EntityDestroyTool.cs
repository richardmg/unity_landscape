using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EntityDestroyTool : MonoBehaviour
{	
	void Update()
	{
		if (!Input.GetMouseButtonDown(0))
			return;
		
		Root.instance.entityToolManager.selectionTool.selectSingleObjectUnderPointer();
		if (Root.instance.player.selectedEntityInstances.Count == 0)
			return;	

		EntityInstanceDescription desc = Root.instance.player.selectedEntityInstances[0];
		Root.instance.notificationManager.notifyEntityInstanceDescriptionRemoved(desc);
	}
}
