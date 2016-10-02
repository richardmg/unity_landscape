using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;

public class KeyboardControls : MonoBehaviour {
	public GameObject player;
	public GameObject uiFirstPerson;
	public GameObject uiThingPainter;
	public KeyCode switchKey;

	void Update () {
		if (!Input.GetKeyDown(switchKey))
			return;

		FirstPersonController controller = player.GetComponent<FirstPersonController>();
		controller.enabled = !controller.enabled;
		Cursor.visible = !controller.enabled;
		Cursor.lockState = CursorLockMode.None;

		uiFirstPerson.SetActive(controller.enabled);
		uiThingPainter.SetActive(!controller.enabled);
	}
}
