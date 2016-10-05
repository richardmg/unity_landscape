using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ThingPainter : MonoBehaviour {
	public Texture2D atlas;
	public Color color = Color.black;

	Texture2D m_texture;
	GameObject m_prefab;
	VoxelObject m_topLevelVoxelObject;
	int m_currentListIndex;
	List<VoxelObject> m_voxelObjectsWithAtlasIndexList;

	void OnEnable ()
    {
		// Use scale to enlarge the UI instead of the rect. At least not both.
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.x == Root.kSubImageWidth);
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.y == Root.kSubImageHeight);
		color = Root.instance.uiManager.colorPicker.GetComponentInChildren<ColorPicker>().selectedColor;
	}

	void Update ()
    {
		if (m_topLevelVoxelObject == null)
			return;
		if (!Input.GetMouseButton(0))
			return;

		Vector2 uv = UIManager.getMousePosOnImage(GetComponent<RawImage>());
		if (!UIManager.isInside(uv))
			return;

        int pixelX = (int)(uv.x * Root.kSubImageWidth);
        int pixelY = (int)(uv.y * Root.kSubImageHeight);

		m_texture.SetPixel(pixelX, pixelY, color);
		m_texture.Apply();
	}

	void setCurrentListIndex(int listIndex)
    {
		m_currentListIndex = listIndex;
		int atlasIndex = m_voxelObjectsWithAtlasIndexList[listIndex].resolvedIndex();

		int atlasPixelX, atlasPixelY;
		Root.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		var pixels = atlas.GetPixels(atlasPixelX, atlasPixelY, Root.kSubImageWidth, Root.kSubImageHeight);

		m_texture = new Texture2D(Root.kSubImageWidth, Root.kSubImageHeight, TextureFormat.ARGB32, false);
		m_texture.filterMode = FilterMode.Point;
		m_texture.SetPixels(pixels);

		m_texture.Apply();
		GetComponent<RawImage>().texture = m_texture;
	}

	public void onIndexFieldEndInput(InputField indexField)
    {
		m_prefab = Root.getPrefab(indexField.text);
		if (m_prefab == null) {
			print("Could not find prefab!");
			return;
		}

		m_voxelObjectsWithAtlasIndexList = new List<VoxelObject>();
		VoxelObject[] voxelObjects = m_prefab.GetComponentsInChildren<VoxelObject>(true);

		for (int i = 0; i < voxelObjects.Length; ++i) {
			int atlasIndex = voxelObjects[i].resolvedIndex();
			if (atlasIndex >= 0) {
				// Only add unique faces
				bool unique = true;
				for (int v = 0; v < m_voxelObjectsWithAtlasIndexList.Count; ++v) {
					if (m_voxelObjectsWithAtlasIndexList[v].resolvedIndex() == atlasIndex) {
						unique = false;
						break;
					}
				}
				if (unique)
					m_voxelObjectsWithAtlasIndexList.Add(voxelObjects[i]);
			}
		}

		if (m_voxelObjectsWithAtlasIndexList.Count == 0) {
			print("Could not find any non-toplevel voxel objects in prefab!");
			return;
		}

		m_topLevelVoxelObject = voxelObjects[0];
		setCurrentListIndex(0);
    }

	public void onPrevButtonClicked()
	{
		if (m_topLevelVoxelObject == null)
			return;

		int index = m_currentListIndex - 1;
		if (index < 0)
			index = m_voxelObjectsWithAtlasIndexList.Count - 1;
		setCurrentListIndex(index);
	}

	public void onNextButtonClicked()
	{
		if (m_topLevelVoxelObject == null)
			return;

		int index = m_currentListIndex + 1;
		if (index >= m_voxelObjectsWithAtlasIndexList.Count)
			index = 0;
		setCurrentListIndex(index);
	}

	public void onSaveButtonClicked()
    {
		if (m_topLevelVoxelObject == null)
			return;

		int atlasIndex = m_voxelObjectsWithAtlasIndexList[m_currentListIndex].resolvedIndex();

		int atlasPixelX;
		int atlasPixelY;
		Root.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		atlas.SetPixels(atlasPixelX, atlasPixelY, Root.kSubImageWidth, Root.kSubImageHeight, m_texture.GetPixels());
		atlas.Apply();

		Root.instance.meshManager.clearCache(m_topLevelVoxelObject.name);
		Root.instance.notificationManager.notifyPrefabChanged(m_prefab);
    }

	public void onColorButtonClicked()
	{
		// TODO: use lambda callback
		Root.instance.uiManager.push(Root.instance.uiManager.colorPicker, (bool accepted) => { print("color selected"); });
	}
}
