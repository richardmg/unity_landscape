using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickFromRaycast : MonoBehaviour {

	// Since the mouse pointer is hidden, we pretende that
	// the mouse is at screen center, issues a ray cast, and
	// perform the click

	GameObject m_button;
	PointerEventData m_ped = new PointerEventData(null);
	Vector3 m_playerRotation;
	float dragScale = 4f;

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) {
			m_ped.position = new Vector2(Screen.width / 2, Screen.height / 2);
			m_button = getButtonUnderPointer();
			m_playerRotation = Root.instance.playerHeadGO.transform.rotation.eulerAngles;
			m_ped.dragging = true;
		}

		if (Input.GetMouseButtonUp(0)) {
			if (m_button) {
				EventTrigger[] triggers = m_button.GetComponents<EventTrigger>();
				foreach (EventTrigger t in triggers)
					t.OnPointerClick(m_ped);
			}
			m_ped.dragging = false;
			m_button = null;
		}

		if (m_ped.dragging) {
			if (m_button) {
				Vector3 newRotation = Root.instance.playerHeadGO.transform.rotation.eulerAngles;
				float deltaX = Mathf.DeltaAngle(newRotation.x, m_playerRotation.x);
				float deltaY = Mathf.DeltaAngle(newRotation.y, m_playerRotation.y);
				m_playerRotation = newRotation;
				m_ped.delta = new Vector2(deltaY * -dragScale, deltaX * dragScale);
				EventTrigger[] triggers = m_button.GetComponents<EventTrigger>();
				foreach (EventTrigger t in triggers)
					t.OnDrag(m_ped);
			}
		}
	}

	GameObject getButtonUnderPointer()
	{
		List<RaycastResult> results = new List<RaycastResult>();
		GraphicRaycaster gr = GetComponentInParent<GraphicRaycaster>();
		gr.Raycast(m_ped, results);
		foreach (RaycastResult r in results) {
			if (r.gameObject.GetComponent<Button>())
				return r.gameObject;
		}
		return null;
	}

}
