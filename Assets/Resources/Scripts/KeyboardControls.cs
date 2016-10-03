using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class KeyboardControls : MonoBehaviour {
	public KeyCode switchKey;

	void Update () {
		if (!Input.GetKeyDown(switchKey))
			return;

		bool enableFps = !Root.instance.uiFirstPerson.activeSelf;

		FirstPersonController controller = Root.instance.player.GetComponent<FirstPersonController>();
		controller.enabled = enableFps;
		Cursor.visible = !enableFps;
		Cursor.lockState = CursorLockMode.None;

		Root.instance.hideUI();
		Root.instance.uiFirstPerson.SetActive(enableFps);
		Root.instance.uiPaintEditor.SetActive(!enableFps);
		Root.instance.uiBackground.SetActive(!enableFps);
	}
}
