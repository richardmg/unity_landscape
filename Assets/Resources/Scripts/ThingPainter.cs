using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ThingPainter : MonoBehaviour {
	public int atlasIndex = 0;
	public Texture2D atlas;

	Texture2D m_texture;

	// Use this for initialization
	void Start () {
		int startPixelX = (atlasIndex * Global.kSubImageWidth) % Global.kAtlasWidth;
		int startPixelY = (int)((atlasIndex * Global.kSubImageWidth) / Global.kAtlasHeight) * Global.kSubImageHeight;

		m_texture = new Texture2D(Global.kSubImageWidth, Global.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		var pixels = atlas.GetPixels(startPixelX, startPixelY, Global.kSubImageWidth, Global.kSubImageHeight);
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
	
	}
}
