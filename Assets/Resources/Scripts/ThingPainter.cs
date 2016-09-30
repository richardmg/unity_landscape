using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ThingPainter : MonoBehaviour {
	public int atlasIndex = 0;
	public Texture2D atlas;

	Texture2D m_texture;

	void OnEnable ()
    {
		int atlasPixelX;
		int atlasPixelY;
		Global.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		var pixels = atlas.GetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight);

		m_texture = new Texture2D(Global.kSubImageWidth, Global.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.SetPixels(pixels);

		m_texture.Apply();
		GetComponent<RawImage>().texture = m_texture;
	}
	
	void Update ()
    {
		if (!Input.GetMouseButton(0))
			return;

		Vector3[] corners = new Vector3[4];
		RawImage image = GetComponent<RawImage>();
		image.rectTransform.GetWorldCorners(corners);

		float uvx = (Input.mousePosition.x - corners[0].x) / (corners[2].x - corners[0].x);
		float uvy = (Input.mousePosition.y - corners[0].y) / (corners[2].y - corners[0].y);

        int pixelX = (int)(uvx * Global.kSubImageWidth);
        int pixelY = (int)(uvy * Global.kSubImageHeight);

		m_texture.SetPixel(pixelX, pixelY, Color.black);
		m_texture.Apply();
	}
}
