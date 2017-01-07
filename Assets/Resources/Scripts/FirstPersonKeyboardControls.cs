using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FirstPersonKeyboardControls : MonoBehaviour
{
	void Update()
	{
		GameObject ui = Root.instance.entityUiGO;
		if (ui.activeSelf)
			return;

		if (Input.GetMouseButtonDown(0)) {
			// Create new entity:
			Vector3 pos = Camera.main.transform.position + (Camera.main.transform.forward * 5);
			pos.y = Root.instance.landscapeManager.sampleHeight(pos);
			VoxelObject vo = new VoxelObject(0, 4);
			GameObject go = vo.createGameObject(null, Root.kLod0, true);
			go.transform.position = pos;

			Root.instance.player.gameObjectInUse = go;

			if (Input.GetKey(KeyCode.LeftApple))
				showEntityUI();
		}	
	}

	void showEntityUI()
	{
		GameObject ui = Root.instance.entityUiGO;
		ui.SetActive(true);
		ui.transform.SetParent(Root.instance.player.gameObjectInUse.transform);
		ui.transform.localPosition = new Vector3(0, 0, 0);
	}

//	Vector3 getRayLandscapePos()
//	{
//		RaycastHit hit;
//		Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
//		LayerMask layerMask = ~LayerMask.NameToLayer("LandscapeGround");
//		return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) ? hit.point : Vector3.zero;
//	}
}
