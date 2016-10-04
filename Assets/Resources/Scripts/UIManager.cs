using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
	public GameObject background;
	public GameObject firstPerson;
	public GameObject colorPicker;
	public GameObject paintEditor;

	public KeyCode uiOnOffKey;

	void Start()
	{
		hideUI();
		firstPerson.SetActive(true);
	}

	public void hideUI()
	{
		background.SetActive(false);
		firstPerson.SetActive(false);
		colorPicker.SetActive(false);
		paintEditor.SetActive(false);
	}

	public void showColorPicker()
	{
		hideUI();
		background.SetActive(true);
		colorPicker.SetActive(true);
	}

	public void showFirstPerson()
	{
		hideUI();
		background.SetActive(false);
		firstPerson.SetActive(true);
	}

	public void showPaintEditor()
	{
		hideUI();
		background.SetActive(true);
		paintEditor.SetActive(true);
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
			showFirstPerson();
		else
			showPaintEditor();
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
