using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class EntityPainterTool : MonoBehaviour
{
	public void OnEnable()
	{
		Root.instance.uiManager.grabMouse(this);
	}

	void Update()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		Root.instance.entityToolManager.selectionTool.selectSingleObjectUnderPointer();
		if (Root.instance.player.selectedEntityInstances.Count == 0)
			return;	

		Debug.Log("Paint: " + Root.instance.player.selectedEntityInstances[0].instance);
	}
}
