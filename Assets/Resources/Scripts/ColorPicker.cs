using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ColorPicker : MonoBehaviour {

	void Update ()
	{
		if (!Input.GetMouseButton(0))
			return;

		RawImage image = GetComponent<RawImage>();
		Vector2 uv = UIManager.getMousePosOnImage(image);
		if (!UIManager.isInside(uv))
			return;

		Texture2D texture = (Texture2D)image.texture;
		int pixelX = (int)(uv.x * texture.width);
		int pixelY = (int)(uv.y * texture.height);
		Color color = texture.GetPixel(pixelX, pixelY);
		print(color);
	}
}
