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
	public GameObject backgroundGO;
	public GameObject firstPersonGO;
	public GameObject colorPickerGO;
	public GameObject paintEditorGO;
	public GameObject prefabVariantPickerGO;

	public KeyCode uiOnOffKey;

	GameObject m_currentMenu;

	[HideInInspector]
	public UIPrefabVariantPicker prefabVariantPicker;

	List<UIManagerStackItem> stack = new List<UIManagerStackItem>();
	MonoBehaviour m_mouseGrab = null;

	void Awake()
	{
		prefabVariantPicker = prefabVariantPickerGO.GetComponent<UIPrefabVariantPicker>();
	}

	void Start()
	{
		hideUI();
		push(prefabVariantPickerGO, (bool accepted) => {});
		showFirstPersonUI();
	}

	void hideUI()
	{
		backgroundGO.SetActive(false);
		firstPersonGO.SetActive(false);
		colorPickerGO.SetActive(false);
		paintEditorGO.SetActive(false);
		prefabVariantPickerGO.SetActive(false);

		m_mouseGrab = null;
	}

	public void push(GameObject ui, Action<bool> callback)
	{
		stack.Add(new UIManagerStackItem(ui, callback));
		m_currentMenu = ui;
		showCurrentMenu();
	}

	public void pop(bool accepted)
	{
		Debug.Assert(stack.Count > 1);	
		UIManagerStackItem itemToPopOff = stack[stack.Count - 1];
		stack.RemoveAt(stack.Count - 1);	
		UIManagerStackItem itemToShow = stack[stack.Count - 1];
		m_currentMenu = itemToShow.ui;
		showCurrentMenu();
		itemToPopOff.callback(accepted);
	}

	public void showFirstPersonUI()
	{
		hideUI();
		backgroundGO.SetActive(false);
		firstPersonGO.SetActive(true);
		enableCursorMode(false);
	}

	public void showCurrentMenu()
	{
		hideUI();
		if (m_currentMenu == paintEditorGO) {
			backgroundGO.SetActive(true);
			paintEditorGO.SetActive(true);
			enableCursorMode(true);
		} else if (m_currentMenu == colorPickerGO) {
			backgroundGO.SetActive(true);
			colorPickerGO.SetActive(true);
			enableCursorMode(true);
		} else if (m_currentMenu == prefabVariantPickerGO) {
			backgroundGO.SetActive(true);
			prefabVariantPickerGO.SetActive(true);
			enableCursorMode(true);
		} else {
			Debug.Assert(false, "Unknown UI to show: " + m_currentMenu);
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

		if (firstPersonGO.activeSelf)
			showCurrentMenu();
		else
			showFirstPersonUI();
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
