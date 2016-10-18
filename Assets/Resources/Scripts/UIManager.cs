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
	public GameObject uiFirstPersonGO;
	public GameObject uiColorPickerGO;
	public GameObject uiPaintEditorGO;
	public GameObject uiEntityClassPickerGO;
	public GameObject entityPainterGO;
	public GameObject backButton;

	public KeyCode uiOnOffKey;

	GameObject m_currentMenu;

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
		push(uiEntityClassPickerGO, (bool accepted) => {});
		backButton.SetActive(false);
		showFirstPersonUI();
	}

	void hideUI()
	{
		backgroundGO.SetActive(false);
		uiFirstPersonGO.SetActive(false);
		uiColorPickerGO.SetActive(false);
		uiPaintEditorGO.SetActive(false);
		uiEntityClassPickerGO.SetActive(false);

		m_mouseGrab = null;
	}

	public void push(GameObject ui, Action<bool> callback)
	{
		stack.Add(new UIManagerStackItem(ui, callback));
		m_currentMenu = ui;
		showCurrentMenu();
		backButton.SetActive(true);
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
		if (stack.Count <= 1)
			backButton.SetActive(false);
	}

	public void showFirstPersonUI()
	{
		hideUI();
		backgroundGO.SetActive(false);
		uiFirstPersonGO.SetActive(true);
		enableCursorMode(false);
	}

	public void showCurrentMenu()
	{
		hideUI();
		if (m_currentMenu == uiPaintEditorGO) {
			backgroundGO.SetActive(true);
			uiPaintEditorGO.SetActive(true);
			enableCursorMode(true);
		} else if (m_currentMenu == uiColorPickerGO) {
			backgroundGO.SetActive(true);
			uiColorPickerGO.SetActive(true);
			enableCursorMode(true);
		} else if (m_currentMenu == uiEntityClassPickerGO) {
			backgroundGO.SetActive(true);
			uiEntityClassPickerGO.SetActive(true);
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

		if (uiFirstPersonGO.activeSelf)
			showCurrentMenu();
		else
			showFirstPersonUI();
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
