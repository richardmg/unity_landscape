using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using EditMode = System.Int32;

public class ThingPainter : MonoBehaviour {
	public Texture2D atlas;
	public Color color = Color.black;

	Texture2D m_texture;
	EntityClass m_entityClass;
	int m_currentListIndex;
	List<VoxelObject> m_uniqueVoxelObjects;
	EditMode m_currentMode = kPaintMode;
	bool m_textureDirty = false;
	bool m_clearToggleOn = false;

	const EditMode kPaintMode = 0;
	const EditMode kColorSelectMode = 1;

	void OnEnable()
    {
		if (m_entityClass != null)
			return;

		// Use scale to enlarge the UI instead of the rect. At least not both.
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.x == Root.kSubImageWidth);
		Debug.Assert(GetComponent<RectTransform>().sizeDelta.y == Root.kSubImageHeight);

		setCurrentEntityClass(Root.instance.player.currentEntityClass);
	}

	void OnDisable()
	{
		saveChanges();
	}

	void Update()
    {
		if (!Root.instance.uiManager.grabMouse(this))
			return;
		if (m_entityClass == null)
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

	void setCurrentEntityClass(EntityClass entityClass)
	{
		m_entityClass = entityClass;
		m_uniqueVoxelObjects = entityClass.getUniqueVoxelObjects();
		setCurrentListIndex(0);
	}

	void setCurrentListIndex(int listIndex)
    {
		m_currentListIndex = listIndex;
		int atlasIndex = m_uniqueVoxelObjects[listIndex].atlasIndex;

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
//		setCurrentEntityClass(new EntityClass(indexField.text));
    }

	public void onPrevButtonClicked()
	{
		int index = m_currentListIndex - 1;
		if (index < 0)
			index = m_uniqueVoxelObjects.Count - 1;
		saveChanges();
		setCurrentListIndex(index);
	}

	public void onNextButtonClicked()
	{
		int index = m_currentListIndex + 1;
		if (index >= m_uniqueVoxelObjects.Count)
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

		int atlasIndex = m_uniqueVoxelObjects[m_currentListIndex].atlasIndex;

		int atlasPixelX;
		int atlasPixelY;
		Root.atlasPixelForIndex(atlasIndex, out atlasPixelX, out atlasPixelY);
		atlas.SetPixels(atlasPixelX, atlasPixelY, Root.kSubImageWidth, Root.kSubImageHeight, m_texture.GetPixels());
		atlas.Apply();

		m_textureDirty = false;

		m_entityClass.markDirty(EntityClass.DirtyFlags.Mesh);
		Root.instance.notificationManager.notifyEntityClassChanged(m_entityClass);
    }

	public void onColorButtonClicked()
	{
		GameObject colorPicker = Root.instance.uiManager.colorPickerGO;
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
