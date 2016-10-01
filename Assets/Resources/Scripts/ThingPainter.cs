using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ThingPainter : MonoBehaviour {
	public Texture2D atlas;
	public Color color = Color.black;

	Texture2D m_texture;
	int m_atlasStartIndex = VoxelObject.kUnknown;
	int m_atlasEditIndex = VoxelObject.kUnknown;
	VoxelObject[] m_voxelObjects;

	void OnEnable ()
    {
		// Use scale to enlarge the UI instead of the rect. At least not both.
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.x == Global.kSubImageWidth);
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.y == Global.kSubImageHeight);
	}

	void Update ()
    {
		if (m_atlasStartIndex < 0)
			return;
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

	void setEditIndex(int atlasIndex)
    {
		m_atlasEditIndex = atlasIndex;

		int atlasPixelX, atlasPixelY;
		Global.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		var pixels = atlas.GetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight);

		m_texture = new Texture2D(Global.kSubImageWidth, Global.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.SetPixels(pixels);

		m_texture.Apply();
		GetComponent<RawImage>().texture = m_texture;
	}

	public void onIndexFieldEndInput(InputField indexField)
    {
		GameObject prefab = VoxelObjectCache.instance().getPrefab(indexField.text);
		if (prefab == null) {
			print("Could not find prefab!");
			return;
		}

		m_voxelObjects = prefab.GetComponentsInChildren<VoxelObject>(true);
		if (m_voxelObjects.Length == 0) {
			print("Could not find any voxel objects in prefab!");
			return;
		}

		for (int i = 0; i < m_voxelObjects.Length; ++i) {
			int atlasIndex = m_voxelObjects[i].atlasIndex();
			if (atlasIndex < 0)
				continue;

			m_atlasStartIndex = atlasIndex;
			setEditIndex(atlasIndex);
			break;
		}
    }

	public void onPrevButtonClicked()
	{
		int index = m_atlasEditIndex - 1;
		if (index < m_atlasStartIndex)
			index = m_atlasStartIndex + m_voxelObjects.Length - 1;
		setEditIndex(index);
	}

	public void onNextButtonClicked()
	{
		int index = m_atlasEditIndex + 1;
		if (index >= m_atlasStartIndex + m_voxelObjects.Length - 1)
			index = m_atlasStartIndex;
		setEditIndex(index);
	}

	public void onSaveButtonClicked()
    {
		if (m_atlasStartIndex < 0)
			return;
		
		int atlasPixelX;
		int atlasPixelY;
		Global.atlasPixelForIndex(m_atlasEditIndex, out atlasPixelX, out atlasPixelY);
		atlas.SetPixels(atlasPixelX, atlasPixelY, Global.kSubImageWidth, Global.kSubImageHeight, m_texture.GetPixels());
		atlas.Apply();
    }
}
