﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonClickFromRaycast : MonoBehaviour {

	// Since the mouse pointer is hidden, we pretende that
	// the mouse is at screen center, issues a ray cast, and
	// perform the click

	GameObject m_button;
	bool m_grab = false;
	PointerEventData m_ped;

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) {
			m_ped = new PointerEventData(null);
			m_ped.position = new Vector2(Screen.width / 2, Screen.height / 2);
			m_button = getButtonUnderPointer();
			m_grab = true;
		}
		if (Input.GetMouseButtonUp(0)) {
			if (m_button)
				m_button.GetComponent<Button>().OnPointerClick(m_ped);
			m_grab = false;
			m_button = null;
		}
		if (m_grab) {
			if (m_button) {
				// Assume only one EventTrigger until a need arise
				EventTrigger trigger = m_button.GetComponent<EventTrigger>();
				if (trigger)
					trigger.OnDrag(m_ped);
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
