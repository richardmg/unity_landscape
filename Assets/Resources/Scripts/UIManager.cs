﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public static class UIManager_GameObjectExtensions
{
	public static void pushDialog(this GameObject go, bool show = true, bool repush = false)
	{
		Root.instance.uiManager.push(go, show, repush);
	}

	public static void pushDialog(this GameObject go, Action<bool> callback, bool show = true, bool repush = false)
	{
		Root.instance.uiManager.push(go, callback, show, repush);
	}
}

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
	public GameObject backgroundGO;
	public GameObject uiFirstPersonGO;
	public GameObject uiColorPickerGO;
	public GameObject uiPaintEditorGO;
	public GameObject uiConstructionEditorGO;
	public GameObject uiEntityClassPickerGO;
	public GameObject uiCommandPromptGO;
	public GameObject entityPainterGO;
	public GameObject backButton;

	public GameObject currentMenu;

	[HideInInspector]
	public UIEntityClassPicker entityClassPicker;
	[HideInInspector]
	public EntityPainter entityPainter;

	List<UIManagerStackItem> stack = new List<UIManagerStackItem>();
	MonoBehaviour m_mouseGrab = null;

	void Awake()
	{
		entityClassPicker = uiEntityClassPickerGO.GetComponent<UIEntityClassPicker>();
		entityPainter = entityPainterGO.GetComponent<EntityPainter>();
	}

	void Start()
	{
		hideUI();
		uiEntityClassPickerGO.pushDialog(false);
		backButton.SetActive(false);
		setMenuVisible(false);
	}

	void hideUI()
	{
		backgroundGO.SetActive(false);
		uiFirstPersonGO.SetActive(false);
		uiColorPickerGO.SetActive(false);
		uiPaintEditorGO.SetActive(false);
		uiConstructionEditorGO.SetActive(false);
		uiEntityClassPickerGO.SetActive(false);
		uiCommandPromptGO.SetActive(false);

		m_mouseGrab = null;
	}

	public void push(GameObject ui, bool show = true, bool repush = false)
	{
		push(ui, (bool a) => {}, show, repush);
	}

	public void push(GameObject ui, Action<bool> callback, bool show = true, bool repush = false)
	{
		if (currentMenu != ui || repush)
			stack.Add(new UIManagerStackItem(ui, callback));
		currentMenu = ui;
		if (show)
			setMenuVisible(true);
		if (stack.Count > 1)
			backButton.SetActive(true);
	}

	public void pop(bool accepted)
	{
		Debug.Assert(stack.Count > 1);	
		UIManagerStackItem itemToPopOff = stack[stack.Count - 1];
		stack.RemoveAt(stack.Count - 1);	
		UIManagerStackItem itemToShow = stack[stack.Count - 1];
		currentMenu = itemToShow.ui;
		setMenuVisible(true);
		itemToPopOff.callback(accepted);
		if (stack.Count <= 1)
			backButton.SetActive(false);
	}

	public void popAll()
	{
		stack = new List<UIManagerStackItem>();
		backButton.SetActive(false);
	}

	public void showCommandPromptUI()
	{
		hideUI();
		uiCommandPromptGO.SetActive(true);
		enableCursorMode(true);
	}

	public void setMenuVisible(bool visible)
	{
		hideUI();
		backgroundGO.SetActive(visible);
		currentMenu.SetActive(visible);
		uiFirstPersonGO.SetActive(!visible);
		enableCursorMode(visible);
	}

	public void enableCursorMode(bool on)
	{
		if (on) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
			Root.instance.playerGO.GetComponent<FirstPersonController>().enabled = false;
		} else {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			Root.instance.playerGO.GetComponent<FirstPersonController>().enabled = true;
		}
	}

	void Update () {
		if (uiCommandPromptGO.activeSelf) {
			if (Input.GetKeyDown(KeyCode.Escape))
				setMenuVisible(false);
			return;
		}

		if (Input.GetKeyDown(KeyCode.Escape))
			showCommandPromptUI();
		else if (Input.GetKeyDown(KeyCode.Tab))
			setMenuVisible(uiFirstPersonGO.activeSelf);
	}

	static public Vector2 getMousePosOnImage(RawImage image, bool flipY = false)
	{
		Vector3[] corners = new Vector3[4];
		image.rectTransform.GetWorldCorners(corners);
		float uvx = (Input.mousePosition.x - corners[0].x) / (corners[2].x - corners[0].x);
		float uvy = (Input.mousePosition.y - corners[0].y) / (corners[2].y - corners[0].y);
		return new Vector2(uvx, flipY ? 1 - uvy : uvy);
	}

	static public bool isInside(Vector2 uv)
	{
		return (uv.x > 0 && uv.x <= 1 && uv.y > 0 && uv.y <= 1);
	}

	public bool grabMouse(MonoBehaviour ui, bool requireMousePress = true)
	{
		if (requireMousePress && !Input.GetMouseButton(0))
			return false;
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
