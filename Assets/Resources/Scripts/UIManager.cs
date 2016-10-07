using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

class UIManagerStackItem {
	public GameObject ui;
	public Action<bool> callback;
	public UIManagerStackItem(GameObject ui, Action<bool> callback)
	{
		this.ui = ui;
		this.callback = callback;
	}
}

public class UIManager : MonoBehaviour {
	public GameObject background;
	public GameObject firstPerson;
	public GameObject colorPicker;
	public GameObject paintEditor;

	public KeyCode uiOnOffKey;

	List<UIManagerStackItem> stack = new List<UIManagerStackItem>();
	MonoBehaviour m_mouseGrab = null;

	void Start()
	{
		show(firstPerson);
	}

	void hideUI()
	{
		background.SetActive(false);
		firstPerson.SetActive(false);
		colorPicker.SetActive(false);
		paintEditor.SetActive(false);

		m_mouseGrab = null;
	}

	public void show(GameObject ui)
	{
		stack = new List<UIManagerStackItem>();
		showUI(ui);
	}

	public void push(GameObject ui, Action<bool> callback)
	{
		callback(true);
		stack.Add(new UIManagerStackItem(ui, callback));
		showUI(ui);
	}

	public void pop(bool accepted)
	{
		Debug.Assert(stack.Count > 1);	
		UIManagerStackItem itemToPopOff = stack[stack.Count - 1];
		stack.RemoveAt(stack.Count - 1);	
		UIManagerStackItem itemToShow = stack[stack.Count - 1];
		showUI(itemToShow.ui);
		itemToPopOff.callback(accepted);
	}

	void showUI(GameObject ui)
	{
		hideUI();
		if (ui == firstPerson) {
			background.SetActive(false);
			firstPerson.SetActive(true);
			enableCursorMode(false);
		} else if (ui == paintEditor) {
			background.SetActive(true);
			paintEditor.SetActive(true);
			enableCursorMode(true);
		} else if (ui == colorPicker) {
			background.SetActive(true);
			colorPicker.SetActive(true);
			enableCursorMode(true);
		}
	}

	public void enableCursorMode(bool on)
	{
		Root.instance.playerGO.GetComponent<FirstPersonController>().enabled = !on;
		Cursor.visible = on;
		Cursor.lockState = CursorLockMode.None;
	}

	void Update () {
		if (!Input.GetKeyDown(uiOnOffKey))
			return;

		if (!firstPerson.activeSelf)
			show(firstPerson);
		else
			push(paintEditor, (bool accepted) => {});
	}

	static public Vector2 getMousePosOnImage(RawImage image)
	{
		Vector3[] corners = new Vector3[4];
		image.rectTransform.GetWorldCorners(corners);
		float uvx = (Input.mousePosition.x - corners[0].x) / (corners[2].x - corners[0].x);
		float uvy = (Input.mousePosition.y - corners[0].y) / (corners[2].y - corners[0].y);
		return new Vector2(uvx, uvy);
	}

	static public bool isInside(Vector2 uv)
	{
		return (uv.x > 0 && uv.x <= 1 && uv.y > 0 && uv.y <= 1);
	}

	public bool grabMouse(MonoBehaviour ui)
	{
		if (m_mouseGrab == ui)
			return true;
		if (!Input.GetMouseButtonDown(0))
			return false;

		m_mouseGrab = ui;
		return true;
	}

	public void clearMouseGrab()
	{
		m_mouseGrab = null;
	}	
}
