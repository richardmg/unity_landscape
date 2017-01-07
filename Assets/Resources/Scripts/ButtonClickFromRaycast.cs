using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickFromRaycast : MonoBehaviour {

	// Since the mouse pointer is hidden, we pretende that
	// the mouse is at screen center, issues a ray cast, and
	// perform the click

	void Update()
	{
		// Flytt ui nærmere camera...

//		GameObject ui = Root.instance.entityUiGO;
//		Vector3 pos = Root.instance.playerGO.transform.position;
//
//		ui.transform.position = pos;

		if (!Input.GetMouseButtonUp(0))
			return;
	
		PointerEventData ped = new PointerEventData(null);
		ped.position = new Vector2(Screen.width / 2, Screen.height / 2);

		List<RaycastResult> results = new List<RaycastResult>();
		GraphicRaycaster gr = GetComponentInParent<GraphicRaycaster>();
		gr.Raycast(ped, results);

		foreach (RaycastResult r in results) {
			Button button = r.gameObject.GetComponent<Button>();
			if (button) {
				button.OnPointerClick(ped);
				break;
			}
		}
	}
}
