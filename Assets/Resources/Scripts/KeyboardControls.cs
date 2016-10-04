using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class KeyboardControls : MonoBehaviour {
	public KeyCode switchKey;

	void Update () {
		if (!Input.GetKeyDown(switchKey))
			return;

		bool enableFps = !Root.instance.uiManager.firstPerson.activeSelf;

		FirstPersonController controller = Root.instance.player.GetComponent<FirstPersonController>();
		controller.enabled = enableFps;
		Cursor.visible = !enableFps;
		Cursor.lockState = CursorLockMode.None;

		GameObject ui = enableFps ? Root.instance.uiManager.firstPerson : Root.instance.uiManager.paintEditor;
		Root.instance.uiManager.showUI(ui);
	}
}
