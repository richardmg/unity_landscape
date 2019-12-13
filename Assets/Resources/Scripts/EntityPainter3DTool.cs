using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityPainter3DTool : MonoBehaviour
{
	public void OnEnable() {
		Root.instance.uiManager.grabMouse(this);
	}

	void Update()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		Root.instance.entityToolManager.selectionTool.selectSingleObjectUnderPointer();
		if (Root.instance.player.selectedEntityInstances.Count == 0)
			return;	

		EntityInstance instance = Root.instance.player.selectedEntityInstances[0].instance;
		Root.instance.uiManager.entityPainter.setEntityInstance(instance);
		Root.instance.uiManager.uiPaintEditorGO.pushDialog();
	}
}
