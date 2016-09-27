using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ThingPainter : MonoBehaviour {
	public int atlasIndex = 0;
	public Texture2D atlas;

	Texture2D m_texture;

	// Use this for initialization
	void Start () {
		int atlasPixelX;
		int atlasPixelY;
		Global.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);

		m_texture = new Texture2D(Global.kSubImageWidth, Global.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		var pixels = atlas.GetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight);
		m_texture.SetPixels(pixels);
//
//		texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));
//		texture.SetPixel(1, 0, Color.clear);
//		texture.SetPixel(0, 1, Color.white);
//		texture.SetPixel(1, 1, Color.black);

		// Apply all SetPixel calls
		m_texture.Apply();

		// connect texture to material of GameObject this script is attached to
		GetComponent<RawImage>().texture = m_texture;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3[] corners = new Vector3[4];
		RawImage image = GetComponent<RawImage>();
		image.rectTransform.GetWorldCorners(corners);

		Rect newRect = new Rect(corners[0], corners[2] - corners[0]);

		//Get the pixel offset amount from the current mouse position to the left edge of the minimap
		//rect transform.  And likewise for the y offset position.
		float x = Input.mousePosition.x - newRect.x;
		float y = Input.mousePosition.y - newRect.y;
		print(x + ", " + y);
	}
}
