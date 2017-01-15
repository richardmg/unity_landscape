using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickFromRaycast : MonoBehaviour {

	// Since the mouse pointer is hidden, we pretende that
	// the mouse is at screen center, issues a ray cast, and
	// perform the click / drag

	public float dragSpeed = 4f;
	public float dragStartDistance = 10f;

	GameObject m_button;
	PointerEventData m_ped = new PointerEventData(null);
	Vector3 m_playerRotation;
	Vector2 m_accumulatedDragDistance = Vector2.zero;

	void Update()
	{
		if (!m_button) {
			if (!Input.GetMouseButtonDown(0))
				return;
			m_button = getButtonUnderPointer();
			m_ped.position = new Vector2(Screen.width / 2, Screen.height / 2);
			m_ped.dragging = false;
			m_playerRotation = Root.instance.playerHeadGO.transform.rotation.eulerAngles;
			m_accumulatedDragDistance.Set(0, 0);
		}

		if (!m_button)
			return;

		if (Input.GetMouseButtonUp(0)) {
			EventTrigger[] triggers = m_button.GetComponents<EventTrigger>();
			foreach (EventTrigger t in triggers)
				t.OnPointerClick(m_ped);
			m_button = null;
			return;
		}

		m_ped.delta = calculateDragDelta();
		m_ped.clickCount = 0;

		if (!m_ped.dragging) {
			m_accumulatedDragDistance += m_ped.delta;
			if (m_accumulatedDragDistance.magnitude > dragStartDistance) {
				m_ped.dragging = true;
				// clickCount = -1 means first drag event
				m_ped.clickCount = -1;
			}
		}

		if (m_ped.dragging) {
			EventTrigger[] triggers = m_button.GetComponents<EventTrigger>();
			foreach (EventTrigger t in triggers)
				t.OnDrag(m_ped);
		}
	}

	Vector2 calculateDragDelta()
	{
		Vector3 newRotation = Root.instance.playerHeadGO.transform.rotation.eulerAngles;
		float deltaX = Mathf.DeltaAngle(newRotation.x, m_playerRotation.x);
		float deltaY = Mathf.DeltaAngle(newRotation.y, m_playerRotation.y);
		m_playerRotation = newRotation;
		return new Vector2(deltaY * -dragSpeed, deltaX * dragSpeed);
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
