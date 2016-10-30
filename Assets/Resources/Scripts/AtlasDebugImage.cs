using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AtlasDebugImage : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	void Update () {
		if (!Input.GetMouseButtonDown(0))
			return;

		Vector2 uv = UIManager.getMousePosOnImage(GetComponent<RawImage>(), true);
		if (!UIManager.isInside(uv))
			return;

		int colCount = Root.kAtlasWidth / Root.kSubImageWidth;
		int rowCount = Root.kAtlasHeight / Root.kSubImageHeight;

		int x = (int)(uv.x * colCount);
		int y = (int)((1 - uv.y) * rowCount);
		int index = x + (y * colCount);

		Root.instance.commandPrompt.log("You clicked on atlas index: " + index);
		Root.instance.commandPrompt.performCommand("atlas hide");
	}
}
