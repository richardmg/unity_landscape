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
		} else if (ui == paintEditor) {
			background.SetActive(true);
			paintEditor.SetActive(true);
		} else if (ui == colorPicker) {
			background.SetActive(true);
			colorPicker.SetActive(true);
		}
	}

	void Update () {
		if (!Input.GetKeyDown(uiOnOffKey))
			return;

		bool enableFps = !firstPerson.activeSelf;

		FirstPersonController controller = Root.instance.player.GetComponent<FirstPersonController>();
		controller.enabled = enableFps;
		Cursor.visible = !enableFps;
		Cursor.lockState = CursorLockMode.None;

		if (enableFps)
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

}
