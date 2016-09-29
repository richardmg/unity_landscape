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
		var pixels = atlas.GetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight);

		m_texture = new Texture2D(Global.kSubImageWidth, Global.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.SetPixels(pixels);
//
//		texture.SetPixel(0, 0, new Color(1.0f, 1.0f, 1.0f, 0.5f));
//		texture.SetPixel(1, 0, Color.clear);
//		texture.SetPixel(0, 1, Color.white);
//		texture.SetPixel(1, 1, Color.black);

		m_texture.Apply();
		GetComponent<RawImage>().texture = m_texture;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3[] corners = new Vector3[4];
		RawImage image = GetComponent<RawImage>();
		image.rectTransform.GetWorldCorners(corners);

		float uvx = (Input.mousePosition.x - corners[0].x) / (corners[2].x - corners[0].x);
		float uvy = (Input.mousePosition.y - corners[0].y) / (corners[2].y - corners[0].y);

        int pixelX = (int)(uvx * Global.kSubImageWidth);
        int pixelY = (int)(uvy * Global.kSubImageHeight);

		print(pixelX + ", " + pixelY);

		m_texture.SetPixel(pixelX, pixelY, Color.black);
		m_texture.Apply();
	}
}
