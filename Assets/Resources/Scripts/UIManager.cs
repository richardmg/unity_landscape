using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

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

}
