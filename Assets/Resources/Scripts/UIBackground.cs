using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBackground : MonoBehaviour {
	public GameObject backgroundGO;

	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Vector2 uv = UIManager.getMousePosOnImage(backgroundGO.GetComponent<RawImage>(), true);
			if (UIManager.isInside(uv))
				return;
			Root.instance.uiManager.setMenuVisible(false);
		}
	}

	public void onBackButtonClicked()
	{
		Root.instance.uiManager.pop(false);
	}

}
