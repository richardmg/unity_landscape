using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ThingPainter : MonoBehaviour {
	public Texture2D atlas;
	public Color color = Color.black;

	Texture2D m_texture;
	int m_atlasIndex = VoxelObject.kUnknown;

	void OnEnable ()
    {
		// Use scale to enlarge the UI instead of the rect. At least not both.
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.x == Global.kSubImageWidth);
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.y == Global.kSubImageHeight);
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
		if (uvx < 0 || uvx > 1 || uvy < 0 || uvy > 1)
			return;

        int pixelX = (int)(uvx * Global.kSubImageWidth);
        int pixelY = (int)(uvy * Global.kSubImageHeight);

		m_texture.SetPixel(pixelX, pixelY, color);
		m_texture.Apply();
	}

	void setIndex(int atlasIndex)
    {
		m_atlasIndex = atlasIndex;

		int atlasPixelX, atlasPixelY;
		Global.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		var pixels = atlas.GetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight);

		m_texture = new Texture2D(Global.kSubImageWidth, Global.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.SetPixels(pixels);

		m_texture.Apply();
		GetComponent<RawImage>().texture = m_texture;
	}

	public void thingToEditChanged(InputField thingNameField)
    {
		GameObject prefab = VoxelObjectCache.instance().getPrefab(thingNameField.text);
		if (prefab == null) {
			print("Could not find prefab!");
			return;
		}

		VoxelObject[] voxelObjects = prefab.GetComponentsInChildren<VoxelObject>(true);
		if (voxelObjects.Length == 0) {
			print("Could not find any voxel objects in prefab!");
			return;
		}

		for (int i = 0; i < voxelObjects.Length; ++i) {
			string index = voxelObjects[0].index;
			int atlasIndex = voxelObjects[i].atlasIndex();
			print("found index: " + index + ", " + atlasIndex);
			if (atlasIndex < 0)
				continue;

			setIndex(atlasIndex);
			break;
		}
    }

    public void save()
    {
		if (m_atlasIndex < 0)
			return;
		
		int atlasPixelX;
		int atlasPixelY;
		Global.atlasPixelForIndex(m_atlasIndex, out atlasPixelX, out atlasPixelY);
		atlas.SetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight, m_texture.GetPixels());
		atlas.Apply();
    }
}
