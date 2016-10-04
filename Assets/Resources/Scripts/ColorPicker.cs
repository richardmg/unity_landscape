using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorPicker : MonoBehaviour {

	public Color selectedColor = Color.black;

	void Update ()
	{
		if (!Input.GetMouseButtonDown(0))
			return;

		RawImage image = GetComponent<RawImage>();
		Vector2 uv = UIManager.getMousePosOnImage(image);
		if (!UIManager.isInside(uv))
			return;

		Texture2D texture = (Texture2D)image.texture;
		int pixelX = (int)(uv.x * texture.width);
		int pixelY = (int)(uv.y * texture.height);

		selectedColor = texture.GetPixel(pixelX, pixelY);
		Root.instance.uiManager.pop();
	}
}
