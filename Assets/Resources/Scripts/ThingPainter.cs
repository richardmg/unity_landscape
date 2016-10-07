using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using EditMode = System.Int32;

public class ThingPainter : MonoBehaviour {
	public Texture2D atlas;
	public Color color = Color.black;

	Texture2D m_texture;
	GameObject m_prefab;
	VoxelObject m_topLevelVoxelObject;
	int m_currentListIndex;
	List<VoxelObject> m_voxelObjectsWithAtlasIndexList;
	EditMode m_currentMode = kPaintMode;
	bool m_textureDirty = false;
	bool m_clearToggleOn = false;

	const EditMode kPaintMode = 0;
	const EditMode kColorSelectMode = 1;

	void OnEnable()
    {
		// Use scale to enlarge the UI instead of the rect. At least not both.
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.x == Root.kSubImageWidth);
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.y == Root.kSubImageHeight);

		setCurrentPrefabVariant(Root.instance.player.currentPrefabVariant);
	}

	void OnDisable()
	{
		saveChanges();
	}

	void Update()
    {
		if (!Root.instance.uiManager.grabMouse(this))
			return;
		if (m_topLevelVoxelObject == null)
			return;
		if (!Input.GetMouseButton(0))
			return;

		Vector2 uv = UIManager.getMousePosOnImage(GetComponent<RawImage>());

		if (Input.GetKey(KeyCode.C) || m_currentMode == kColorSelectMode)
			updateColorSelect(uv);
		else if (m_currentMode == kPaintMode)
			updatePaint(uv);
	}

	void updatePaint(Vector2 uv)
	{
		if (!UIManager.isInside(uv))
			return;

		int pixelX = (int)(uv.x * m_texture.width);
		int pixelY = (int)(uv.y * m_texture.height);

		if (m_clearToggleOn || Input.GetKey(KeyCode.LeftShift))
			m_texture.SetPixel(pixelX, pixelY, Color.clear);
		else
			m_texture.SetPixel(pixelX, pixelY, color);

		m_texture.Apply();
		m_textureDirty = true;
	}

	void updateColorSelect(Vector2 uv)
	{
		if (!UIManager.isInside(uv))
			return;

		int pixelX = (int)(uv.x * m_texture.width);
		int pixelY = (int)(uv.y * m_texture.height);
		color = m_texture.GetPixel(pixelX, pixelY);

		m_currentMode = kPaintMode;
	}

	void setCurrentPrefabVariant(PrefabVariant prefabVariant)
	{
		m_prefab = Root.getPrefab(prefabVariant.prefabName);
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

		m_textureDirty = false;
	}

	public void onIndexFieldEndInput(InputField indexField)
    {
		saveChanges();
		setCurrentPrefabVariant(new PrefabVariant(indexField.text));
    }

	public void onPrevButtonClicked()
	{
		if (m_topLevelVoxelObject == null)
			return;

		int index = m_currentListIndex - 1;
		if (index < 0)
			index = m_voxelObjectsWithAtlasIndexList.Count - 1;
		saveChanges();
		setCurrentListIndex(index);
	}

	public void onNextButtonClicked()
	{
		if (m_topLevelVoxelObject == null)
			return;

		int index = m_currentListIndex + 1;
		if (index >= m_voxelObjectsWithAtlasIndexList.Count)
			index = 0;
		saveChanges();
		setCurrentListIndex(index);
	}

	public void onDiscardButtonClicked()
	{
		setCurrentListIndex(m_currentListIndex);
	}

	public void saveChanges()
    {
		if (m_texture == null || !m_textureDirty)
			return;

		int atlasIndex = m_voxelObjectsWithAtlasIndexList[m_currentListIndex].resolvedIndex();

		int atlasPixelX;
		int atlasPixelY;
		Root.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		atlas.SetPixels(atlasPixelX, atlasPixelY, Root.kSubImageWidth, Root.kSubImageHeight, m_texture.GetPixels());
		atlas.Apply();

		m_textureDirty = false;

		Root.instance.meshManager.clearCache(m_topLevelVoxelObject.name);
		Root.instance.notificationManager.notifyPrefabChanged(m_prefab);
    }

	public void onColorButtonClicked()
	{
		GameObject colorPicker = Root.instance.uiManager.colorPicker;
		Root.instance.uiManager.push(colorPicker, (bool accepted) => {
			if (accepted)
				color = colorPicker.GetComponentInChildren<ColorPicker>().selectedColor;
		});
	}

	public void onColorSamplerButtonClicked()
	{
		m_currentMode = kColorSelectMode;
		Root.instance.uiManager.clearMouseGrab();
	}

	public void onEraseToggleClicked(Toggle toggle)
	{
		m_clearToggleOn = toggle.isOn;
	}

	public void onClearButtonClicked()
	{
		for (int x = 0; x < m_texture.width; ++x) {
			for (int y = 0; y < m_texture.height; ++y) {
				m_texture.SetPixel(x, y, Color.clear);
			}
		}

		m_texture.Apply();
		m_textureDirty = true;
	}

}
